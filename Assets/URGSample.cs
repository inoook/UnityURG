using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// http://sourceforge.net/p/urgnetwork/wiki/top_jp/
// https://www.hokuyo-aut.co.jp/02sensor/07scanner/download/pdf/URG_SCIP20.pdf
public class URGSample : MonoBehaviour {

	class DetectObject
	{
		public List<long> distList;
		public List<int> idList;

		public long startDist;

		public DetectObject()
		{
			distList = new List<long>();
			idList = new List<int>();
		}
	}

	List<DetectObject> detectObjects;
	List<int> detectIdList;

	private Vector3[] directions;
	private bool cached = false;

	UrgDeviceEthernet urg;
	public float scale = 0.1f;
	public float limit = 300.0f;//mm
	public int noiseLimit = 5;

	public Color distanceColor = Color.white;
//	public Color detectColor = Color.white;
	public Color strengthColor = Color.white;

	public Color[] groupColors;

	List<long> distances;
	List<long> strengths;

	public Rect areaRect;

	public bool debugDraw = false;

	int drawCount;

	// Use this for initialization
	void Start () {
		distances = new List<long>();
		strengths = new List<long>();

		urg = this.gameObject.AddComponent<UrgDeviceEthernet>();
		urg.StartTCP();
	}
	
	// Update is called once per frame
	void Update () {
		
		if(gd_loop){
			urg.Write(SCIP_library.SCIP_Writer.GD(0, 1080));
		}

		// center offset rect
		Rect detectAreaRect = areaRect;
		detectAreaRect.x *= scale;
		detectAreaRect.y *= scale;
		detectAreaRect.width *= scale;
		detectAreaRect.height *= scale;
		
		detectAreaRect.x = -detectAreaRect.width / 2;
		//

		float d = Mathf.PI * 2 / 1440;
		float offset = d * 540;

		// cache directions
		if(urg.distances.Count > 0 ){
			if(!cached){
				directions = new Vector3[urg.distances.Count];
				for(int i = 0; i < directions.Length; i++){
					float a = d * i + offset;
					directions[i] = new Vector3(-Mathf.Cos(a), -Mathf.Sin(a), 0);
				}
				cached = true;
			}
		}

		// strengths
		try{
			if(urg.strengths.Count > 0){
				strengths.Clear();
				strengths.AddRange(urg.strengths);
			}
		}catch{
		}
		// distances
		try{
			if(urg.distances.Count > 0){
				distances.Clear();
				distances.AddRange(urg.distances);
			}
		}catch{
		}
//		List<long> distances = urg.distances;

		if(debugDraw){
			// strengths
			for(int i = 0; i < strengths.Count; i++){
				//float a = d * i + offset;
				//Vector3 dir = new Vector3(-Mathf.Cos(a), -Mathf.Sin(a), 0);
				Vector3 dir = directions[i];
				long dist = strengths[i];
				Debug.DrawRay(Vector3.zero, Mathf.Abs( dist ) * dir * scale, strengthColor);
			}

			// distances
//			float colorD = 1.0f / 1440;
			for(int i = 0; i < distances.Count; i++){
				//float a = d * i + offset;
				//Vector3 dir = new Vector3(-Mathf.Cos(a), -Mathf.Sin(a), 0);
				Vector3 dir = directions[i];
				long dist = distances[i];
				//color = (dist < limit && dir.y > 0) ? detectColor : new Color(colorD * i, 0,0,1.0f);
				//Color color = (dist < limit && dir.y > 0) ? detectColor : distanceColor;
//				Debug.DrawRay(Vector3.zero, dist * dir * scale, color);
				Debug.DrawRay(Vector3.zero, dist * dir * scale, distanceColor);
			}
		}

		//-----------------
		//  group
		detectObjects = new List<DetectObject>();
		//
		//------
//		bool endGroup = true;
//		for(int i = 0; i < distances.Count; i++){
//			int id = i;
//			long dist = distances[id];
//
//			float a = d * i + offset;
//			Vector3 dir = new Vector3(-Mathf.Cos(a), -Mathf.Sin(a), 0);
//
//			if(dist < limit && dir.y > 0){
//				DetectObject detect;
//				if(endGroup){
//					detect = new DetectObject();
//					detect.idList.Add(id);
//					detect.distList.Add(dist);
//
//					detect.startDist = dist;
//					detectObjects.Add(detect);
//					
//					endGroup = false;
//				}else{
//					detect = detectObjects[detectObjects.Count-1];
//					detect.idList.Add(id);
//					detect.distList.Add(dist);
//
//					if(dist > detect.startDist){
//						endGroup = true;
//					}
//				}
//			}else{
//				endGroup = true;
//			}
//		}

		//------
//		bool endGroup = true;
//		for(int i = 1; i < distances.Count-1; i++){
//			long dist = distances[i];
//			float delta = Mathf.Abs((float)(distances[i] - distances[i-1]));
//			float delta1 = Mathf.Abs((float)(distances[i+1] - distances[i]));
//			
//			float a = d * i + offset;
//			Vector3 dir = new Vector3(-Mathf.Cos(a), -Mathf.Sin(a), 0);
//			
//			if(dir.y > 0){
//				DetectObject detect;
//				if(endGroup){
//					if(dist < limit && delta > 50){
//						detect = new DetectObject();
//						detect.idList.Add(i);
//						detect.distList.Add(dist);
//						
//						detect.startDist = dist;
//						detectObjects.Add(detect);
//						
//						endGroup = false;
//					}
//				}else{
//					if(delta < 50){
//						detect = detectObjects[detectObjects.Count-1];
//						detect.idList.Add(i);
//						detect.distList.Add(dist);
//					}else{
//						endGroup = true;
//					}
//				}
//			}
//		}


		//------
		bool endGroup = true;
		float deltaLimit = 100; // 認識の閾値　連続したもののみを取得するため (mm)
		for(int i = 1; i < distances.Count-1; i++){
			//float a = d * i + offset;
			//Vector3 dir = new Vector3(-Mathf.Cos(a), -Mathf.Sin(a), 0);
			Vector3 dir = directions[i];
			long dist = distances[i];
			float delta = Mathf.Abs((float)(distances[i] - distances[i-1]));
			float delta1 = Mathf.Abs((float)(distances[i+1] - distances[i]));
				
			if(dir.y > 0){
				DetectObject detect;
				if(endGroup){
					Vector3 pt = dist * dir * scale;
					if(dist < limit && (delta < deltaLimit && delta1 < deltaLimit)){
//					bool isArea = detectAreaRect.Contains(pt);
//					if(isArea && (delta < deltaLimit && delta1 < deltaLimit)){
						detect = new DetectObject();
						detect.idList.Add(i);
						detect.distList.Add(dist);
						
						detect.startDist = dist;
						detectObjects.Add(detect);
						
						endGroup = false;
					}
				}else{
					if(delta1 >= deltaLimit || delta >= deltaLimit){
						endGroup = true;
					}else{
						detect = detectObjects[detectObjects.Count-1];
						detect.idList.Add(i);
						detect.distList.Add(dist);
					}
				}
			}
		}

		//-----------------
		// draw 
		drawCount = 0;
		for(int i = 0; i < detectObjects.Count; i++){
			DetectObject detect = detectObjects[i];

			// noise
			if(detect.idList.Count < noiseLimit){
				continue;
			}

			int offsetCount = detect.idList.Count / 3;
			int avgId = 0;
			for(int n = 0; n < detect.idList.Count; n++){
				avgId += detect.idList[n];
			}
			avgId = avgId / (detect.idList.Count);

			long avgDist = 0;
			for(int n = offsetCount; n < detect.distList.Count - offsetCount; n++){
				avgDist += detect.distList[n];
			}
			avgDist = avgDist / (detect.distList.Count - offsetCount * 2);

			//float a = d * avgId + offset;
			//Vector3 dir = new Vector3(-Mathf.Cos(a), -Mathf.Sin(a), 0);
			Vector3 dir = directions[avgId];
			long dist = avgDist;


			//float a0 = d * detect.idList[offsetCount] + offset;
			//Vector3 dir0 = new Vector3(-Mathf.Cos(a0), -Mathf.Sin(a0), 0);
			int id0 = detect.idList[offsetCount];
			Vector3 dir0 = directions[id0];
			long dist0 = detect.distList[offsetCount];

			//float a1 = d * detect.idList[detect.idList.Count-1 - offsetCount] + offset;
			//Vector3 dir1 = new Vector3(-Mathf.Cos(a1), -Mathf.Sin(a1), 0);
			int id1 = detect.idList[detect.idList.Count-1 - offsetCount];
			Vector3 dir1 = directions[id1];
			long dist1 = detect.distList[detect.distList.Count-1 - offsetCount];

			Color gColor;
			if(drawCount < groupColors.Length){
				gColor = groupColors[drawCount];
			}else{
				gColor = Color.green;
			}
			for(int j = offsetCount; j < detect.idList.Count - offsetCount; j++){
				//float _a = d * detect.idList[j] + offset;
				//Vector3 _dir = new Vector3(-Mathf.Cos(_a), -Mathf.Sin(_a), 0);
				int _id = detect.idList[j];
				Vector3 _dir = directions[_id];
				long _dist = detect.distList[j];
				Debug.DrawRay(Vector3.zero, _dist * _dir * scale, gColor);
			}

			Debug.DrawLine(dist0 * dir0 * scale, dist1 * dir1 * scale, gColor);
			Debug.DrawRay(Vector3.zero, dist * dir * scale, Color.green);

			drawCount++;
		}

		DrawRect(detectAreaRect, Color.green);
	}
	void DrawRect(Rect rect, Color color)
	{
		Vector3 p0 = new Vector3(rect.x, rect.y, 0);
		Vector3 p1 = new Vector3(rect.x + rect.width, rect.y, 0);
		Vector3 p2 = new Vector3(rect.x + rect.width, rect.y + rect.height, 0);
		Vector3 p3 = new Vector3(rect.x, rect.y + rect.height, 0);
		Debug.DrawLine(p0, p1, color);
		Debug.DrawLine(p1, p2, color);
		Debug.DrawLine(p2, p3, color);
		Debug.DrawLine(p3, p0, color);
	}

	private bool gd_loop = false;

	// PP
//	MODL ... センサ型式情報
//	DMIN ... 最小計測可能距離 (mm)
//	DMAX ... 最大計測可能距離 (mm)
//	ARES ... 角度分解能(360度の分割数)
//	AMIN ... 最小計測可能方向値
//	AMAX ... 最大計測可能方向値
//	AFRT ... 正面方向値
//	SCAN ... 標準操作角速度

	void OnGUI()
	{
		if(GUILayout.Button("VV")){
			urg.Write(SCIP_library.SCIP_Writer.VV());
		}
//		if(GUILayout.Button("SCIP2")){
//			urg.Write(SCIP_library.SCIP_Writer.SCIP2());
//		}
		if(GUILayout.Button("PP")){
			urg.Write(SCIP_library.SCIP_Writer.PP());
		}
		if(GUILayout.Button("MD: (計測＆送信要求)")){
			urg.Write(SCIP_library.SCIP_Writer.MD(0, 1080, 1, 0, 0));
		}
		if(GUILayout.Button("ME: (計測＆距離データ・受光強度値送信要求)")){
			urg.Write(SCIP_library.SCIP_Writer.ME(0, 1080, 1, 1, 0));
		}
		if(GUILayout.Button("BM")){
			urg.Write(SCIP_library.SCIP_Writer.BM());
		}
		if(GUILayout.Button("GD: (計測済み距離データ送信要求)")){
			urg.Write(SCIP_library.SCIP_Writer.GD(0, 1080));
		}
		if(GUILayout.Button("GD_loop")){
			gd_loop = !gd_loop;
		}
		if(GUILayout.Button("QUIT")){
			urg.Write(SCIP_library.SCIP_Writer.QT());
		}

		GUILayout.Label("distances.Count: "+distances.Count + " / strengths.Count: "+strengths.Count);
		GUILayout.Label("drawCount: "+drawCount + " / detectObjects: "+detectObjects.Count);
	}


}

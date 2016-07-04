using UnityEngine;
using System.Collections;

public class UrgDevice : MonoBehaviour {

	public enum CMD
	{
		// https://www.hokuyo-aut.jp/02sensor/07scanner/download/pdf/URG_SCIP20.pdf
		VV, PP, II, // センサ情報要求コマンド(3 種類)  
		BM, QT, //計測開始・終了コマンド
		MD, GD, // 距離要求コマンド(2 種類) 
		ME //距離・受光強度要求コマンド 
	}

	public static string GetCMDString(CMD cmd)
	{
		return cmd.ToString();
	}
}

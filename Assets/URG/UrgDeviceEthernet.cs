/*!
 * \file
 * \brief Get distance data from Ethernet type URG
 * \author Jun Fujimoto
 * $Id: get_distance_ethernet.cs 403 2013-07-11 05:24:12Z fujimoto $
 */
using UnityEngine;
using System.Threading;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using SCIP_library;

public class UrgDeviceEthernet : UrgDevice
{
//	private Thread listenThread;
	private Thread clientThread;
	TcpClient tcpClient;
	
	public List<long> distances;
	public List<long> strengths;

//	private Queue messageQueue;
	
	private string ip_address = "192.168.0.10";
	private int port_number = 10940;

	public void StartTCP(string ip = "192.168.0.10", int port = 10940)
    {
//		messageQueue = Queue.Synchronized(new Queue());

		ip_address = ip;
		port_number = port;

		distances = new List<long>();
		strengths = new List<long>();

        try {
            tcpClient = new TcpClient();
            tcpClient.Connect(ip_address, port_number);
			
			Debug.Log("Connect setting = IP Address : " + ip_address + " Port number : " + port_number.ToString());
            
//			this.listenThread = new Thread(new ThreadStart(ListenForClients));
//			this.listenThread.Start();

			ListenForClients();
        } catch (Exception ex) {
            Debug.Log(ex.Message);
        } finally {

        }
    }

	void OnDisable()
	{
		DeInit();
	}
	void OnApplicationQuit()
	{
		DeInit();
	}
	
	void DeInit()
	{
		if(tcpClient != null){
			if( tcpClient.Connected ){
				NetworkStream stream = tcpClient.GetStream();
				if(stream != null){
					stream.Close();
				}
			}
			tcpClient.Close();
		}
		
		if(this.clientThread != null){
			this.clientThread.Abort();
		}
	}

	public void Write(string scip)
	{
		NetworkStream stream = tcpClient.GetStream();
		write(stream, scip);
	}

	private void ListenForClients()
	{
		clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
		clientThread.Start(tcpClient);
	}
	private void HandleClientComm(object obj)
	{
		try
		{
			using (TcpClient client = (TcpClient)obj)
			{
				using (NetworkStream stream = client.GetStream())
				{
//					NetworkStream clientStream = client.GetStream();
					while (true)
					{
						long time_stamp = 0;
						string receive_data = read_line(stream);
//						messageQueue.Enqueue( receive_data );

						string cmd = GetCommand(receive_data);
						if(cmd == GetCMDString(CMD.MD)){
							distances.Clear();
							SCIP_Reader.MD(receive_data, ref time_stamp, ref distances);
						}else if(cmd == GetCMDString(CMD.ME)){
							distances.Clear();
							strengths.Clear();
							SCIP_Reader.ME(receive_data, ref time_stamp, ref distances, ref strengths);
						}else{
							Debug.Log(">>"+receive_data);
						}
					}
//					client.Close();
				}
			}
		} catch (System.Exception ex) {
			Debug.LogWarning("error: "+ex);
		}
	}

	string GetCommand(string get_command)
	{
		string[] split_command = get_command.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
		return split_command[0].Substring(0, 2);
	}
	
	bool CheckCommand(string get_command, string cmd)
	{
		string[] split_command = get_command.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
		return split_command[0].StartsWith(cmd);
	}

//	void Update()
//	{
//		lock(messageQueue.SyncRoot){
//			if(messageQueue.Count > 0){
//				string receive_data = messageQueue.Dequeue().ToString();
//				long time_stamp;
//				if(CheckCommand(receive_data, "MD")){
//					distances.Clear();
//					time_stamp = 0;
//
//					SCIP_Reader.MD(receive_data, ref time_stamp, ref distances);
//					//Debug.Log("time stamp: " + time_stamp.ToString() + " / count: "+distances.Count);
//				}else if(CheckCommand(receive_data, "GD")){
//					distances.Clear();
//					time_stamp = 0;
//
//					SCIP_Reader.GD(receive_data, ref time_stamp, ref distances);
//				}else{
//					Debug.Log(">>"+receive_data);
//				}
//			}
//		}
//		
//	}


    /// <summary>
    /// Read to "\n\n" from NetworkStream
    /// </summary>
    /// <returns>receive data</returns>
    static string read_line(NetworkStream stream)
    {
        if (stream.CanRead) {
            StringBuilder sb = new StringBuilder();
            bool is_NL2 = false;
            bool is_NL = false;
            do {
                char buf = (char)stream.ReadByte();
                if (buf == '\n') {
                    if (is_NL) {
                        is_NL2 = true;
                    } else {
                        is_NL = true;
                    }
                } else {
                    is_NL = false;
                }
                sb.Append(buf);
            } while (!is_NL2);

            return sb.ToString();
        } else {
            return null;
        }
    }

    /// <summary>
    /// write data
    /// </summary>
    static bool write(NetworkStream stream, string data)
    {
        if (stream.CanWrite) {
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            stream.Write(buffer, 0, buffer.Length);
            return true;
        } else {
            return false;
        }
    }
}
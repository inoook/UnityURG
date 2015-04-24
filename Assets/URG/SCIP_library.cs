// http://sourceforge.net/p/urgnetwork/wiki/cs_sample_jp/

using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace SCIP_library
{
    public class SCIP_Writer
    {
		public static string END
		{
			get{ return "\n"; }
		}

        /// <param name="start">measurement start step</param>
        /// <param name="end">measurement end step</param>
        /// <param name="grouping">grouping step number</param>
        /// <param name="skips">skip scan number</param>
        /// <param name="scans">get scan numbar</param>
        /// <returns>created command</returns>
        public static string MD(int start, int end, int grouping = 1, int skips = 0, int scans = 0)
        {
			return UrgDevice.GetCMDString(UrgDevice.CMD.MD) + start.ToString("D4") + end.ToString("D4") + grouping.ToString("D2") + skips.ToString("D1") + scans.ToString("D2") + END;
        }
		public static string ME(int start, int end, int grouping = 1, int skips = 0, int scans = 0)
		{
			return UrgDevice.GetCMDString(UrgDevice.CMD.ME) + start.ToString("D4") + end.ToString("D4") + grouping.ToString("D2") + skips.ToString("D1") + scans.ToString("D2") + END;
		}

		public static string BM()
		{
			return UrgDevice.GetCMDString(UrgDevice.CMD.BM) + END;
		}
		public static string GD(int start, int end, int grouping = 1)
		{
			return UrgDevice.GetCMDString(UrgDevice.CMD.GD) + start.ToString("D4") + end.ToString("D4") + grouping.ToString("D2") + END;
		}
		
		public static string VV()
        {
			return UrgDevice.CMD.VV.ToString() + END;
        }

        public static string II()
        {
			return UrgDevice.GetCMDString(UrgDevice.CMD.II) + END;
        }

        public static string PP()
        {
			return UrgDevice.GetCMDString(UrgDevice.CMD.PP) + END;
        }

        public static string SCIP2()
        {
			return "SCIP2.0" + END;
        }

        public static string QT()
        {
			return UrgDevice.GetCMDString(UrgDevice.CMD.QT) + END;
        }
    }

    public class SCIP_Reader
    {
        /// <summary>
        /// read MD command
        /// </summary>
        /// <param name="get_command">received command</param>
        /// <param name="time_stamp">timestamp data</param>
        /// <param name="distances">distance data</param>
        /// <returns>is successful</returns>
        public static bool MD(string get_command, ref long time_stamp, ref List<long> distances)
        {
            //distances.Clear();
            string[] split_command = get_command.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

//            if (!split_command[0].StartsWith("MD")) {
//                return false;
//            }

            if (split_command[1].StartsWith("00")) {
                return true;
            } else if (split_command[1].StartsWith("99")) {
                time_stamp = SCIP_Reader.decode(split_command[2], 4);
                distance_data(split_command, 3, ref distances);
                return true;
            } else {
                return false;
            }
        }

		public static bool GD(string get_command, ref long time_stamp, ref List<long> distances)
		{
			//distances.Clear();
			string[] split_command = get_command.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			
//			if (!split_command[0].StartsWith("GD")) {
//				return false;
//			}

			if (split_command[1].StartsWith("00")) {
				time_stamp = SCIP_Reader.decode(split_command[2], 4);
				distance_data(split_command, 3, ref distances);
				return true;
			} else {
				return false;
			}
		}
	
		/// <summary>
		/// read distance data
		/// </summary>
		/// <param name="lines"></param>
		/// <param name="start_line"></param>
		/// <returns></returns>
        public static bool distance_data(string[] lines, int start_line, ref List<long> distances)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = start_line; i < lines.Length; ++i) {
                sb.Append(lines[i].Substring(0, lines[i].Length - 1));
            }
            return SCIP_Reader.decode_array(sb.ToString(), 3, ref distances);
        }

        /// <summary>
        /// decode part of string 
        /// </summary>
        /// <param name="data">encoded string</param>
        /// <param name="size">encode size</param>
        /// <param name="offset">decode start position</param>
        /// <returns>decode result</returns>
        public static long decode(string data, int size, int offset = 0)
        {
            long value = 0;

            for (int i = 0; i < size; ++i) {
                value <<= 6;
                value |= (long)data[offset + i] - 0x30;
            }

            return value;
        }

        /// <summary>
        /// decode multiple data
        /// </summary>
        /// <param name="data">encoded string</param>
        /// <param name="size">encode size</param>
        /// <returns>decode result</returns>
        public static bool decode_array(string data, int size, ref List<long> decoded_data)
        {
            for (int pos = 0; pos <= data.Length - size; pos += size) {
                decoded_data.Add(decode(data, size, pos));
            }
            return true;
        }

		
		public static bool ME(string get_command, ref long time_stamp, ref List<long> distances, ref List<long> strengths)
		{
			string[] split_command = get_command.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			
//			if (!split_command[0].StartsWith("ME")) {
//				return false;
//			}
			
			if (split_command[1].StartsWith("00")) {
				return true;
			} else if (split_command[1].StartsWith("99")) {
				time_stamp = SCIP_Reader.decode(split_command[2], 4);
				distance_strength_data(split_command, 3, ref distances, ref strengths);
				return true;
			} else {
				return false;
			}
		}

		public static bool distance_strength_data(string[] lines, int start_line, ref List<long> distances, ref List<long> strengths)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = start_line; i < lines.Length; ++i) {
				sb.Append(lines[i].Substring(0, lines[i].Length - 1));
			}
			return SCIP_Reader.decode_array(sb.ToString(), 3, ref distances, ref strengths);
		}
		public static bool decode_array(string data, int size, ref List<long> decoded_data, ref List<long> stdecoded_data)
		{
			for (int pos = 0; pos <= data.Length - size * 2; pos += size * 2) {
				decoded_data.Add(decode(data, size, pos));
				stdecoded_data.Add(decode(data, size, pos + size));
			}
			return true;
		}
    }
}

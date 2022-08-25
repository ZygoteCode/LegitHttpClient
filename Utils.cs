namespace LegitHttpClient
{
	using System.Collections.Generic;
	using System.IO;
	using System.Net.Sockets;
	using System;
	using System.Diagnostics;
	using System.Text;
	using System.Net;
	using System.Reflection;

	public class Utils
	{
		public static IEnumerable<string> SplitToLines(string input)
		{
			if (input == null)
			{
				yield break;
			}

			using (System.IO.StringReader reader = new System.IO.StringReader(input))
			{
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					yield return line;
				}
			}
		}

		public static byte[] Combine(byte[] first, byte[] second)
		{
			byte[] ret = new byte[first.Length + second.Length];

			Buffer.BlockCopy(first, 0, ret, 0, first.Length);
			Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);

			return ret;
		}

		public static byte[] ReadFully(Stream input)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				input.CopyTo(ms);
				return ms.ToArray();
			}
		}

		public static TcpClient ConnectionProxy(string targetHost, int targetPort, string proxyUrl, string proxyUserName = "", string proxyPassword = "")
		{
			var request = WebRequest.Create("http://" + targetHost + ":" + targetPort);
			var webProxy = new WebProxy(proxyUrl);
			request.Proxy = webProxy;
			request.Method = "CONNECT";
			
			if (proxyUserName != "" && proxyPassword != "")
            {
				var credentials = new NetworkCredential(proxyUserName, proxyPassword);
				webProxy.Credentials = credentials;
			}

			var response = request.GetResponse();
			var responseStream = response.GetResponseStream();
			Debug.Assert(responseStream != null);
			const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;
			var rsType = responseStream.GetType();
			var connectionProperty = rsType.GetProperty("Connection", Flags);
			var connection = connectionProperty.GetValue(responseStream, null);
			var connectionType = connection.GetType();
			var networkStreamProperty = connectionType.GetProperty("NetworkStream", Flags);
			var networkStream = networkStreamProperty.GetValue(connection, null);
			var nsType = networkStream.GetType();
			var socketProperty = nsType.GetProperty("Socket", Flags);
			var socket = (Socket)socketProperty.GetValue(networkStream, null);
			return new TcpClient { Client = socket };
		}
	}
}
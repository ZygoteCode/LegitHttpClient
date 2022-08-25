namespace LegitHttpClient
{
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Collections.Generic;
    using System;
    using Microsoft.VisualBasic;
    using System;
    using System.Text;
    using System.Net.Sockets;
    using System.IO;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Linq;

    public class HttpClient
    {
        public TcpClient TcpClient { get; set; }
        public NetworkStream NetworkStream { get; set; }
        public SslStream SslStream { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool HasSsl { get; set; }
        public int ByteRate { get; set; }

        public bool ConnectTo(string host, bool ssl = false, int port = 80, string proxyUrl = "", string proxyUsername = "", string proxyPassword = "")
        {
            try
            {
                ByteRate = 1;
                Host = host;
                HasSsl = ssl;
                Port = port;
                
                if (proxyUrl != "")
                {
                    TcpClient = Utils.ConnectionProxy(host, port, proxyUrl, proxyUsername, proxyPassword);
                }
                else
                {
                    TcpClient = new TcpClient(host, port);
                }

                if (!ssl)
                {
                    NetworkStream = TcpClient.GetStream();
                }
                else
                {
                    SslStream = new SslStream(TcpClient.GetStream(), false, new RemoteCertificateValidationCallback((a, b, c, d) => { return true; }), null);
                    SslStream.ReadTimeout = int.MaxValue;
                    SslStream.AuthenticateAsClient(host, null, System.Security.Authentication.SslProtocols.Tls12, false);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Disconnect()
        {
            TcpClient.Close();
            TcpClient.Dispose();

            if (NetworkStream != null)
            {
                NetworkStream.Close();
                NetworkStream.Dispose();
            }

            if (SslStream != null)
            {
                SslStream.Close();
                SslStream.Dispose();
            }

            TcpClient = null;
            NetworkStream = null;
            SslStream = null;
        }

        public HttpResponse Send(HttpRequest request, bool getResponse = true)
        {
            try
            {
                string firstPart = request.GetMethodStr() + " " + request.URI + " " + request.GetVersionStr();

                foreach (HttpHeader header in request.Headers)
                {
                    firstPart += "\r\n" + header.Name + ": " + header.Value;
                }

                firstPart += "\r\n\r\n";
                byte[] toSend = Encoding.UTF8.GetBytes(firstPart);

                if (request.Body != null)
                {
                    toSend = Utils.Combine(toSend, request.Body);
                }

                if (!HasSsl)
                {
                    NetworkStream.Write(toSend, 0, toSend.Length);
                    NetworkStream.Flush();
                }
                else
                {
                    SslStream.Write(toSend, 0, toSend.Length);
                    SslStream.Flush();
                }

                if (!getResponse)
                {
                    return null;
                }

                HttpResponse response = new HttpResponse();
                byte[] read = null;

                if (!HasSsl)
                {
                    read = Utils.ReadFully(NetworkStream);
                }
                else
                {
                    byte[] buffer = new byte[ByteRate];
                    int bytes = 0;

                    try
                    {
                        while (true)
                        {
                            bytes = SslStream.Read(buffer, 0, buffer.Length);
                            SslStream.ReadTimeout = 50;

                            if (read == null)
                            {
                                read = buffer.Take(bytes).ToArray();
                            }
                            else
                            {
                                read = Utils.Combine(read, buffer.Take(bytes).ToArray());
                            }
                        }
                    }
                    catch
                    {

                    }
                }

                string content = Encoding.UTF8.GetString(read);
                List<string> parts = new List<string>();
                List<HttpHeader> headers = new List<HttpHeader>();

                foreach (string part in Utils.SplitToLines(content))
                {
                    parts.Add(part);
                }

                for (int i = 0; i < parts.Count - 1; i++)
                {
                    if (i == 0)
                    {
                        string firstLine = parts[0];
                        string[] splitted = firstLine.Split(' ');

                        response.Version = splitted[0].Equals("HTTP/1.0") ? HttpVersion.HTTP_10 : HttpVersion.HTTP_11;
                        response.StatusCode = int.Parse(splitted[1]);
                        response.StatusDescription = splitted[2];
                    }
                    else
                    {
                        if (parts[i] == "")
                        {
                            break;
                        }
                        else
                        {
                            string[] splitted = Strings.Split(parts[i], ": ");

                            headers.Add(new HttpHeader()
                            {
                                Name = splitted[0],
                                Value = splitted[1]
                            });
                        }
                    }
                }

                response.Headers = headers;

                try
                {
                    int lastIndex = 0;
                    bool meet = false;

                    for (int i = 0; i < content.Length; i++)
                    {
                        if (meet)
                        {
                            if (content[i] != '\r')
                            {
                                meet = false;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (content[i] == '\n')
                            {
                                meet = true;
                                lastIndex = i;
                            }
                        }
                    }

                    response.Body = read.Skip(lastIndex).ToArray();
                }
                catch
                {

                }

                return response;
            }
            catch
            {
                return null;
            }
        }
    }
}
namespace LegitHttpClient
{
    using System.Collections.Generic;
    using System.Text;

    public class HttpRequest
    {
        public string URI { get; set; }
        public List<HttpHeader> Headers { get; set; }
        public byte[] Body { get; set; }
        public HttpMethod Method { get; set; }
        public HttpVersion Version { get; set; }

        public HttpRequest()
        {
            Headers = new List<HttpHeader>();
            Body = Encoding.UTF8.GetBytes("");
        }


        public string GetVersionStr()
        {
            switch (Version)
            {
                case HttpVersion.HTTP_10:
                    return "HTTP/1.0";
                case HttpVersion.HTTP_11:
                    return "HTTP/1.1";
                default:
                    return null;
            }
        }

        public string GetMethodStr()
        {
            return Method.ToString();
        }
    }
}
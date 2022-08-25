namespace LegitHttpClient
{
    using System.Collections.Generic;
    using System.Text;
    using BrotliSharpLib;

    public class HttpResponse
    {
        public HttpVersion Version { get; set; }
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public List<HttpHeader> Headers { get; set; }
        public byte[] Body { get; set; }

        public HttpResponse()
        {
            Headers = new List<HttpHeader>();
            Body = Encoding.UTF8.GetBytes("");
        }

        public void DecompressBrotli()
        {
            Body = Brotli.DecompressBuffer(Body, 0, Body.Length);
        }
    }
}
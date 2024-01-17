using System;
using System.Collections.Generic;
using System.Web;

namespace CompressionPriority
{
    public class CompressionPriority : IHttpModule
    {
        #region IHttpModule implementation

        public void Begin(Object source, EventArgs e)
        {
            HttpApplication app = (HttpApplication)source;
            HttpRequest request = app.Context.Request;
            if(request.Headers["Accept-Encoding"] != null)
            {
                List<string> encoding = new List<string>();
                string original_accept_encoding = request.Headers["Accept-Encoding"];
                if (original_accept_encoding.Contains("zstd"))
                    encoding.Add("zstd");
                if (original_accept_encoding.Contains("br"))
                    encoding.Add("br");
                if (original_accept_encoding.Contains("gzip"))
                    encoding.Add("gzip");
                if (original_accept_encoding.Contains("deflate"))
                    encoding.Add("deflate");
                string reordered_accept_encoding = string.Join(", ", encoding);
#if DEBUG
                System.Diagnostics.Debug.WriteLine("[CompressionPriority]: old order = " + original_accept_encoding + " new order = " + reordered_accept_encoding);
#endif
                request.Headers.Remove("Accept-Encoding");
                request.Headers.Add("Accept-Encoding", reordered_accept_encoding);
            }
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(Begin);
        }

        public void Dispose()
        {

        }

        #endregion
    }
}

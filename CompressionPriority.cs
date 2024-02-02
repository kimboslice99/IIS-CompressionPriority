using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                string acceptEncoding;
                string[] split = request.Headers["Accept-Encoding"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(s => s.Trim())
                                                  .ToArray();
                // fix older IIS versions that pick first in list
                Array.Reverse(split);

                // rfc9110 12.4.2 compliance fix
                // 0.000 means "not acceptable"

                if (split.Any(s => s.Contains(";q=0")))
                {
                    List<string> parts = new List<string>();
                    foreach (string part in split)
                    {
                        if (!part.Contains(";q="))
                        {
                            parts.Add(part);
                            continue;
                        }
                        if (double.TryParse(part.Split('=')[1], out double value) && value != 0)
                            parts.Add(part);
                    }

                    acceptEncoding = string.Join(", ", parts.ToArray());
                }
                else
                {
                    acceptEncoding = string.Join(", ", split);
                }
#if DEBUG
                System.Diagnostics.Debug.WriteLine("[CompressionPriority]: old order =[" + request.Headers["Accept-Encoding"] + "] new order =[" + acceptEncoding + ']');
#endif
                request.Headers.Remove("Accept-Encoding");
                request.Headers.Add("Accept-Encoding", acceptEncoding);
            }
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(Begin);
        }

        public void Dispose() { }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Logging;
namespace PostMeteion
{
   
    public class HttpServer
    {
        private Thread _serverThread;
        private HttpListener _listener;
        public bool IsRunning { get; private set; }
        public int Port { get; private set; }
        public Func<string, string,string> PostMeteionDelegate = null;
        public event OnExceptionEventHandler OnException;
        public delegate void OnExceptionEventHandler(Exception ex);

        public HttpServer(int port)
        {
            Initialize(port);
        }

        public void Initialize(int port)
        {
            this.Port = port;
            this.IsRunning = false;
            _serverThread = new Thread(Listen);
            _serverThread.Start();
        }
        public void Stop()
        {
            _serverThread.Interrupt();
            _listener.Stop();
            IsRunning = false;
        }
        public void Dispose()
        {
            Stop();
        }
        private void Listen()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://127.0.0.1:" + Port + "/");
                _listener.Start();
                PluginLog.Information("Working :D");
                PluginLog.Information("Listen On: http://127.0.0.1:" + Port + "/");
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
                PluginLog.Error("HttpServerStartError:" + ex.Message);
                IsRunning = false;
                return;
            }

            ThreadPool.QueueUserWorkItem(o => {
                try
                {
                    IsRunning=true;
                    while (_listener.IsListening)
                        ThreadPool.QueueUserWorkItem(c => {
                            if (!(c is HttpListenerContext context))
                                throw new ArgumentNullException(nameof(context));
                            try
                            {
                                DoAction(ref context);
                            }
                            catch (Exception ex)
                            {
                                PluginLog.Error("HttpServer:" + ex.Message);
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            }
                            finally
                            {
                                context.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                }
                catch
                {
                    // ignored
                }
            });
        }
        private void DoAction(ref HttpListenerContext context)
        {
            var payload = new StreamReader(context.Request.InputStream, Encoding.UTF8).ReadToEnd();

            var res = PostMeteionDelegate?.Invoke(TrimUrl(context.Request.Url.AbsolutePath), payload);

            var buf = Encoding.UTF8.GetBytes(res);
            context.Response.ContentLength64 = buf.Length;
            context.Response.OutputStream.Write(buf, 0, buf.Length);

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.OutputStream.Flush();
        }
        public string TrimUrl(string url)
        {
            return url.Trim(new char[] { '/' });
        }


    }
}

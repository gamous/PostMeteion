using Dalamud.Logging;

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ZodiacPost
{
    class HttpServer : IDisposable
    {
        private Thread _serverThread;
        private HttpListener _listener;

        public int Port { get; private set; }

        private ZodiacPost Plugin { get; }

        public HttpServer(ZodiacPost plugin, int port)
        {
            this.Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin), "ZodicPost cannot be null");
            this.Port = port;
            Initialize(Port);
        }

        public void Stop()
        {
            _listener.Stop();
            this.Plugin.serverState = false;
            PluginLog.Information("Safe Exit: " + this.Port);
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
                _listener.Prefixes.Add("http://*:" + Port + "/command/");
                _listener.Start();
                PluginLog.Information("Working :D");
            }
            catch (Exception ex)
            {
                PluginLog.Information("服务启动失败..."+ex);
                return;
            }

            ThreadPool.QueueUserWorkItem(o => {
                try
                {
                    while (_listener.IsListening) {
                        this.Plugin.serverState = true;
                        HttpListenerContext ctx = _listener.GetContext();
                        ThreadPool.QueueUserWorkItem(new WaitCallback(DoActionCommand), ctx);
                    }
                }
                catch
                {
                    // ignored
                }
            });
        }


        private void DoActionCommand(object o)
        {
            HttpListenerContext ctx = (HttpListenerContext)o;
            ctx.Response.StatusCode = 200;//设置返回给客服端http状态代码
            //接收POST参数
            Stream stream = ctx.Request.InputStream;
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            String body = reader.ReadToEnd();
            PluginLog.Information("收到POST数据:" + HttpUtility.UrlDecode(body));
            this.Plugin.DoCommand(body);
            //使用Writer输出http响应代码,UTF8格式
            using (StreamWriter writer = new StreamWriter(ctx.Response.OutputStream, Encoding.UTF8))
            {
                writer.Write("ok<br/>");
                writer.Write(body);
                writer.Close();
                ctx.Response.Close();
            }
        }

        private void Initialize(int port)
        {
            Port = port;
            _serverThread = new Thread(Listen);
            _serverThread.Start();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Dalamud.Logging;
using Dalamud.Game.Text;
using Newtonsoft.Json.Linq;


namespace PostMeteion
{
    public class WebhookClient
    {
        private readonly HttpClient httpClient;
        public Timer heartbeatTimer;
        public bool IsConnected;
        public bool IsRegistered;
        public string reportAddr="";
        
        internal WebhookClient()
        {
            httpClient = new() { Timeout = TimeSpan.FromMilliseconds(5000) };
            heartbeatTimer = new Timer(30*1000);
            heartbeatTimer.AutoReset = true;
            heartbeatTimer.Enabled = true;
            heartbeatTimer.Elapsed += OnTimedEvent;
            IsConnected = false;
            IsRegistered = false;
            _=Connect();
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            //PluginLog.Verbose("WebhookTimerElapsed {0:HH:mm:ss.fff}", e.SignalTime);
            _=Connect();
        }
        public async Task Connect()
        {
            var content = await GetA();
            if (content == "OK") { IsConnected = true; }
            else { IsConnected = false; }
        }
        public async Task<string> GetA(string uri="")
        {
            try
            {
                var response = await httpClient.GetAsync(reportAddr + uri);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }catch (Exception e){
                return "Timeout";
            }
}
        public async Task<string> PostA(string raw,string uri="")
        {
            try
            {
                var data = new StringContent(raw, Encoding.UTF8, "text/plain");
                var response = await httpClient.PostAsync(reportAddr+uri, data);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }catch (Exception e)
            {
                return "Timeout";
            }
        }
        public async Task<string> PostA(object o,string uri="")
        {
            try
            {
                var json = JsonConvert.SerializeObject(o);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(reportAddr + uri, data);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }catch (Exception e)
            {
                return "Timeout";
            }
        }
    }
}

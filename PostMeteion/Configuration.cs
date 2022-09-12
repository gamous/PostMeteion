using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace PostMeteion
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool ServerAutoStart { get; set; } = false;
        public int ApiServerPort { get; set; } = 12019;
        public string WebhookServer { get; set; } = "http://127.0.0.1:15000/meteion";
        public bool WebhookAutoStart { get; set; } = false;

        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}

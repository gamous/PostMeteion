using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Logging;
using XivCommon;


using System.Reflection;
using System.IO;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace ZodiacPost
{
    public class ZodiacPost : IDalamudPlugin
    {
        public string Name => "ZodiacPost";

        private const string commandName = "/xpost";

        public bool serverState = false;

        private DalamudPluginInterface PluginInterface { get; init; }

        private readonly XivCommonBase Common;

        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }

        public ZodiacPost(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Common = new XivCommonBase(Hooks.ContextMenu);

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            this.PluginUi = new PluginUI(this,this.Configuration);

            this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Just Post command to http://localhost:{port}/command"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.PluginUi.Dispose();
            this.CommandManager.RemoveHandler(commandName);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            this.PluginUi.Visible = true;


        }

        private void DrawUI()
        {
            this.PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            this.PluginUi.SettingsVisible = true;
        }

        public void DoCommand(string command)
        {
            this.Common.Functions.Chat.SendMessage(command);
        }

    }
}
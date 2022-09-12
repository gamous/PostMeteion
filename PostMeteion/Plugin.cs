using Dalamud.Game.Command;
using Dalamud.Game.ClientState;
using Dalamud.IoC;
using Dalamud.Data;
using Dalamud.Plugin;
using Dalamud.Logging;
using XivCommon;
using System.IO;
using System.Linq;
using System;
using System.Reflection;
using System.Collections.Generic;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.PartyFinder.Types;
using Newtonsoft.Json;

namespace PostMeteion
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "PostMeteion";

        private const string commandName = "/xpost";

        private DalamudPluginInterface PluginInterface { get; init; }
        public Configuration Config;

        private PluginUI PluginUi { get; init; }

        private readonly XivCommonBase Common;

        public readonly WayMark Waymark;
        public readonly Status Status;
        public HttpServer? httpServer;
        public WebhookClient Webhook;

        public delegate string HandlerDelegate(string command);
        private Dictionary<string, HandlerDelegate> CmdBind = new(StringComparer.OrdinalIgnoreCase);

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
            this.Config = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Config.Initialize(this.PluginInterface);
            this.PluginUi = new PluginUI(this);

            pluginInterface.Create<Svc>();
            this.Common = new XivCommonBase();

            this.Waymark = new WayMark();
            this.Status = new Status();
            this.Webhook = new WebhookClient();
            this.Webhook.reportAddr = this.Config.WebhookServer;

            Svc.Commands.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Go to Propylaion"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            InitAction();
            if (this.Config.ServerAutoStart == true)
            {
                ServerStart(this.Config.ApiServerPort);
            }
            if (this.Config.WebhookAutoStart == true)
            {
                WebhookStart();
            }
        }

        public void Dispose()
        {
            this.PluginUi.Dispose();
            Svc.Commands.RemoveHandler(commandName);
            ServerStop();
            WebhookStop();
        }

        //webhook callback
        public void WebhookStart()
        {
            if (Webhook.IsRegistered==true) return;
            Svc.Chat.ChatMessage += OnMessageHandler;
            Svc.PfGui.ReceiveListing += PartyFinderListingEventHandler;
            Webhook.IsRegistered = true;
        }
        public void WebhookStop()
        {
            if (Webhook.IsRegistered==false) return;
            Svc.Chat.ChatMessage -= OnMessageHandler;
            Svc.PfGui.ReceiveListing -= PartyFinderListingEventHandler;
            Webhook.IsRegistered = false;
            Webhook.heartbeatTimer.Enabled = false;
        }
        public void OnMessageHandler(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (Webhook.IsConnected) _=Webhook.PostA($"{type}|{sender}|{message}|{(from p in message.Payloads select p.ToString()).ToArray()}","/chat");
        }
        public void PartyFinderListingEventHandler(PartyFinderListing listing, PartyFinderListingEventArgs args)
        {
            if (Webhook.IsConnected) _=Webhook.PostA($"{listing.Id}|{listing.Name}@{listing.HomeWorld.Value.Name}|{listing.CurrentWorld.Value.Name}|{listing.Duty.Value.Name}|{listing.SecondsRemaining/60:D}|{listing.Description}", "/partyfinder");
        }
        //httpserver
        public void ServerStart(int port)
        {
            try
            {
                this.httpServer = new HttpServer(port);
                this.httpServer.PostMeteionDelegate = DoAction;
                this.httpServer.OnException += OnException;
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }
        public void ServerStop()
        {
            if(this.httpServer != null)
            {
                if(httpServer.IsRunning == true) this.httpServer.Stop();
                this.httpServer.PostMeteionDelegate = null;
                this.httpServer.OnException -= OnException;
            }
        }
        private void OnException(Exception ex)
        {
            string errorMessage = $"Cannot Listen on {httpServer?.Port} \n{ex.Message}";
            PluginLog.Error(errorMessage);
        }

        //Action
        public void InitAction()
        {
            HandlerDelegate commandHandler = DoTextCommand;
            SetAction("command", commandHandler);
            HandlerDelegate waymarkHandler = DoWaymarks;
            SetAction("waymark", waymarkHandler);
            SetAction("place", waymarkHandler);
            HandlerDelegate queryHandler = DoStatusQuery;
            SetAction("query", queryHandler);
        }
        public void SetAction(string command, HandlerDelegate action)
        {
            CmdBind[command] = action;
        }
        public void ClearAction()
        {
            CmdBind.Clear();
        }
        public string DoAction(string command, string payload)
        {
            try
            {
                return CmdBind[command](payload);
            }
            catch (Exception ex)
            {
                var errorMsg = "DoActionWrong(NoSuchAction):" + ex.ToString();
                PluginLog.Error(errorMsg);
                return errorMsg;
            }
        }
        public string DoStatusQuery(string command)
        {
            return Status.DoQuery(command);
        }
        public string DoWaymarks(string command)
        {
            return Waymark.DoWaymarks(command);
        }
        public string DoTextCommand(string command)
        {
            
            if (command.StartsWith("/") & command.Length >= 2 & command.Length < 400)
            {
                //bool res;
                //res=Svc.Commands.ProcessCommand(command);//only dalamud command
                SafeSendMessage(command);
                return "Exectued";
            }
            else
            {
                var errorMsg = "DoTextCommandWrong(InvalidCommand):" + command;
                PluginLog.Information(errorMsg);
                return errorMsg;
            }
        }
        private static object LockChat = new object();
        public void SafeSendMessage(string command)
        {
            lock (LockChat)
            {
                PluginLog.Information("ExecutedTextCommand:"+command);
                this.Common.Functions.Chat.SendMessage(command);
            }
        }


        //Command Controller
        private void OnCommand(string command, string args)
        {
            this.PluginUi.Visible = true;
        }


        //UI
        private void DrawUI()
        {
            this.PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            this.PluginUi.SettingsVisible = true;
        }
    }
}

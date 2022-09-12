using ImGuiNET;
using System;
using System.Numerics;

using Dalamud.Logging;

namespace PostMeteion
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private readonly Plugin plugin;

        public PluginUI(Plugin plugin)
        {
            this.plugin = plugin;
        }

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }
        private bool testingVisible = false;
        public bool TestingVisible
        {
            get { return this.testingVisible; }
            set { this.testingVisible = value; }
        }

        private static Vector4 errorColor = new Vector4(1f, 0f, 0f, 1f);
        private static Vector4 fineColor = new Vector4(0.337f, 1f, 0.019f, 1f);

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
            DrawSettingsWindow();
            DrawTestingWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 150), ImGuiCond.FirstUseEver);
            //ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Elpis - Home of Meteion", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (ImGui.Button("The Twelve Wonders - SettingCenter"))
                {
                    SettingsVisible = true;
                }
                if (ImGui.Button("Anagnorisis - TestingLab"))
                {
                    TestingVisible = true;
                }
            }
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            if (ImGui.Begin("HttpSetting", ref this.settingsVisible,
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                var serverAutoStart = this.plugin.Config.ServerAutoStart;
                if (ImGui.Checkbox("ServerAutoStart", ref serverAutoStart))
                {
                    this.plugin.Config.ServerAutoStart = serverAutoStart;
                    this.plugin.Config.Save();
                }
                var apiServerPort = this.plugin.Config.ApiServerPort;
                if (ImGui.InputInt("ListenPort", ref apiServerPort, 1, 1, ImGuiInputTextFlags.CharsDecimal))
                {
                    this.plugin.Config.ApiServerPort = apiServerPort;
                    this.plugin.Config.Save();
                    //ShouldReloading
                }

                ImGui.Text("ServerStatus: ");
                ImGui.SameLine();
                //getstatus
                bool serverState = false;
                if (this.plugin.httpServer is not null) {
                    serverState= this.plugin.httpServer.IsRunning;
                }
                ImGui.TextColored(serverState ? fineColor : errorColor, serverState ? "Running":"Halting");
                ImGui.SameLine();
                if (serverState)
                {
                    if (ImGui.Button("Restart Server")) {
                        this.plugin.ServerStop();
                        this.plugin.ServerStart(apiServerPort);
                    };
                    ImGui.SameLine();
                    if (ImGui.Button("Stop Server")){
                        this.plugin.ServerStop();
                    };
                }
                else
                {
                    if (ImGui.Button("Start Server")) {
                        this.plugin.ServerStart(apiServerPort);
                    }
                }


                var webhookAutoStart = this.plugin.Config.WebhookAutoStart;
                if (ImGui.Checkbox("WebhookAutoStart", ref webhookAutoStart))
                {
                    this.plugin.Config.WebhookAutoStart = webhookAutoStart;
                    this.plugin.Config.Save();
                }
                var reportAddr = this.plugin.Config.WebhookServer;
                if (ImGui.InputText("ReportAddress", ref reportAddr, 100))
                {
                    this.plugin.Config.WebhookServer=reportAddr;
                }
                ImGui.Text("WebhookConnection: ");
                ImGui.SameLine();
                bool isConnected = this.plugin.Webhook.IsConnected;
                ImGui.TextColored(isConnected ? fineColor : errorColor, isConnected ? "Connected" : "Disconnected");
                ImGui.SameLine();
                if (ImGui.Button("TryConnect"))
                {
                    this.plugin.Config.Save();
                    this.plugin.Webhook.reportAddr=this.plugin.Config.WebhookServer;
                    _=this.plugin.Webhook.Connect();
                }
                ImGui.Text("WebhookDelegate: ");
                ImGui.SameLine();
                bool isRegistered = this.plugin.Webhook.IsRegistered;
                ImGui.TextColored(isRegistered ? fineColor : errorColor, isRegistered ? "Registered" : "Unregistered");
                ImGui.SameLine();
                if (isRegistered)
                {
                    if (ImGui.Button("Unregister"))
                    {
                        this.plugin.WebhookStop();
                    };
                }
                else
                {
                    if (ImGui.Button("Register"))
                    {
                        this.plugin.WebhookStart();
                    }
                }
            }
            ImGui.End();
        }

        private string test_command = "/e You struggle in vain. You will not silence our song of oblivion!";
        private string test_waymark = "{}";
        public void DrawTestingWindow()
        {
            if (!TestingVisible)
            {
                return;
            }
            ImGui.SetNextWindowSize(new Vector2(560, 180), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Anagnorisis - TestingLab", ref this.testingVisible,
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                
                if (ImGui.InputText("TestCommand", ref this.test_command, 100))
                {
                    this.plugin.Config.Save();
                }
                if (ImGui.Button("DoCommand"))
                {
                    PluginLog.Information(test_command);
                    this.plugin.DoTextCommand(test_command);
                }
                if (ImGui.InputText("TestWaymark", ref this.test_waymark, 400)) { 
                }
                if (ImGui.Button("ShowWaymark"))
                {
                    test_waymark = plugin.Waymark.ExportWaymark();
                }
            }
            ImGui.End();
        }
        public void Dispose()
        {

        }
    }
}

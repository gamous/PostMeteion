using ImGuiNET;
using System;
using System.Numerics;

namespace ZodiacPost
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private ZodiacPost Plugin { get; }
        private Configuration configuration;

        public int port = 15000;

        private HttpServer server { get; set; }

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

        // passing in the image here just for simplicity
        public PluginUI(ZodiacPost plugin, Configuration configuration)
        {
            this.Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin), "ZodiacPost cannot be null");
            this.configuration = configuration;

            if (this.configuration.AutoStart == true)
            {
                this.server = new HttpServer(this.Plugin, this.configuration.Port);
            }
        }

        public void Dispose()
        {
            this.server.Stop();
        }

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
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }
            
            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("ZodiacPost", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                


                if (ImGui.Button("Show Settings"))
                {
                    SettingsVisible = true;
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
            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("ZodiacPost Settings", ref this.settingsVisible,
                  ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                // can't ref a property, so use a local copy
                var configValue = this.configuration.AutoStart;
                if (ImGui.Checkbox("AutoStart", ref configValue))
                {
                    this.configuration.AutoStart = configValue;
                    // can save immediately on change, if you don't want to provide a "Save and Close" button
                    this.configuration.Save();
                }
                var port = this.configuration.Port;
                if (ImGui.InputInt("ListenPort", ref port, 1, 1, 1))
                {
                    this.configuration.Port = port;
                    this.configuration.Save();
                }
                if (ImGui.Button("Test Command"))
                {
                    Plugin.DoCommand("/e <se.1>");
                    Plugin.DoCommand("/e <se.2>");
                    Plugin.DoCommand("/e <se.3>");
                    Plugin.DoCommand("/e now listening: " +"http://localhost:" + this.configuration.Port + "/command/");
                }

                ImGui.Text("Server State:" + Plugin.serverState);

                if (ImGui.Button(Plugin.serverState==false?"Start Server":"ReStart Server"))
                {
                    if (Plugin.serverState == true)
                    {
                        this.server.Stop();

                    }
                    this.server = new HttpServer(this.Plugin, this.configuration.Port);
                }
                if (ImGui.Button("Stop Server"))
                {
                    if (Plugin.serverState == true)
                    {
                        this.server.Stop();

                    }
                }

            }
            ImGui.End();
        }
    }
}

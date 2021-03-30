using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;
using Dalamud.Game.Network;
using Dalamud.Game.Network.Structures;
using FFXIV_ACT_Plugin;
using System.Web.Script.Serialization;

[assembly: AssemblyTitle("FF14 David's Tools")]
[assembly: AssemblyDescription("Does stuff")]
[assembly: AssemblyCompany("?")]
[assembly: AssemblyVersion("1.0.0.0")]

namespace UniversalisPlugin
{
    public class UniversalisPluginControl : UserControl, IActPluginV1
    {
        #region Designer Created Code (Avoid editing)
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.logTextBox = new System.Windows.Forms.RichTextBox();
            this.checkbox_sendraw = new System.Windows.Forms.CheckBox();
            this.textBox_addr = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(106, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 0;
            // 
            // logTextBox
            // 
            this.logTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logTextBox.Location = new System.Drawing.Point(3, 22);
            this.logTextBox.Margin = new System.Windows.Forms.Padding(500);
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.Size = new System.Drawing.Size(686, 8000);
            this.logTextBox.TabIndex = 3;
            this.logTextBox.Text = "";
            // 
            // checkbox_sendraw
            // 
            this.checkbox_sendraw.AutoSize = true;
            this.checkbox_sendraw.Location = new System.Drawing.Point(3, 0);
            this.checkbox_sendraw.Name = "checkbox_sendraw";
            this.checkbox_sendraw.Size = new System.Drawing.Size(196, 17);
            this.checkbox_sendraw.TabIndex = 5;
            this.checkbox_sendraw.Text = "Send raw packets to server address";
            this.checkbox_sendraw.UseVisualStyleBackColor = true;
            this.checkbox_sendraw.CheckedChanged += new System.EventHandler(this.checkbox_sendraw_CheckedChanged);
            // 
            // textBox_addr
            // 
            this.textBox_addr.Location = new System.Drawing.Point(219, -2);
            this.textBox_addr.Name = "textBox_addr";
            this.textBox_addr.Size = new System.Drawing.Size(100, 20);
            this.textBox_addr.TabIndex = 6;
            this.textBox_addr.Text = "server address";
            this.textBox_addr.TextChanged += new System.EventHandler(this.textBox_addr_TextChanged);
            // 
            // UniversalisPluginControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBox_addr);
            this.Controls.Add(this.checkbox_sendraw);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.label1);
            this.Name = "UniversalisPluginControl";
            this.Size = new System.Drawing.Size(686, 516);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public RichTextBox logTextBox;
        private CheckBox checkbox_sendraw;
        private TextBox textBox_addr;
        private System.Windows.Forms.Label label1;

        #endregion
        public UniversalisPluginControl()
        {
            InitializeComponent();
        }

        Label lblStatus;    // The status label that appears in ACT's Plugin tab
        string settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\market_logger.config.xml");
        SettingsSerializer xmlSettings;

        private Definitions _definitions;

        public FFXIV_ACT_Plugin.FFXIV_ACT_Plugin FfxivPlugin;

        public uint CurrentWorldId => FfxivPlugin.DataRepository.GetCombatantList()
            .First(c => c.ID == FfxivPlugin.DataRepository.GetCurrentPlayerID()).CurrentWorldID;
        public ulong LocalContentId;

        public uint current_player = 0;
        public long last_update_time = 0;

        #region IActPluginV1 Members
        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            lblStatus = pluginStatusText;   // Hand the status label's reference to our local var
            pluginScreenSpace.Controls.Add(this);   // Add this UserControl to the tab ACT provides
            this.Dock = DockStyle.Fill; // Expand the UserControl to fill the tab's client space
            xmlSettings = new SettingsSerializer(this); // Create a new settings serializer and pass it this instance
            xmlSettings.AddControlSetting(textBox_addr.Name, textBox_addr);
            xmlSettings.AddControlSetting(checkbox_sendraw.Name, checkbox_sendraw);

            LoadSettings();

            pluginScreenSpace.Text = "DauntUtils";


            try
            {
                _definitions = new Definitions();
                FfxivPlugin = GetFfxivPlugin();

                FfxivPlugin.DataSubscription.LogLine += DataSubscriptionLogLine;
                FfxivPlugin.DataSubscription.NetworkReceived += DataSubscriptionOnNetworkReceived;
                FfxivPlugin.DataSubscription.NetworkSent += DataSubscriptionOnNetworkSent;

                Log("plugin loaded.");
                lblStatus.Text = "Plugin Started";
            }
            catch (Exception ex)
            {
                Log("[ERROR] Could not initialize plugin:\n" + ex);
                lblStatus.Text = "Plugin Failed";
            }
        }

        public void DeInitPlugin()
        {
            // Unsubscribe from any events you listen to when exiting!
            FfxivPlugin.DataSubscription.NetworkReceived -= DataSubscriptionOnNetworkReceived;
            FfxivPlugin.DataSubscription.LogLine -= DataSubscriptionLogLine;
            FfxivPlugin.DataSubscription.NetworkSent -= DataSubscriptionOnNetworkSent;

            SaveSettings();
            lblStatus.Text = "Plugin Exited";
        }
        #endregion

        #region FFXIV plugin handling

        private void SendRaw(byte[] message, string endpoint, long epoch)
        {
            var sb = new StringBuilder("");
            sb.Append(epoch.ToString("X") + " ");

            foreach (var b in message)
            {
                sb.Append(b.ToString("X") + " ");
            }
            using (var client = new WebClient())
            {
                client.UploadString($"http://{this.textBox_addr.Text}/{endpoint}", "POST", sb.ToString());
            }
        }
        private void SendText(string message, string endpoint)
        {
            using (var client = new WebClient())
            {
                client.UploadString($"http://{this.textBox_addr.Text}/{endpoint}", "POST", message);
            }
        }
        private void DataSubscriptionLogLine(uint EventType, uint Seconds, string message)
        {
            if (checkbox_sendraw.Checked)
            {
                this.SendText(message, "logs");
            }
        }
        private void DataSubscriptionOnNetworkSent(string connection, long epoch, byte[] message)
        {
            if (checkbox_sendraw.Checked)
            {
                this.SendRaw(message, "outbound", epoch);
            }
        }
        private void DataSubscriptionOnNetworkReceived(string connection, long epoch, byte[] message)
        {
            //Log($"{connection}: {epoch.ToString("X")}");
            var opCode = BitConverter.ToInt16(message, 0x12);

            if (checkbox_sendraw.Checked)
            {
                if (current_player != FfxivPlugin.DataRepository.GetCurrentPlayerID() || last_update_time < DateTimeOffset.Now.ToUnixTimeSeconds() - 10)
                {
                    current_player = FfxivPlugin.DataRepository.GetCurrentPlayerID();
                    last_update_time = DateTimeOffset.Now.ToUnixTimeSeconds();
                    this.SendText(new JavaScriptSerializer().Serialize(new { playerID = current_player, worldID = CurrentWorldId }), "metadata");
                }

                this.SendRaw(message, "raw", epoch);
            }

            if (opCode == 350)
            {
                var listing = HousingWardInfo.Read(message.Skip(0x20).ToArray(), this);
                return;
            }

        }

        private FFXIV_ACT_Plugin.FFXIV_ACT_Plugin GetFfxivPlugin()
        {
            object ffxivPlugin = null;  
            
            while (ffxivPlugin == null)
            {
                var plugins = Advanced_Combat_Tracker.ActGlobals.oFormActMain.ActPlugins;
                foreach (var plugin in plugins)
                {
                    if (plugin.pluginFile.Name.ToUpper().Contains("FFXIV_ACT_Plugin".ToUpper()) &&
                        plugin.lblPluginStatus.Text.ToUpper().Contains("FFXIV Plugin Started.".ToUpper()))
                    {
                        ffxivPlugin = plugin.pluginObj;
                    }
                }
                System.Threading.Thread.Sleep(1);
            }

            if (ffxivPlugin == null)
                throw new Exception("Could not find FFXIV plugin. Make sure that it is loaded this.");

            return (FFXIV_ACT_Plugin.FFXIV_ACT_Plugin) ffxivPlugin;
        }

        #endregion

        #region Miscellaneous

        public void Log(string text) => logTextBox.AppendText($"{text}\n");


        #endregion

        void LoadSettings()
		{
			// Add any controls you want to save the state of
			//xmlSettings.AddControlSetting(textBox1.Name, textBox1);

			if (File.Exists(settingsFile))
			{
				FileStream fs = new FileStream(settingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				XmlTextReader xReader = new XmlTextReader(fs);

				try
				{
					while (xReader.Read())
					{
						if (xReader.NodeType == XmlNodeType.Element)
						{
							if (xReader.LocalName == "SettingsSerializer")
							{
								xmlSettings.ImportFromXml(xReader);
							}
						}
					}
				}
				catch (Exception ex)
				{
					lblStatus.Text = "Error loading settings: " + ex.Message;
				}
				xReader.Close();
			}
		}
		void SaveSettings()
		{
			FileStream fs = new FileStream(settingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			XmlTextWriter xWriter = new XmlTextWriter(fs, Encoding.UTF8);
			xWriter.Formatting = Formatting.Indented;
			xWriter.Indentation = 1;
			xWriter.IndentChar = '\t';
			xWriter.WriteStartDocument(true);
			xWriter.WriteStartElement("Config");    // <Config>
			xWriter.WriteStartElement("SettingsSerializer");    // <Config><SettingsSerializer>
			xmlSettings.ExportToXml(xWriter);   // Fill the SettingsSerializer XML
			xWriter.WriteEndElement();  // </SettingsSerializer>
			xWriter.WriteEndElement();  // </Config>
			xWriter.WriteEndDocument(); // Tie up loose ends (shouldn't be any)
			xWriter.Flush();    // Flush the file buffer to disk
			xWriter.Close();
		}

        private void checkbox_sendraw_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void textBox_addr_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

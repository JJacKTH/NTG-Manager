using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;
using RBX_Alt_Manager.Classes;
using RBX_Alt_Manager.Forms;

namespace RBX_Alt_Manager.Nexus
{
    public class SettingsControl : UserControl
    {
        private bool SettingsLoaded = false;
        private RegistryKey StartupKey;

        // UI Controls - General
        private CheckBox AutoUpdateCB;
        private CheckBox AsyncJoinCB;
        private Label DelayLabel;
        private NumericUpDown LaunchDelayNumber;
        private CheckBox SavePasswordCB;
        private CheckBox DisableAgingAlertCB;
        private CheckBox HideMRobloxCB;
        private CheckBox StartOnPCStartup;
        private CheckBox ShuffleLowestServerCB;
        private CheckBox MultiRobloxCB;
        private CheckBox AutoCookieRefreshCB;
        private Label RegionFormatLabel;
        private TextBox RegionFormatTB;
        private Label MRGLabel;
        private NumericUpDown MaxRecentGamesNumber;
        private Label RSLabel;
        private Button EncryptionSelectionButton;
        private ComboBox themeCombo;

        // UI Controls - Developer
        private CheckBox EnableDMCB;
        private CheckBox EnableWSCB;
        private Label PortLabel;
        private NumericUpDown PortNumber;
        private CheckBox ERRPCB;
        private CheckBox AllowGCCB;
        private CheckBox AllowGACB;
        private CheckBox AllowLACB;
        private CheckBox AllowAECB;
        private CheckBox DisableImagesCB;
        private CheckBox AllowExternalConnectionsCB;
        private Label WSPWLabel;
        private TextBox PasswordTextBox;

        // UI Controls - Miscellaneous
        private CheckBox PresenceCB;
        private Label PresenceUpdateLabel;
        private NumericUpDown PresenceUpdateRateNum;
        private CheckBox UnlockFPSCB;
        private Label FPSCapLabel;
        private NumericUpDown MaxFPSValue;
        private CheckBox OverrideWithCustomCB;
        private Button ForceUpdateButton;
        private OpenFileDialog CustomClientSettingsDialog;

        private ToolTip Helper;
        private NBTabControl SettingsTC;
        private TabPage GeneralTab;
        private TabPage DeveloperTab;
        private TabPage MiscellaneousTab;

        public SettingsControl()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(15, 17, 26);
            this.Dock = DockStyle.Fill;
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            InitializeComponents();
            LoadSettings();
        }

        private void InitializeComponents()
        {
            this.Controls.Clear();
            Helper = new ToolTip();

            SettingsTC = new NBTabControl
            {
                Dock = DockStyle.Fill
            };

            GeneralTab = new TabPage("General") { BackColor = Color.FromArgb(15, 17, 26), Padding = new Padding(12) };
            DeveloperTab = new TabPage("Developer") { BackColor = Color.FromArgb(15, 17, 26), Padding = new Padding(12) };
            MiscellaneousTab = new TabPage("Miscellaneous") { BackColor = Color.FromArgb(15, 17, 26), Padding = new Padding(12) };

            BuildGeneralTab();
            BuildDeveloperTab();
            BuildMiscellaneousTab();

            SettingsTC.TabPages.Add(GeneralTab);
            SettingsTC.TabPages.Add(DeveloperTab);
            SettingsTC.TabPages.Add(MiscellaneousTab);

            this.Controls.Add(SettingsTC);
        }

        private FlowLayoutPanel CreateScrollableLayoutPanel()
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(15, 17, 26)
            };
        }

        private CheckBox CreateCheckBox(string text, string tooltipText)
        {
            CheckBox cb = new CheckBox
            {
                Text = text,
                AutoSize = true,
                ForeColor = Color.FromArgb(240, 243, 254),
                Margin = new Padding(3, 6, 3, 6),
                Cursor = Cursors.Hand
            };
            if (!string.IsNullOrEmpty(tooltipText)) Helper.SetToolTip(cb, tooltipText);
            return cb;
        }

        private Panel CreateRowContainer(Control leftControl, Control rightControl)
        {
            Panel row = new Panel
            {
                Size = new Size(500, 36),
                Margin = new Padding(3, 4, 3, 4)
            };
            leftControl.Location = new Point(0, 8);
            rightControl.Location = new Point(220, 4);
            row.Controls.Add(leftControl);
            row.Controls.Add(rightControl);
            return row;
        }

        private void BuildGeneralTab()
        {
            FlowLayoutPanel panel = CreateScrollableLayoutPanel();

            Label header = new Label
            {
                Text = "⚙️ GENERAL SETTINGS",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 242, 254),
                AutoSize = true,
                Margin = new Padding(3, 0, 3, 16)
            };
            panel.Controls.Add(header);

            AutoUpdateCB = CreateCheckBox("Check for Updates", "ตรวจสอบการอัปเดตโปรแกรมใหม่อัตโนมัติเมื่อเปิดใช้งาน");
            AutoUpdateCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("CheckForUpdates", AutoUpdateCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(AutoUpdateCB);

            AsyncJoinCB = CreateCheckBox("Async Launching", "เปิดใช้งานการรันล็อกอินหลายไอดีแบบขนาน (Async) ลดเวลารอระหว่างเข้าเกม");
            LaunchDelayNumber = new NumericUpDown { Maximum = 60, Minimum = 0, Width = 80, Value = 1 };
            DelayLabel = new Label { Text = "Launch Delay (s):", AutoSize = true, ForeColor = Color.FromArgb(240, 243, 254) };
            AsyncJoinCB.CheckedChanged += (s, e) =>
            {
                LaunchDelayNumber.Enabled = !AsyncJoinCB.Checked;
                if (SettingsLoaded) { AccountManager.General.Set("AsyncJoin", AsyncJoinCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); }
            };
            LaunchDelayNumber.ValueChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("AccountJoinDelay", LaunchDelayNumber.Value.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(AsyncJoinCB);
            panel.Controls.Add(CreateRowContainer(DelayLabel, LaunchDelayNumber));

            SavePasswordCB = CreateCheckBox("Save Passwords", "บันทึกรหัสผ่านบัญชีเก็บไว้ในไฟล์ระบบอย่างปลอดภัย");
            SavePasswordCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("SavePasswords", SavePasswordCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(SavePasswordCB);

            DisableAgingAlertCB = CreateCheckBox("Disable Aging Alert", "ปิดการแจ้งเตือนสัญลักษณ์แจ้งเตือนไอดีไม่ได้เข้าใช้นาน (Aging Dots)");
            DisableAgingAlertCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("DisableAgingAlert", DisableAgingAlertCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(DisableAgingAlertCB);

            HideMRobloxCB = CreateCheckBox("Hide Multi Roblox Alert", "ปิดการแสดงป๊อปอัปเตือนเรื่อง Multi Roblox");
            HideMRobloxCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("HideRbxAlert", HideMRobloxCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(HideMRobloxCB);

            StartOnPCStartup = CreateCheckBox("Run on Windows Startup", "สั่งให้โปรแกรมเปิดทำงานอัตโนมัติเมื่อเปิดคอมพิวเตอร์ (Windows Startup)");
            StartOnPCStartup.CheckedChanged += (s, e) =>
            {
                if (!SettingsLoaded) return;
                try
                {
                    if (StartOnPCStartup.Checked) StartupKey?.SetValue(Application.ProductName, Application.ExecutablePath);
                    else StartupKey?.DeleteValue(Application.ProductName);
                }
                catch { }
            };
            panel.Controls.Add(StartOnPCStartup);

            ShuffleLowestServerCB = CreateCheckBox("Shuffle Chooses Lowest Server", "สุ่มเลือกเซิร์ฟเวอร์ที่มีจำนวนผู้เล่นน้อยที่สุด เพื่อลดความแออัด");
            ShuffleLowestServerCB.CheckedChanged += (s, e) => { AccountManager.General.Set("ShuffleChoosesLowestServer", ShuffleLowestServerCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); };
            panel.Controls.Add(ShuffleLowestServerCB);

            MultiRobloxCB = CreateCheckBox("Multi Roblox", "อนุญาตให้เปิดโปรแกรมเกม Roblox พร้อมกันได้หลายๆ จอ/ไอดี");
            MultiRobloxCB.CheckedChanged += (s, e) =>
            {
                AccountManager.General.Set("EnableMultiRbx", MultiRobloxCB.Checked ? "true" : "false");
                AccountManager.IniSettings.Save("RAMSettings.ini");
                if (!AccountManager.Instance.UpdateMultiRoblox())
                    MessageBox.Show("Roblox is currently running, multi roblox will not work if roblox is open.", "Roblox Account Manager", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };
            panel.Controls.Add(MultiRobloxCB);

            RegionFormatLabel = new Label { Text = "Region Format:", AutoSize = true, ForeColor = Color.FromArgb(240, 243, 254) };
            RegionFormatTB = new TextBox { Width = 200, BackColor = Color.FromArgb(25, 30, 48), ForeColor = Color.White };
            RegionFormatTB.TextChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("ServerRegionFormat", RegionFormatTB.Text); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(CreateRowContainer(RegionFormatLabel, RegionFormatTB));

            MRGLabel = new Label { Text = "Max Recent Games:", AutoSize = true, ForeColor = Color.FromArgb(240, 243, 254) };
            MaxRecentGamesNumber = new NumericUpDown { Maximum = 30, Minimum = 1, Width = 80, Value = 5 };
            MaxRecentGamesNumber.ValueChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("MaxRecentGames", MaxRecentGamesNumber.Value.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(CreateRowContainer(MRGLabel, MaxRecentGamesNumber));

            Label themeLabel = new Label { Text = "🎨 เลือกธีมระบบ (Custom Preset Theme):", AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(240, 243, 254), Margin = new Padding(3, 12, 3, 4) };
            themeCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300, BackColor = Color.FromArgb(25, 30, 48), ForeColor = Color.White };
            themeCombo.Items.AddRange(new object[]
            {
                "1. Slate Blue (โทนเทาอมฟ้าสไตล์ซอฟต์)",
                "2. Warm Gray (โทนอุ่น สบายสายตา)",
                "3. Cool Neutral Modern (โทนเทากลาง มินิมอล macOS)",
                "4. Mid-tone Pastel (โทนพาสเทลกลางๆ ดูสดใส)"
            });
            themeCombo.SelectedIndexChanged += (s, ev) =>
            {
                if (!SettingsLoaded) return;
                ThemeEditor.ApplyPresetTheme(themeCombo.SelectedIndex);
            };
            panel.Controls.Add(themeLabel);
            panel.Controls.Add(themeCombo);

            RSLabel = new Label
            {
                Text = "* Some settings may require restarting the program (e.g. WebServer Port, Aging Alert)",
                AutoSize = true,
                ForeColor = Color.FromArgb(123, 132, 163),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                Margin = new Padding(3, 12, 3, 8)
            };
            panel.Controls.Add(RSLabel);

            EncryptionSelectionButton = new Button
            {
                Text = "🔑 Reset Encryption Method",
                Width = 260,
                Height = 36,
                BackColor = Color.FromArgb(25, 30, 48),
                ForeColor = Color.FromArgb(0, 242, 254),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(3, 4, 3, 16)
            };
            EncryptionSelectionButton.FlatAppearance.BorderColor = Color.FromArgb(60, 0, 242, 254);
            EncryptionSelectionButton.Click += (s, e) =>
            {
                if (Utilities.YesNoPrompt("Settings", "Change Encryption Method", "Are you sure you want to change how your data is encrypted?", false))
                    AccountManager.Instance.ResetEncryption(true);
            };
            panel.Controls.Add(EncryptionSelectionButton);

            GeneralTab.Controls.Add(panel);
        }

        private void BuildDeveloperTab()
        {
            FlowLayoutPanel panel = CreateScrollableLayoutPanel();

            Label header = new Label
            {
                Text = "💻 DEVELOPER & WEB SERVER SETTINGS",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 242, 254),
                AutoSize = true,
                Margin = new Padding(3, 0, 3, 16)
            };
            panel.Controls.Add(header);

            EnableDMCB = CreateCheckBox("Enable Developer Mode", "เปิดโหมดนักพัฒนาเพื่อเข้าถึงเมนูและคำสั่งขั้นสูง");
            EnableDMCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.Developer.Set("DevMode", EnableDMCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(EnableDMCB);

            EnableWSCB = CreateCheckBox("Enable Web Server", "เปิดใช้งาน Web Server ในตัวสำหรับให้สคริปต์ภายนอกหรือบอทเชื่อมต่อเข้ามาได้");
            EnableWSCB.CheckedChanged += (s, e) =>
            {
                if (!SettingsLoaded) return;
                AccountManager.Developer.Set("EnableWebServer", EnableWSCB.Checked ? "true" : "false");
                AccountManager.IniSettings.Save("RAMSettings.ini");
                MessageBox.Show("Roblox Account Manager must be restarted to enable this setting", "Roblox Account Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            panel.Controls.Add(EnableWSCB);

            PortLabel = new Label { Text = "Web Server Port:", AutoSize = true, ForeColor = Color.FromArgb(240, 243, 254) };
            PortNumber = new NumericUpDown { Maximum = 65535, Minimum = 1, Width = 100, Value = 7963 };
            PortNumber.ValueChanged += (s, e) => { if (SettingsLoaded) { AccountManager.WebServer.Set("WebServerPort", PortNumber.Value.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(CreateRowContainer(PortLabel, PortNumber));

            ERRPCB = CreateCheckBox("Every Request Requires Password", "บังคับให้ต้องส่ง WebServer Password ในทุกคำสั่ง API เพื่อความปลอดภัย");
            ERRPCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.WebServer.Set("EveryRequestRequiresPassword", ERRPCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(ERRPCB);

            AllowGCCB = CreateCheckBox("Allow GetCookie Method", "อนุญาตให้ API ภายนอกดึงข้อมูล Cookie บัญชีได้");
            AllowGCCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.WebServer.Set("AllowGetCookie", AllowGCCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(AllowGCCB);

            AllowGACB = CreateCheckBox("Allow GetAccounts Method", "อนุญาตให้ API ภายนอกดึงรายชื่อบัญชีทั้งหมดได้");
            AllowGACB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.WebServer.Set("AllowGetAccounts", AllowGACB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(AllowGACB);

            AllowLACB = CreateCheckBox("Allow LaunchAccount Method", "อนุญาตให้ API ภายนอกสั่งรันเข้าเกมในไอดีที่ต้องการได้");
            AllowLACB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.WebServer.Set("AllowLaunchAccount", AllowLACB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(AllowLACB);

            AllowAECB = CreateCheckBox("Allow Account Modification Methods", "อนุญาตให้ API ภายนอกแก้ไขข้อมูลบัญชีได้");
            AllowAECB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.WebServer.Set("AllowAccountEditing", AllowAECB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(AllowAECB);

            DisableImagesCB = CreateCheckBox("Disable Image Loading [Less RAM %]", "ปิดการโหลดภาพรูปโปรไฟล์เพื่อประหยัดหน่วยความจำ RAM");
            DisableImagesCB.CheckedChanged += (s, e) => { AccountManager.General.Set("DisableImages", DisableImagesCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); };
            panel.Controls.Add(DisableImagesCB);

            AllowExternalConnectionsCB = CreateCheckBox("Allow External Connections", "อนุญาตให้เครื่องอื่นในเครือข่าย LAN/Internet เชื่อมต่อมายัง WebServer ได้");
            AllowExternalConnectionsCB.CheckedChanged += (s, e) =>
            {
                if (!SettingsLoaded) return;
                AccountManager.WebServer.Set("AllowExternalConnections", AllowExternalConnectionsCB.Checked ? "true" : "false");
                AccountManager.IniSettings.Save("RAMSettings.ini");
                MessageBox.Show("Roblox Account Manager must be restarted to enable this setting\n\nThis setting requires admin privileges", "Roblox Account Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            panel.Controls.Add(AllowExternalConnectionsCB);

            WSPWLabel = new Label { Text = "Webserver Password:", AutoSize = true, ForeColor = Color.FromArgb(240, 243, 254) };
            PasswordTextBox = new TextBox { Width = 200, BackColor = Color.FromArgb(25, 30, 48), ForeColor = Color.White };
            PasswordTextBox.TextChanged += (s, e) =>
            {
                if (!SettingsLoaded) return;
                PasswordTextBox.Text = Regex.Replace(PasswordTextBox.Text, "[^0-9a-zA-Z ]", "");
                AccountManager.WebServer.Set("Password", PasswordTextBox.Text);
                AccountManager.IniSettings.Save("RAMSettings.ini");
            };
            panel.Controls.Add(CreateRowContainer(WSPWLabel, PasswordTextBox));

            DeveloperTab.Controls.Add(panel);
        }

        private void BuildMiscellaneousTab()
        {
            FlowLayoutPanel panel = CreateScrollableLayoutPanel();

            Label header = new Label
            {
                Text = "🛠️ MISCELLANEOUS SETTINGS",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 242, 254),
                AutoSize = true,
                Margin = new Padding(3, 0, 3, 16)
            };
            panel.Controls.Add(header);

            PresenceCB = CreateCheckBox("Show Account Presence", "แสดงสถานะการใช้งานไอดีย้อนหลังสดใหม่ (In-Game / Online / Offline)");
            PresenceCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("ShowPresence", PresenceCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(PresenceCB);

            PresenceUpdateLabel = new Label { Text = "Refresh Presence (min):", AutoSize = true, ForeColor = Color.FromArgb(240, 243, 254) };
            PresenceUpdateRateNum = new NumericUpDown { Maximum = 60, Minimum = 1, Width = 80, Value = 5 };
            PresenceUpdateRateNum.ValueChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("PresenceUpdateRate", PresenceUpdateRateNum.Value.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(CreateRowContainer(PresenceUpdateLabel, PresenceUpdateRateNum));

            UnlockFPSCB = CreateCheckBox("Unlock FPS", "ปลดล็อกเฟรมเรตเกม Roblox ให้สูงกว่า 60 FPS");
            UnlockFPSCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("UnlockFPS", UnlockFPSCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(UnlockFPSCB);

            FPSCapLabel = new Label { Text = "Max FPS Cap:", AutoSize = true, ForeColor = Color.FromArgb(240, 243, 254) };
            MaxFPSValue = new NumericUpDown { Maximum = 360, Minimum = 15, Width = 80, Value = 60 };
            MaxFPSValue.ValueChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("MaxFPSValue", MaxFPSValue.Value.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); } };
            panel.Controls.Add(CreateRowContainer(FPSCapLabel, MaxFPSValue));

            CustomClientSettingsDialog = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };
            OverrideWithCustomCB = CreateCheckBox("Use Custom ClientAppSettings", "ใช้ไฟล์คอนฟิก ClientAppSettings.json แบบกำหนดเองสำหรับ Roblox");
            OverrideWithCustomCB.CheckedChanged += (s, e) =>
            {
                if (!SettingsLoaded) return;
                UnlockFPSCB.Enabled = !OverrideWithCustomCB.Checked;
                void Remove()
                {
                    AccountManager.General.RemoveProperty("CustomClientSettings");
                    OverrideWithCustomCB.Checked = false;
                }
                if (OverrideWithCustomCB.Checked)
                {
                    if (CustomClientSettingsDialog.ShowDialog() == DialogResult.OK)
                    {
                        if (File.Exists(CustomClientSettingsDialog.FileName) && File.ReadAllText(CustomClientSettingsDialog.FileName).TryParseJson<object>(out _))
                        {
                            string FileName = Path.Combine(Environment.CurrentDirectory, "CustomClientAppSettings.json");
                            File.Copy(CustomClientSettingsDialog.FileName, FileName, true);
                            AccountManager.General.Set("CustomClientSettings", FileName);
                        }
                        else MessageBox.Show("Invalid file selected, make sure it contains valid JSON", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else Remove();
                }
                else Remove();
                AccountManager.IniSettings.Save("RAMSettings.ini");
            };
            panel.Controls.Add(OverrideWithCustomCB);

            ForceUpdateButton = new Button
            {
                Text = "⚡ Force Update",
                Width = 200,
                Height = 36,
                BackColor = Color.FromArgb(25, 30, 48),
                ForeColor = Color.FromArgb(255, 99, 132),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(3, 16, 3, 16)
            };
            ForceUpdateButton.FlatAppearance.BorderColor = Color.FromArgb(60, 255, 99, 132);
            ForceUpdateButton.Click += (s, e) =>
            {
                if (!Utilities.YesNoPrompt("Auto Update", "Are you sure you want to update?", "", false)) return;
                string AFN = Path.Combine(Directory.GetCurrentDirectory(), "Auto Update.exe");
                File.WriteAllBytes(AFN, File.ReadAllBytes(Application.ExecutablePath));
                Process.Start(AFN, "-update");
                Environment.Exit(1);
            };
            panel.Controls.Add(ForceUpdateButton);

            MiscellaneousTab.Controls.Add(panel);
        }

        public void LoadSettings()
        {
            AutoUpdateCB.Checked = AccountManager.General.Get<bool>("CheckForUpdates");
            AsyncJoinCB.Checked = AccountManager.General.Get<bool>("AsyncJoin");
            LaunchDelayNumber.Value = AccountManager.General.Get<decimal>("AccountJoinDelay");
            SavePasswordCB.Checked = AccountManager.General.Get<bool>("SavePasswords");
            DisableAgingAlertCB.Checked = AccountManager.General.Get<bool>("DisableAgingAlert");
            HideMRobloxCB.Checked = AccountManager.General.Get<bool>("HideRbxAlert");
            DisableImagesCB.Checked = AccountManager.General.Get<bool>("DisableImages");
            ShuffleLowestServerCB.Checked = AccountManager.General.Get<bool>("ShuffleChoosesLowestServer");
            MultiRobloxCB.Checked = AccountManager.General.Get<bool>("EnableMultiRbx");
            RegionFormatTB.Text = AccountManager.General.Get<string>("ServerRegionFormat");
            MaxRecentGamesNumber.Value = AccountManager.General.Get<int>("MaxRecentGames");

            EnableDMCB.Checked = AccountManager.Developer.Get<bool>("DevMode");
            EnableWSCB.Checked = AccountManager.Developer.Get<bool>("EnableWebServer");
            ERRPCB.Checked = AccountManager.WebServer.Get<bool>("EveryRequestRequiresPassword");
            AllowGCCB.Checked = AccountManager.WebServer.Get<bool>("AllowGetCookie");
            AllowGACB.Checked = AccountManager.WebServer.Get<bool>("AllowGetAccounts");
            AllowLACB.Checked = AccountManager.WebServer.Get<bool>("AllowLaunchAccount");
            AllowAECB.Checked = AccountManager.WebServer.Get<bool>("AllowAccountEditing");
            AllowExternalConnectionsCB.Checked = AccountManager.WebServer.Get<bool>("AllowExternalConnections");
            PasswordTextBox.Text = AccountManager.WebServer.Get("Password");
            PortNumber.Value = AccountManager.WebServer.Get<decimal>("WebServerPort");

            PresenceCB.Checked = AccountManager.General.Get<bool>("ShowPresence");
            PresenceUpdateRateNum.Value = AccountManager.General.Get<int>("PresenceUpdateRate");
            UnlockFPSCB.Checked = AccountManager.General.Get<bool>("UnlockFPS");
            MaxFPSValue.Value = AccountManager.General.Get<int>("MaxFPSValue");

            if (AccountManager.General.Exists("CustomClientSettings") && File.Exists(AccountManager.General.Get<string>("CustomClientSettings")))
            {
                OverrideWithCustomCB.Checked = true;
                UnlockFPSCB.Enabled = false;
            }

            try { StartupKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true); } catch { }
            if (StartupKey != null && StartupKey.GetValue(Application.ProductName) is string ExistingPath)
            {
                if (ExistingPath != Application.ExecutablePath) StartupKey.SetValue(Application.ProductName, Application.ExecutablePath);
                StartOnPCStartup.Checked = true;
            }

            int savedTheme = AccountManager.Watcher.Get<int>("SelectedThemeIndex");
            themeCombo.SelectedIndex = (savedTheme >= 0 && savedTheme < 4) ? savedTheme : 0;

            SettingsLoaded = true;
        }
    }
}

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

        private Panel CreateMainScrollContainer()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(24),
                BackColor = Color.FromArgb(15, 17, 26)
            };
        }

        private Panel CreateCardPanel(int x, int y, int width, int height)
        {
            Panel p = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.FromArgb(22, 26, 38),
                Padding = new Padding(16)
            };
            p.MakeRounded(12);
            return p;
        }

        private CheckBox CreateCheckBox(string text, int x, int y, string tooltipText)
        {
            CheckBox cb = new CheckBox
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            if (!string.IsNullOrEmpty(tooltipText)) Helper.SetToolTip(cb, tooltipText);
            return cb;
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(148, 163, 184)
            };
        }

        private void BuildGeneralTab()
        {
            Panel container = CreateMainScrollContainer();
            int top = 10;

            Label header = new Label
            {
                Text = "⚙️ GENERAL SETTINGS",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 242, 254),
                Location = new Point(0, top),
                AutoSize = true
            };
            container.Controls.Add(header);
            top += 40;

            // Card 1: Core Startup & App Settings
            Panel card1 = CreateCardPanel(0, top, 680, 290);
            top += 305;

            AutoUpdateCB = CreateCheckBox("Check for Updates (ตรวจสอบการอัปเดตใหม่อัตโนมัติ)", 16, 16, "ตรวจสอบการอัปเดตโปรแกรมใหม่อัตโนมัติเมื่อเปิดใช้งาน");
            AutoUpdateCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("CheckForUpdates", AutoUpdateCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            AsyncJoinCB = CreateCheckBox("Async Launching (เข้าเกมหลายไอดีแบบขนาน)", 16, 50, "เปิดใช้งานการรันล็อกอินหลายไอดีแบบขนาน (Async) ลดเวลารอระหว่างเข้าเกม");
            DelayLabel = CreateLabel("Launch Delay (s):", 340, 50);
            LaunchDelayNumber = new NumericUpDown { Location = new Point(460, 47), Size = new Size(70, 25), Maximum = 60, Minimum = 0, Value = 1, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
            AsyncJoinCB.CheckedChanged += (s, e) =>
            {
                LaunchDelayNumber.Enabled = !AsyncJoinCB.Checked;
                if (SettingsLoaded) { AccountManager.General.Set("AsyncJoin", AsyncJoinCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); }
            };
            LaunchDelayNumber.ValueChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("AccountJoinDelay", LaunchDelayNumber.Value.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            SavePasswordCB = CreateCheckBox("Save Passwords (บันทึกรหัสผ่านบัญชีอย่างปลอดภัย)", 16, 84, "บันทึกรหัสผ่านบัญชีเก็บไว้ในไฟล์ระบบอย่างปลอดภัย");
            SavePasswordCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("SavePasswords", SavePasswordCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            DisableAgingAlertCB = CreateCheckBox("Disable Aging Alert (ปิดการแจ้งเตือนไอดีไม่ได้เข้านาน)", 16, 118, "ปิดการแจ้งเตือนสัญลักษณ์แจ้งเตือนไอดีไม่ได้เข้าใช้นาน (Aging Dots)");
            DisableAgingAlertCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("DisableAgingAlert", DisableAgingAlertCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            HideMRobloxCB = CreateCheckBox("Hide Multi Roblox Alert (ปิดแจ้งเตือนป๊อปอัป Multi Roblox)", 16, 152, "ปิดการแสดงป๊อปอัปเตือนเรื่อง Multi Roblox");
            HideMRobloxCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("HideRbxAlert", HideMRobloxCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            StartOnPCStartup = CreateCheckBox("Run on Windows Startup (เปิดโปรแกรมอัตโนมัติพร้อม Windows)", 16, 186, "สั่งให้โปรแกรมเปิดทำงานอัตโนมัติเมื่อเปิดคอมพิวเตอร์ (Windows Startup)");
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

            ShuffleLowestServerCB = CreateCheckBox("Shuffle Chooses Lowest Server (สุ่มเข้าเซิร์ฟคนน้อยสุด)", 16, 220, "สุ่มเลือกเซิร์ฟเวอร์ที่มีจำนวนผู้เล่นน้อยที่สุด เพื่อลดความแออัด");
            ShuffleLowestServerCB.CheckedChanged += (s, e) => { AccountManager.General.Set("ShuffleChoosesLowestServer", ShuffleLowestServerCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); };

            MultiRobloxCB = CreateCheckBox("Multi Roblox (เปิด Roblox ได้หลายๆ จอ/ไอดีพร้อมกัน)", 16, 254, "อนุญาตให้เปิดโปรแกรมเกม Roblox พร้อมกันได้หลายๆ จอ/ไอดี");
            MultiRobloxCB.CheckedChanged += (s, e) =>
            {
                AccountManager.General.Set("EnableMultiRbx", MultiRobloxCB.Checked ? "true" : "false");
                AccountManager.IniSettings.Save("RAMSettings.ini");
                if (!AccountManager.Instance.UpdateMultiRoblox())
                    MessageBox.Show("Roblox is currently running, multi roblox will not work if roblox is open.", "Roblox Account Manager", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };

            card1.Controls.AddRange(new Control[] { AutoUpdateCB, AsyncJoinCB, DelayLabel, LaunchDelayNumber, SavePasswordCB, DisableAgingAlertCB, HideMRobloxCB, StartOnPCStartup, ShuffleLowestServerCB, MultiRobloxCB });
            container.Controls.Add(card1);

            // Card 2: Formatting, Theme & Security
            Panel card2 = CreateCardPanel(0, top, 680, 200);
            top += 215;

            RegionFormatLabel = CreateLabel("Region Format:", 16, 20);
            RegionFormatTB = new TextBox { Location = new Point(140, 17), Size = new Size(180, 25), BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            RegionFormatTB.TextChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("ServerRegionFormat", RegionFormatTB.Text); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            MRGLabel = CreateLabel("Max Recent Games:", 340, 20);
            MaxRecentGamesNumber = new NumericUpDown { Location = new Point(480, 17), Size = new Size(70, 25), Maximum = 30, Minimum = 1, Value = 5, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
            MaxRecentGamesNumber.ValueChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("MaxRecentGames", MaxRecentGamesNumber.Value.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            Label themeLabel = CreateLabel("🎨 เลือกธีมระบบ (Custom Preset Theme):", 16, 65);
            themeLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            themeCombo = new ComboBox { Location = new Point(16, 90), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
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

            EncryptionSelectionButton = new Button
            {
                Text = "🔑 Reset Encryption Method",
                Location = new Point(340, 86),
                Size = new Size(220, 32),
                BackColor = Color.FromArgb(45, 52, 75),
                ForeColor = Color.FromArgb(0, 242, 254),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            EncryptionSelectionButton.FlatAppearance.BorderSize = 0;
            EncryptionSelectionButton.MakeRounded(8);
            EncryptionSelectionButton.Click += (s, e) =>
            {
                if (Utilities.YesNoPrompt("Settings", "Change Encryption Method", "Are you sure you want to change how your data is encrypted?", false))
                    AccountManager.Instance.ResetEncryption(true);
            };

            RSLabel = new Label
            {
                Text = "* Some settings may require restarting the program (e.g. WebServer Port, Aging Alert)",
                Location = new Point(16, 145),
                AutoSize = true,
                ForeColor = Color.FromArgb(123, 132, 163),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic)
            };

            card2.Controls.AddRange(new Control[] { RegionFormatLabel, RegionFormatTB, MRGLabel, MaxRecentGamesNumber, themeLabel, themeCombo, EncryptionSelectionButton, RSLabel });
            container.Controls.Add(card2);

            GeneralTab.Controls.Add(container);
        }

        private void BuildDeveloperTab()
        {
            Panel container = CreateMainScrollContainer();
            int top = 10;

            Label header = new Label
            {
                Text = "💻 DEVELOPER & WEB SERVER SETTINGS",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 242, 254),
                Location = new Point(0, top),
                AutoSize = true
            };
            container.Controls.Add(header);
            top += 40;

            Panel card1 = CreateCardPanel(0, top, 680, 360);
            top += 375;

            EnableDMCB = CreateCheckBox("Enable Developer Mode (เปิดโหมดนักพัฒนา)", 16, 16, "เปิดโหมดนักพัฒนาเพื่อเข้าถึงเมนูและคำสั่งขั้นสูง");
            EnableDMCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.Developer.Set("DevMode", EnableDMCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            EnableWSCB = CreateCheckBox("Enable Web Server (เปิดใช้งาน HTTP WebServer ในตัว)", 16, 50, "เปิดใช้งาน Web Server ในตัวสำหรับให้สคริปต์ภายนอกหรือบอทเชื่อมต่อเข้ามาได้");
            EnableWSCB.CheckedChanged += (s, e) =>
            {
                if (!SettingsLoaded) return;
                AccountManager.Developer.Set("EnableWebServer", EnableWSCB.Checked ? "true" : "false");
                AccountManager.IniSettings.Save("RAMSettings.ini");
                MessageBox.Show("Roblox Account Manager must be restarted to enable this setting", "Roblox Account Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            PortLabel = CreateLabel("Web Server Port:", 360, 50);
            PortNumber = new NumericUpDown { Location = new Point(480, 47), Size = new Size(80, 25), Maximum = 65535, Minimum = 1, Value = 7963, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
            PortNumber.ValueChanged += (s, e) => { if (SettingsLoaded) { AccountManager.WebServer.Set("WebServerPort", PortNumber.Value.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            ERRPCB = CreateCheckBox("Every Request Requires Password (บังคับส่งรหัสผ่าน WebServer ทุก Request)", 16, 84, "บังคับให้ต้องส่ง WebServer Password ในทุกคำสั่ง API เพื่อความปลอดภัย");
            ERRPCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.WebServer.Set("EveryRequestRequiresPassword", ERRPCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            AllowGCCB = CreateCheckBox("Allow GetCookie Method (อนุญาต API ดึง Cookie)", 16, 118, "อนุญาตให้ API ภายนอกดึงข้อมูล Cookie บัญชีได้");
            AllowGCCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.WebServer.Set("AllowGetCookie", AllowGCCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            AllowGACB = CreateCheckBox("Allow GetAccounts Method (อนุญาต API ดึงรายชื่อบัญชี)", 16, 152, "อนุญาตให้ API ภายนอกดึงรายชื่อบัญชีทั้งหมดได้");
            AllowGACB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.WebServer.Set("AllowGetAccounts", AllowGACB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            AllowLACB = CreateCheckBox("Allow LaunchAccount Method (อนุญาต API สั่งเข้าเกมตามไอดี)", 16, 186, "อนุญาตให้ API ภายนอกสั่งรันเข้าเกมในไอดีที่ต้องการได้");
            AllowLACB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.WebServer.Set("AllowLaunchAccount", AllowLACB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            AllowAECB = CreateCheckBox("Allow Account Modification Methods (อนุญาต API แก้ไขข้อมูลบัญชี)", 16, 220, "อนุญาตให้ API ภายนอกแก้ไขข้อมูลบัญชีได้");
            AllowAECB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.WebServer.Set("AllowAccountEditing", AllowAECB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            DisableImagesCB = CreateCheckBox("Disable Image Loading [Less RAM %] (ปิดการโหลดรูปประหยัด RAM)", 16, 254, "ปิดการโหลดภาพรูปโปรไฟล์เพื่อประหยัดหน่วยความจำ RAM");
            DisableImagesCB.CheckedChanged += (s, e) => { AccountManager.General.Set("DisableImages", DisableImagesCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); };

            AllowExternalConnectionsCB = CreateCheckBox("Allow External Connections (อนุญาตการเชื่อมต่อภายนอก LAN/Internet)", 16, 288, "อนุญาตให้เครื่องอื่นในเครือข่าย LAN/Internet เชื่อมต่อมายัง WebServer ได้");
            AllowExternalConnectionsCB.CheckedChanged += (s, e) =>
            {
                if (!SettingsLoaded) return;
                AccountManager.WebServer.Set("AllowExternalConnections", AllowExternalConnectionsCB.Checked ? "true" : "false");
                AccountManager.IniSettings.Save("RAMSettings.ini");
                MessageBox.Show("Roblox Account Manager must be restarted to enable this setting\n\nThis setting requires admin privileges", "Roblox Account Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            WSPWLabel = CreateLabel("Webserver Password:", 16, 325);
            PasswordTextBox = new TextBox { Location = new Point(160, 322), Size = new Size(200, 25), BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            PasswordTextBox.TextChanged += (s, e) =>
            {
                if (!SettingsLoaded) return;
                PasswordTextBox.Text = Regex.Replace(PasswordTextBox.Text, "[^0-9a-zA-Z ]", "");
                AccountManager.WebServer.Set("Password", PasswordTextBox.Text);
                AccountManager.IniSettings.Save("RAMSettings.ini");
            };

            card1.Controls.AddRange(new Control[] { EnableDMCB, EnableWSCB, PortLabel, PortNumber, ERRPCB, AllowGCCB, AllowGACB, AllowLACB, AllowAECB, DisableImagesCB, AllowExternalConnectionsCB, WSPWLabel, PasswordTextBox });
            container.Controls.Add(card1);

            DeveloperTab.Controls.Add(container);
        }

        private void BuildMiscellaneousTab()
        {
            Panel container = CreateMainScrollContainer();
            int top = 10;

            Label header = new Label
            {
                Text = "🛠️ MISCELLANEOUS SETTINGS",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 242, 254),
                Location = new Point(0, top),
                AutoSize = true
            };
            container.Controls.Add(header);
            top += 40;

            Panel card1 = CreateCardPanel(0, top, 680, 240);
            top += 255;

            PresenceCB = CreateCheckBox("Show Account Presence (แสดงสถานะบัญชี In-Game/Online)", 16, 16, "แสดงสถานะการใช้งานไอดีย้อนหลังสดใหม่ (In-Game / Online / Offline)");
            PresenceCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("ShowPresence", PresenceCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            PresenceUpdateLabel = CreateLabel("Refresh Presence (min):", 400, 16);
            PresenceUpdateRateNum = new NumericUpDown { Location = new Point(540, 13), Size = new Size(70, 25), Maximum = 60, Minimum = 1, Value = 5, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
            PresenceUpdateRateNum.ValueChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("PresenceUpdateRate", PresenceUpdateRateNum.Value.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            UnlockFPSCB = CreateCheckBox("Unlock FPS (ปลดล็อกเฟรมเรต Roblox)", 16, 55, "ปลดล็อกเฟรมเรตเกม Roblox ให้สูงกว่า 60 FPS");
            UnlockFPSCB.CheckedChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("UnlockFPS", UnlockFPSCB.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            FPSCapLabel = CreateLabel("Max FPS Cap:", 400, 55);
            MaxFPSValue = new NumericUpDown { Location = new Point(540, 52), Size = new Size(70, 25), Maximum = 360, Minimum = 15, Value = 60, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
            MaxFPSValue.ValueChanged += (s, e) => { if (SettingsLoaded) { AccountManager.General.Set("MaxFPSValue", MaxFPSValue.Value.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); } };

            CustomClientSettingsDialog = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };
            OverrideWithCustomCB = CreateCheckBox("Use Custom ClientAppSettings (ใช้ไฟล์คอนฟิก Roblox แบบกำหนดเอง)", 16, 95, "ใช้ไฟล์คอนฟิก ClientAppSettings.json แบบกำหนดเองสำหรับ Roblox");
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

            ForceUpdateButton = new Button
            {
                Text = "⚡ Force Update Application",
                Location = new Point(16, 150),
                Size = new Size(240, 36),
                BackColor = Color.FromArgb(45, 52, 75),
                ForeColor = Color.FromArgb(255, 99, 132),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            ForceUpdateButton.FlatAppearance.BorderSize = 0;
            ForceUpdateButton.MakeRounded(8);
            ForceUpdateButton.Click += (s, e) =>
            {
                if (!Utilities.YesNoPrompt("Auto Update", "Are you sure you want to update?", "", false)) return;
                string AFN = Path.Combine(Directory.GetCurrentDirectory(), "Auto Update.exe");
                File.WriteAllBytes(AFN, File.ReadAllBytes(Application.ExecutablePath));
                Process.Start(AFN, "-update");
                Environment.Exit(1);
            };

            card1.Controls.AddRange(new Control[] { PresenceCB, PresenceUpdateLabel, PresenceUpdateRateNum, UnlockFPSCB, FPSCapLabel, MaxFPSValue, OverrideWithCustomCB, ForceUpdateButton });
            container.Controls.Add(card1);

            MiscellaneousTab.Controls.Add(container);
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

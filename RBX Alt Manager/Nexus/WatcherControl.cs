using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using RBX_Alt_Manager.Classes;

namespace RBX_Alt_Manager.Nexus
{
    public class WatcherControl : UserControl
    {
        private CheckBox chkEnableWatcher;
        private NumericUpDown numScanInterval;
        private NumericUpDown numReadInterval;

        private CheckBox chkExitOnBeta;
        private CheckBox chkExitNoConnection;
        private NumericUpDown numTimeout;

        private CheckBox chkSaveWindowPos;
        private CheckBox chkVerifyDataModel;
        private CheckBox chkIgnoreExisting;

        private CheckBox chkCloseRbxMemory;
        private NumericUpDown numMemoryLow;

        private CheckBox chkCloseWindowTitle;
        private TextBox txtExpectedWindowTitle;

        private Button btnOpenLogs;

        public WatcherControl()
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

            Panel mainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(24),
                BackColor = Color.FromArgb(15, 17, 26)
            };

            int top = 10;

            // Title Header
            Label lblHeader = new Label
            {
                Text = "👁️ ROBLOX WATCHER SETTINGS",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 242, 254),
                Location = new Point(0, top),
                AutoSize = true
            };
            mainContainer.Controls.Add(lblHeader);
            top += 35;

            Label lblSubHeader = new Label
            {
                Text = "ระบบเฝ้าระวังและตรวจจับสถานะของจอ Roblox (ค้าง, หลุด, เมนูหลัก, ใช้แรมผิดปกติ)",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(0, top),
                AutoSize = true
            };
            mainContainer.Controls.Add(lblSubHeader);
            top += 40;

            // Card 1: Main Control & Intervals
            Panel pnlMain = CreateCardPanel(top, 160);
            top += 175;

            chkEnableWatcher = CreateCheckBox("Enable Roblox Watcher (เปิดระบบตรวจจับสถานะ Roblox)", 15, 15);
            chkEnableWatcher.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            chkEnableWatcher.ForeColor = Color.FromArgb(34, 197, 94);

            Label lblScan = CreateLabel("Scan Interval (sec):", 15, 60);
            numScanInterval = CreateNumericUpDown(180, 56, 1, 3600, 6);

            Label lblRead = CreateLabel("Read Interval (ms):", 15, 100);
            numReadInterval = CreateNumericUpDown(180, 96, 50, 10000, 250);

            pnlMain.Controls.AddRange(new Control[] { chkEnableWatcher, lblScan, numScanInterval, lblRead, numReadInterval });
            mainContainer.Controls.Add(pnlMain);

            // Card 2: Detection Rules
            Panel pnlRules = CreateCardPanel(top, 240);
            top += 255;

            chkExitOnBeta = CreateCheckBox("Exit if Beta Home Menu Detected (ปิดทันทีเมื่อเด้งกลับหน้าเมนูหลัก)", 15, 15);
            
            chkExitNoConnection = CreateCheckBox("Exit if No Connection to Server for", 15, 55);
            numTimeout = CreateNumericUpDown(270, 52, 5, 3600, 60);
            Label lblSeconds = CreateLabel("Seconds", 360, 55);

            chkSaveWindowPos = CreateCheckBox("Save Window Positions (จำตำแหน่งหน้าต่าง Roblox)", 15, 95);
            chkVerifyDataModel = CreateCheckBox("Data Model Verification (ตรวจสอบความถูกต้องของการโหลดฉาก)", 15, 135);
            chkIgnoreExisting = CreateCheckBox("Ignore Existing Processes During Startup (ข้ามการสแกนจอที่เปิดค้างไว้ก่อนหน้า)", 15, 175);

            pnlRules.Controls.AddRange(new Control[] { chkExitOnBeta, chkExitNoConnection, numTimeout, lblSeconds, chkSaveWindowPos, chkVerifyDataModel, chkIgnoreExisting });
            mainContainer.Controls.Add(pnlRules);

            // Card 3: Memory & Title Verification
            Panel pnlAdvanced = CreateCardPanel(top, 190);
            top += 205;

            chkCloseRbxMemory = CreateCheckBox("Close Roblox if Memory is Less Than", 15, 15);
            numMemoryLow = CreateNumericUpDown(290, 12, 10, 16000, 200);
            Label lblMb = CreateLabel("MB", 380, 15);

            chkCloseWindowTitle = CreateCheckBox("Close Roblox if WindowTitle Isn't", 15, 60);
            txtExpectedWindowTitle = new TextBox
            {
                Location = new Point(270, 57),
                Size = new Size(160, 25),
                BackColor = Color.FromArgb(30, 35, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Roblox"
            };

            btnOpenLogs = new Button
            {
                Text = "📁 Open Roblox Logs Folder",
                Location = new Point(15, 115),
                Size = new Size(240, 34),
                BackColor = Color.FromArgb(45, 52, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnOpenLogs.FlatAppearance.BorderSize = 0;
            btnOpenLogs.MakeRounded(10);
            btnOpenLogs.Click += (s, e) =>
            {
                try
                {
                    Process.Start("explorer.exe", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox", "logs"));
                }
                catch { }
            };

            pnlAdvanced.Controls.AddRange(new Control[] { chkCloseRbxMemory, numMemoryLow, lblMb, chkCloseWindowTitle, txtExpectedWindowTitle, btnOpenLogs });
            mainContainer.Controls.Add(pnlAdvanced);

            this.Controls.Add(mainContainer);

            // Event Handlers for Live Saving
            BindEvents();
        }

        private Panel CreateCardPanel(int top, int height)
        {
            Panel p = new Panel
            {
                Location = new Point(0, top),
                Size = new Size(680, height),
                BackColor = Color.FromArgb(22, 26, 38),
                Padding = new Padding(15)
            };
            p.MakeRounded(12);
            return p;
        }

        private CheckBox CreateCheckBox(string text, int x, int y)
        {
            return new CheckBox
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
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

        private NumericUpDown CreateNumericUpDown(int x, int y, decimal min, decimal max, decimal val)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(80, 25),
                Minimum = min,
                Maximum = max,
                Value = val,
                BackColor = Color.FromArgb(30, 35, 50),
                ForeColor = Color.White
            };
        }

        private void LoadSettings()
        {
            try
            {
                chkEnableWatcher.Checked = AccountManager.Watcher.Get<bool>("Enabled");
                chkExitOnBeta.Checked = AccountManager.Watcher.Get<bool>("ExitOnBeta");
                chkExitNoConnection.Checked = AccountManager.Watcher.Get<bool>("ExitIfNoConnection");
                chkSaveWindowPos.Checked = AccountManager.Watcher.Get<bool>("SaveWindowPositions");
                chkVerifyDataModel.Checked = AccountManager.Watcher.Exists("VerifyDataModel") ? AccountManager.Watcher.Get<bool>("VerifyDataModel") : true;
                chkIgnoreExisting.Checked = AccountManager.Watcher.Exists("IgnoreExistingProcesses") ? AccountManager.Watcher.Get<bool>("IgnoreExistingProcesses") : true;
                chkCloseRbxMemory.Checked = AccountManager.Watcher.Get<bool>("CloseRbxMemory");
                chkCloseWindowTitle.Checked = AccountManager.Watcher.Get<bool>("CloseRbxWindowTitle");

                txtExpectedWindowTitle.Text = AccountManager.Watcher.Exists("ExpectedWindowTitle") ? AccountManager.Watcher.Get<string>("ExpectedWindowTitle") : "Roblox";
                numMemoryLow.Value = AccountManager.Watcher.Exists("MemoryLowValue") ? Utilities.Clamp(AccountManager.Watcher.Get<decimal>("MemoryLowValue"), numMemoryLow.Minimum, numMemoryLow.Maximum) : 200;
                numTimeout.Value = AccountManager.Watcher.Exists("NoConnectionTimeout") ? Utilities.Clamp(AccountManager.Watcher.Get<decimal>("NoConnectionTimeout"), numTimeout.Minimum, numTimeout.Maximum) : 60;

                numScanInterval.Value = AccountManager.Watcher.Exists("ScanInterval") ? AccountManager.Watcher.Get<int>("ScanInterval") : 6;
                numReadInterval.Value = AccountManager.Watcher.Exists("ReadInterval") ? AccountManager.Watcher.Get<int>("ReadInterval") : 250;
            }
            catch { }
        }

        private void SaveSetting(string key, string val)
        {
            try
            {
                AccountManager.Watcher.Set(key, val);
                AccountManager.IniSettings.Save("RAMSettings.ini");
            }
            catch { }
        }

        private void BindEvents()
        {
            chkEnableWatcher.CheckedChanged += (s, e) => SaveSetting("Enabled", chkEnableWatcher.Checked.ToString().ToLower());
            chkExitOnBeta.CheckedChanged += (s, e) => SaveSetting("ExitOnBeta", chkExitOnBeta.Checked.ToString().ToLower());
            chkExitNoConnection.CheckedChanged += (s, e) => SaveSetting("ExitIfNoConnection", chkExitNoConnection.Checked.ToString().ToLower());
            chkSaveWindowPos.CheckedChanged += (s, e) =>
            {
                RobloxWatcher.RememberWindowPositions = chkSaveWindowPos.Checked;
                SaveSetting("SaveWindowPositions", chkSaveWindowPos.Checked.ToString().ToLower());
            };
            chkVerifyDataModel.CheckedChanged += (s, e) =>
            {
                RobloxWatcher.VerifyDataModel = chkVerifyDataModel.Checked;
                SaveSetting("VerifyDataModel", chkVerifyDataModel.Checked.ToString().ToLower());
            };
            chkIgnoreExisting.CheckedChanged += (s, e) =>
            {
                RobloxWatcher.IgnoreExistingProcesses = chkIgnoreExisting.Checked;
                SaveSetting("IgnoreExistingProcesses", chkIgnoreExisting.Checked.ToString().ToLower());
            };
            chkCloseRbxMemory.CheckedChanged += (s, e) =>
            {
                RobloxWatcher.CloseIfMemoryLow = chkCloseRbxMemory.Checked;
                SaveSetting("CloseRbxMemory", chkCloseRbxMemory.Checked.ToString().ToLower());
            };
            chkCloseWindowTitle.CheckedChanged += (s, e) =>
            {
                RobloxWatcher.CloseIfWindowTitle = chkCloseWindowTitle.Checked;
                SaveSetting("CloseRbxWindowTitle", chkCloseWindowTitle.Checked.ToString().ToLower());
            };

            numScanInterval.ValueChanged += (s, e) => SaveSetting("ScanInterval", numScanInterval.Value.ToString());
            numReadInterval.ValueChanged += (s, e) => SaveSetting("ReadInterval", numReadInterval.Value.ToString());
            numTimeout.ValueChanged += (s, e) => SaveSetting("NoConnectionTimeout", numTimeout.Value.ToString());
            numMemoryLow.ValueChanged += (s, e) =>
            {
                RobloxWatcher.MemoryLowValue = (int)numMemoryLow.Value;
                SaveSetting("MemoryLowValue", numMemoryLow.Value.ToString());
            };

            txtExpectedWindowTitle.TextChanged += (s, e) =>
            {
                RobloxWatcher.ExpectedWindowTitle = txtExpectedWindowTitle.Text;
                SaveSetting("ExpectedWindowTitle", txtExpectedWindowTitle.Text);
            };
        }
    }
}

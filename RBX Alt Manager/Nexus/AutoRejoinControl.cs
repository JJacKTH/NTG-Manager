using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using RBX_Alt_Manager.Classes;

namespace RBX_Alt_Manager.Nexus
{
    public class AutoRejoinControl : UserControl
    {
        // UI Controls - Top Stats Cards (3 Cards in TableLayoutPanel for Responsive Auto-Filling)
        private TableLayoutPanel topCardsTableLayout;
        private Panel panelIdleCard;
        private Panel panelOnlineCard;
        private Panel panelOfflineCard;

        // UI Controls - Table & Footer
        private ListView listViewAccounts;
        private ColumnHeader colNum;
        private ColumnHeader colUsername;
        private ColumnHeader colPlaceId;
        private ColumnHeader colJobId;
        private ColumnHeader colStatus;
        private ColumnHeader colPid;
        private ColumnHeader colUptime;

        private Panel footerPanel;
        private Label lblSelectedCount;

        // UI Controls - Right Panel
        private Panel rightControlPanel;
        private CheckBox chkAutoRelaunch;
        private TextBox txtPlaceId;
        private TextBox txtJobId;
        private Button btnSavePlaceJob;
        private Button btnArrangeWindows;
        private Button btnMinimizeWindows;
        private Button btnCloseRoblox;
        private ComboBox comboCpuCore;
        private ComboBox comboRamLimit;
        private TextBox txtDelay;
        private TextBox txtDelayOpenRblx;
        private TextBox txtOfflineTime;

        // Win32 API for Window Rearrange & Minimize
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        private const int SW_MINIMIZE = 6;

        private System.Windows.Forms.Timer realTimeTimer;

        public AutoRejoinControl()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(15, 17, 26);
            this.Dock = DockStyle.Fill;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            InitializeComponents();
            RefreshAccountsList();

            realTimeTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            realTimeTimer.Tick += (s, e) => UpdateRealTimeStatus();
            realTimeTimer.Start();
        }

        private void InitializeComponents()
        {
            this.Controls.Clear();

            // 1. Top Panel for Stats Cards - Uses TableLayoutPanel to full width expand dynamically
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                Padding = new Padding(12, 10, 12, 5),
                BackColor = Color.Transparent
            };

            topCardsTableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            topCardsTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            topCardsTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            topCardsTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            topCardsTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            panelIdleCard = CreateCard("Paused Accounts", "IDLE 0", Color.FromArgb(234, 179, 8), Color.FromArgb(25, 28, 40));
            panelOnlineCard = CreateCard("Farming / Online Accounts", "ONLINE 0", Color.FromArgb(34, 197, 94), Color.FromArgb(25, 28, 40));
            panelOfflineCard = CreateCard("Disconnected / Offline", "OFFLINE 0", Color.FromArgb(239, 68, 68), Color.FromArgb(25, 28, 40));

            panelIdleCard.Margin = new Padding(4);
            panelOnlineCard.Margin = new Padding(4);
            panelOfflineCard.Margin = new Padding(4);

            panelIdleCard.Dock = DockStyle.Fill;
            panelOnlineCard.Dock = DockStyle.Fill;
            panelOfflineCard.Dock = DockStyle.Fill;

            topCardsTableLayout.Controls.Add(panelIdleCard, 0, 0);
            topCardsTableLayout.Controls.Add(panelOnlineCard, 1, 0);
            topCardsTableLayout.Controls.Add(panelOfflineCard, 2, 0);

            topPanel.Controls.Add(topCardsTableLayout);

            // 2. Right Control Panel (Auto Relaunch Settings)
            rightControlPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 370,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(20, 24, 36),
                AutoScroll = true
            };

            BuildRightControlPanel();

            // 3. Footer Bar at Bottom
            footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                BackColor = Color.FromArgb(22, 26, 38),
                Padding = new Padding(12, 0, 12, 0)
            };

            lblSelectedCount = new Label
            {
                Text = "เลือกอยู่ 0 รายการ / ทั้งหมด 0 รายการ",
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true
            };
            footerPanel.Controls.Add(lblSelectedCount);

            // 4. Center ListView (Accounts Table with Status Highlight & Border)
            listViewAccounts = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BackColor = Color.FromArgb(18, 20, 30),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
            };

            colNum = new ColumnHeader { Text = "#", Width = 45 };
            colUsername = new ColumnHeader { Text = "Username", Width = 150 };
            colPlaceId = new ColumnHeader { Text = "Place-ID", Width = 120 };
            colJobId = new ColumnHeader { Text = "JobId", Width = 110 };
            colStatus = new ColumnHeader { Text = "Status", Width = 110 };
            colPid = new ColumnHeader { Text = "PID Process", Width = 100 };
            colUptime = new ColumnHeader { Text = "Uptime", Width = 100 };

            listViewAccounts.Columns.AddRange(new[] { colNum, colUsername, colPlaceId, colJobId, colStatus, colPid, colUptime });
            listViewAccounts.OwnerDraw = true;
            listViewAccounts.DrawColumnHeader += ListViewAccounts_DrawColumnHeader;
            listViewAccounts.DrawItem += ListViewAccounts_DrawItem;
            listViewAccounts.DrawSubItem += ListViewAccounts_DrawSubItem;
            listViewAccounts.SelectedIndexChanged += ListViewAccounts_SelectedIndexChanged;

            this.Controls.Add(listViewAccounts);
            this.Controls.Add(footerPanel);
            this.Controls.Add(rightControlPanel);
            this.Controls.Add(topPanel);
        }

        private Panel CreateCard(string title, string countText, Color statusColor, Color bgColor)
        {
            Panel p = new Panel
            {
                BackColor = bgColor,
                Padding = new Padding(12, 10, 12, 10)
            };
            p.MakeRounded(12);

            Label lblTitle = new Label
            {
                Text = title,
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 22
            };

            Label lblCount = new Label
            {
                Text = countText,
                ForeColor = statusColor,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            p.Controls.Add(lblCount);
            p.Controls.Add(lblTitle);
            return p;
        }

        private void BuildRightControlPanel()
        {
            int top = 15;

            // 1. ⚡ AUTO RELAUNCH & QUICK LAUNCH
            Label lblHeaderRelaunch = new Label { Text = "⚡ AUTO RELAUNCH & QUICK LAUNCH", ForeColor = Color.FromArgb(0, 242, 254), Location = new Point(15, top), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            rightControlPanel.Controls.Add(lblHeaderRelaunch);
            top += 25;

            chkAutoRelaunch = new CheckBox
            {
                Text = "⚡ Enable Auto Relaunch",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, top),
                AutoSize = true,
                Checked = AccountManager.Watcher.Get<bool>("AutoRejoinEnabled")
            };
            chkAutoRelaunch.CheckedChanged += (s, e) =>
            {
                AccountManager.Watcher.Set("AutoRejoinEnabled", chkAutoRelaunch.Checked.ToString().ToLower());
            };
            rightControlPanel.Controls.Add(chkAutoRelaunch);
            top += 35;

            Label lblPlace = new Label { Text = "placeId :", ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(15, top + 3), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtPlaceId = new TextBox { Location = new Point(85, top), Size = new Size(245, 25), BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Text = AccountManager.CurrentPlaceId };
            rightControlPanel.Controls.AddRange(new Control[] { lblPlace, txtPlaceId });
            top += 35;

            Label lblJob = new Label { Text = "JobId :", ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(15, top + 3), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtJobId = new TextBox { Location = new Point(85, top), Size = new Size(245, 25), BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Text = string.IsNullOrEmpty(AccountManager.CurrentJobId) ? "None" : AccountManager.CurrentJobId };
            rightControlPanel.Controls.AddRange(new Control[] { lblJob, txtJobId });
            top += 35;

            btnSavePlaceJob = new Button
            {
                Text = "💾 บันทึกตั้งค่า PlaceID & JobID",
                Location = new Point(15, top),
                Size = new Size(315, 32),
                BackColor = Color.FromArgb(0, 230, 118),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSavePlaceJob.FlatAppearance.BorderSize = 0;
            btnSavePlaceJob.MakeRounded(8);
            btnSavePlaceJob.Click += (s, e) =>
            {
                if (AccountManager.AccountsList != null)
                {
                    List<string> targetUsernames = new List<string>();
                    if (listViewAccounts.SelectedItems.Count > 0)
                    {
                        foreach (ListViewItem item in listViewAccounts.SelectedItems)
                        {
                            if (item.SubItems.Count > 1) targetUsernames.Add(item.SubItems[1].Text);
                        }
                    }

                    var targets = AccountManager.AccountsList.Where(a =>
                        targetUsernames.Count > 0
                            ? targetUsernames.Contains(a.Username)
                            : a.GetField("AddToRelaunch") == "true"
                    ).ToList();

                    if (targets.Count == 0) targets = AccountManager.AccountsList;

                    foreach (var acc in targets)
                    {
                        acc.SetField("SavedPlaceId", txtPlaceId.Text);
                        acc.SetField("SavedJobId", txtJobId.Text == "None" ? "" : txtJobId.Text);
                    }

                    AccountManager.SaveAccounts();
                    string msg = targetUsernames.Count > 0
                        ? $"บันทึก PlaceID และ JobID สำหรับ {targetUsernames.Count} บัญชีที่เลือกเรียบร้อยแล้ว!"
                        : "บันทึก PlaceID และ JobID สำหรับทุกบัญชี Auto Relaunch เรียบร้อยแล้ว!";
                    MessageBox.Show(msg, "บันทึกสำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshAccountsList();
                }
            };
            rightControlPanel.Controls.Add(btnSavePlaceJob);
            top += 42;

            // Action Buttons
            btnArrangeWindows = new Button { Text = "🖼️ Arrange Windows", Location = new Point(15, top), Size = new Size(152, 30), BackColor = Color.FromArgb(45, 52, 75), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
            btnArrangeWindows.FlatAppearance.BorderSize = 0;
            btnArrangeWindows.MakeRounded(8);
            btnArrangeWindows.Click += BtnArrangeWindows_Click;

            btnMinimizeWindows = new Button { Text = "🔽 Minimize", Location = new Point(173, top), Size = new Size(75, 30), BackColor = Color.FromArgb(45, 52, 75), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
            btnMinimizeWindows.FlatAppearance.BorderSize = 0;
            btnMinimizeWindows.MakeRounded(8);
            btnMinimizeWindows.Click += BtnMinimizeWindows_Click;

            btnCloseRoblox = new Button { Text = "❌ Close", Location = new Point(254, top), Size = new Size(76, 30), BackColor = Color.FromArgb(180, 40, 40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
            btnCloseRoblox.FlatAppearance.BorderSize = 0;
            btnCloseRoblox.MakeRounded(8);
            btnCloseRoblox.Click += BtnCloseRoblox_Click;

            rightControlPanel.Controls.AddRange(new Control[] { btnArrangeWindows, btnMinimizeWindows, btnCloseRoblox });
            top += 45;

            // 2. ⚙️ NEXUS CONTROL SETTINGS (Imported from Account Control)
            Label lblHeaderNexus = new Label { Text = "⚙️ NEXUS & CONTROL SETTINGS", ForeColor = Color.FromArgb(0, 242, 254), Location = new Point(15, top), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            rightControlPanel.Controls.Add(lblHeaderNexus);
            top += 25;

            CheckBox chkStartOnLaunch = new CheckBox { Text = "Start Nexus on Account Manager Launch", Location = new Point(15, top), AutoSize = true, ForeColor = Color.White, Checked = AccountManager.AccountControl.Get<bool>("StartOnLaunch") };
            chkStartOnLaunch.CheckedChanged += (s, e) => { AccountManager.AccountControl.Set("StartOnLaunch", chkStartOnLaunch.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); };
            top += 26;

            CheckBox chkAllowExternal = new CheckBox { Text = "Allow External Connections", Location = new Point(15, top), AutoSize = true, ForeColor = Color.White, Checked = AccountManager.AccountControl.Get<bool>("AllowExternalConnections") };
            chkAllowExternal.CheckedChanged += (s, e) => { AccountManager.AccountControl.Set("AllowExternalConnections", chkAllowExternal.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); };
            top += 26;

            CheckBox chkInternetCheck = new CheckBox { Text = "Check for Internet Before Launch", Location = new Point(15, top), AutoSize = true, ForeColor = Color.White, Checked = AccountManager.AccountControl.Get<bool>("InternetCheck") };
            chkInternetCheck.CheckedChanged += (s, e) => { AccountManager.AccountControl.Set("InternetCheck", chkInternetCheck.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); };
            top += 26;

            CheckBox chkUsePresence = new CheckBox { Text = "Use Presence API", Location = new Point(15, top), AutoSize = true, ForeColor = Color.White, Checked = AccountManager.AccountControl.Get<bool>("UsePresence") };
            chkUsePresence.CheckedChanged += (s, e) => { AccountManager.AccountControl.Set("UsePresence", chkUsePresence.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); };
            top += 32;

            rightControlPanel.Controls.AddRange(new Control[] { chkStartOnLaunch, chkAllowExternal, chkInternetCheck, chkUsePresence });

            Label lRelaunchDelay = new Label { Text = "Relaunch Delay Per Account (sec):", Location = new Point(15, top + 3), AutoSize = true, ForeColor = Color.FromArgb(148, 163, 184) };
            NumericUpDown numRelaunchDelay = new NumericUpDown { Location = new Point(245, top), Size = new Size(85, 24), Minimum = 1, Maximum = 9999, Value = decimal.TryParse(AccountManager.AccountControl.Get("RelaunchDelay"), out decimal rd) ? rd : 60, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
            numRelaunchDelay.ValueChanged += (s, e) => { AccountManager.AccountControl.Set("RelaunchDelay", numRelaunchDelay.Value.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); };
            top += 30;

            Label lLauncherDelay = new Label { Text = "Launcher Delay (sec):", Location = new Point(15, top + 3), AutoSize = true, ForeColor = Color.FromArgb(148, 163, 184) };
            NumericUpDown numLauncherDelay = new NumericUpDown { Location = new Point(245, top), Size = new Size(85, 24), Minimum = 1, Maximum = 9999, Value = decimal.TryParse(AccountManager.AccountControl.Get("LauncherDelay"), out decimal ld) ? ld : 15, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
            numLauncherDelay.ValueChanged += (s, e) => { AccountManager.AccountControl.Set("LauncherDelay", numLauncherDelay.Value.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); };
            top += 30;

            Label lPort = new Label { Text = "Port:", Location = new Point(15, top + 3), AutoSize = true, ForeColor = Color.FromArgb(148, 163, 184) };
            NumericUpDown numPort = new NumericUpDown { Location = new Point(245, top), Size = new Size(85, 24), Minimum = 1, Maximum = 65535, Value = decimal.TryParse(AccountManager.AccountControl.Get("NexusPort"), out decimal pVal) ? pVal : 5242, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
            numPort.ValueChanged += (s, e) => { AccountManager.AccountControl.Set("NexusPort", numPort.Value.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); };
            top += 38;

            rightControlPanel.Controls.AddRange(new Control[] { lRelaunchDelay, numRelaunchDelay, lLauncherDelay, numLauncherDelay, lPort, numPort });

            // 3. 🔄 AUTO MINIMIZE & AUTO CLOSE
            Label lblHeaderAutoOpt = new Label { Text = "🔄 AUTO MINIMIZE & AUTO CLOSE", ForeColor = Color.FromArgb(0, 242, 254), Location = new Point(15, top), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            rightControlPanel.Controls.Add(lblHeaderAutoOpt);
            top += 25;

            CheckBox chkAutoMinimize = new CheckBox { Text = "Auto Minimize", Location = new Point(15, top), AutoSize = true, ForeColor = Color.White, Checked = AccountManager.AccountControl.Get<bool>("AutoMinimizeEnabled") };
            chkAutoMinimize.CheckedChanged += (s, e) => { AccountManager.AccountControl.Set("AutoMinimizeEnabled", chkAutoMinimize.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); };

            Label lMinInterval = new Label { Text = "Interval (sec):", Location = new Point(150, top + 2), AutoSize = true, ForeColor = Color.FromArgb(148, 163, 184) };
            NumericUpDown numAutoMinInterval = new NumericUpDown { Location = new Point(245, top), Size = new Size(85, 24), Minimum = 1, Maximum = 99999, Value = decimal.TryParse(AccountManager.AccountControl.Get("AutoMinimizeInterval"), out decimal mi) ? mi : 3000, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
            numAutoMinInterval.ValueChanged += (s, e) => { AccountManager.AccountControl.Set("AutoMinimizeInterval", numAutoMinInterval.Value.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); };
            top += 30;

            CheckBox chkAutoClose = new CheckBox { Text = "Auto Close", Location = new Point(15, top), AutoSize = true, ForeColor = Color.White, Checked = AccountManager.AccountControl.Get<bool>("AutoCloseEnabled") };
            chkAutoClose.CheckedChanged += (s, e) => { AccountManager.AccountControl.Set("AutoCloseEnabled", chkAutoClose.Checked ? "true" : "false"); AccountManager.IniSettings.Save("RAMSettings.ini"); };

            ComboBox comboAutoCloseType = new ComboBox { Location = new Point(180, top), Size = new Size(150, 24), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
            comboAutoCloseType.Items.AddRange(new object[] { "Per Instance", "Global" });
            comboAutoCloseType.SelectedIndex = int.TryParse(AccountManager.AccountControl.Get("AutoCloseType"), out int acType) ? Math.Min(1, Math.Max(0, acType)) : 0;
            comboAutoCloseType.SelectedIndexChanged += (s, e) => { AccountManager.AccountControl.Set("AutoCloseType", comboAutoCloseType.SelectedIndex.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); };
            top += 30;

            Label lCloseInterval = new Label { Text = "Interval (min):", Location = new Point(15, top + 3), AutoSize = true, ForeColor = Color.FromArgb(148, 163, 184) };
            NumericUpDown numCloseInterval = new NumericUpDown { Location = new Point(245, top), Size = new Size(85, 24), Minimum = 1, Maximum = 9999, Value = decimal.TryParse(AccountManager.AccountControl.Get("AutoCloseInterval"), out decimal ci) ? ci : 1, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
            numCloseInterval.ValueChanged += (s, e) => { AccountManager.AccountControl.Set("AutoCloseInterval", numCloseInterval.Value.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); };
            top += 30;

            Label lMaxInstances = new Label { Text = "Max Instances:", Location = new Point(15, top + 3), AutoSize = true, ForeColor = Color.FromArgb(148, 163, 184) };
            NumericUpDown numMaxInstances = new NumericUpDown { Location = new Point(245, top), Size = new Size(85, 24), Minimum = 1, Maximum = 99, Value = decimal.TryParse(AccountManager.AccountControl.Get("MaxInstances"), out decimal maxInst) ? maxInst : 1, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
            numMaxInstances.ValueChanged += (s, e) => { AccountManager.AccountControl.Set("MaxInstances", numMaxInstances.Value.ToString()); AccountManager.IniSettings.Save("RAMSettings.ini"); };
            top += 40;

            rightControlPanel.Controls.AddRange(new Control[] { chkAutoMinimize, lMinInterval, numAutoMinInterval, chkAutoClose, comboAutoCloseType, lCloseInterval, numCloseInterval, lMaxInstances, numMaxInstances });

            // 4. 💻 CPU & HARDWARE LIMITS
            Label lblCpuHeader = new Label { Text = "💻 CPU & RESOURCE SETTINGS", ForeColor = Color.FromArgb(0, 242, 254), Location = new Point(15, top), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            top += 25;

            Label lblCpu = new Label { Text = "CPU Core:", ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(15, top + 3), AutoSize = true };
            comboCpuCore = new ComboBox { Location = new Point(85, top), Size = new Size(80, 24), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
            comboCpuCore.Items.AddRange(new object[] { "ALL", "CORE 1", "CORE 2", "CORE 4" });
            comboCpuCore.SelectedIndex = 0;

            Label lblRam = new Label { Text = "RAM:", ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(185, top + 3), AutoSize = true };
            comboRamLimit = new ComboBox { Location = new Point(230, top), Size = new Size(100, 24), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
            comboRamLimit.Items.AddRange(new object[] { "MAX", "RAM 1GB", "RAM 2GB", "RAM 4GB" });
            comboRamLimit.SelectedIndex = 0;

            rightControlPanel.Controls.AddRange(new Control[] { lblCpuHeader, lblCpu, comboCpuCore, lblRam, comboRamLimit });
        }

        private void ListViewAccounts_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedFooter();

            if (listViewAccounts.SelectedItems.Count == 1)
            {
                ListViewItem selectedItem = listViewAccounts.SelectedItems[0];
                if (selectedItem.SubItems.Count > 1)
                {
                    string username = selectedItem.SubItems[1].Text;
                    var acc = AccountManager.AccountsList?.FirstOrDefault(a => a.Username == username);
                    if (acc != null)
                    {
                        string pId = acc.GetField("SavedPlaceId");
                        if (string.IsNullOrEmpty(pId)) pId = AccountManager.CurrentPlaceId ?? "2753915549";
                        txtPlaceId.Text = pId;

                        string jId = acc.GetField("SavedJobId");
                        txtJobId.Text = string.IsNullOrEmpty(jId) ? "None" : jId;
                    }
                }
            }
        }

        private void UpdateSelectedFooter()
        {
            int selectedCount = listViewAccounts.SelectedItems.Count;
            int totalCount = listViewAccounts.Items.Count;
            lblSelectedCount.Text = $"เลือกอยู่ {selectedCount} รายการ / ทั้งหมด {totalCount} รายการ";
        }

        public void UpdateRealTimeStatus()
        {
            if (AccountManager.AccountsList == null || listViewAccounts == null) return;

            var relaunchAccounts = AccountManager.AccountsList.Where(a => a.GetField("AddToRelaunch") == "true").ToList();
            if (relaunchAccounts.Count == 0) relaunchAccounts = AccountManager.AccountsList;

            if (listViewAccounts.Items.Count != relaunchAccounts.Count)
            {
                RefreshAccountsList();
                return;
            }

            int idle = 0, online = 0, offline = 0;
            var allRbxProcs = System.Diagnostics.Process.GetProcessesByName("RobloxPlayerBeta");

            for (int i = 0; i < relaunchAccounts.Count; i++)
            {
                var acc = relaunchAccounts[i];
                var item = listViewAccounts.Items[i];

                System.Diagnostics.Process rbxProc = null;
                if (!string.IsNullOrEmpty(acc.BrowserTrackerID))
                {
                    rbxProc = allRbxProcs.FirstOrDefault(p =>
                    {
                        try { return !p.HasExited && p.GetCommandLine().Contains(acc.BrowserTrackerID); }
                        catch { return false; }
                    });
                }

                bool isRunning = rbxProc != null && !rbxProc.HasExited;

                string statusText = "IDLE";
                if (isRunning)
                {
                    statusText = "ONLINE";
                    online++;
                }
                else if (acc.GetField("Status")?.ToUpper() == "OFFLINE" || acc.GetField("IsOffline") == "true")
                {
                    statusText = "OFFLINE";
                    offline++;
                }
                else
                {
                    statusText = "IDLE";
                    idle++;
                }

                string pidText = isRunning ? rbxProc.Id.ToString() : "-";
                string uptimeText = "-";
                if (isRunning)
                {
                    try
                    {
                        TimeSpan duration = DateTime.Now - rbxProc.StartTime;
                        uptimeText = duration.TotalHours >= 1
                            ? $"{(int)duration.TotalHours}h {duration.Minutes:D2}m {duration.Seconds:D2}s"
                            : $"{duration.Minutes:D2}m {duration.Seconds:D2}s";
                    }
                    catch { }
                }

                string savedPlaceId = acc.GetField("SavedPlaceId") ?? txtPlaceId?.Text ?? "2753915549";
                string savedJobId = acc.GetField("SavedJobId") ?? txtJobId?.Text ?? "";
                if (string.IsNullOrEmpty(savedJobId)) savedJobId = "None";

                if (item.SubItems.Count >= 7)
                {
                    if (item.SubItems[1].Text != acc.Username) item.SubItems[1].Text = acc.Username;
                    if (item.SubItems[2].Text != savedPlaceId) item.SubItems[2].Text = savedPlaceId;
                    if (item.SubItems[3].Text != savedJobId) item.SubItems[3].Text = savedJobId;
                    if (item.SubItems[4].Text != statusText) item.SubItems[4].Text = statusText;
                    if (item.SubItems[5].Text != pidText) item.SubItems[5].Text = pidText;
                    if (item.SubItems[6].Text != uptimeText) item.SubItems[6].Text = uptimeText;
                }
            }

            if (panelIdleCard?.Controls.Count > 0 && panelIdleCard.Controls[0] is Label lIdle) lIdle.Text = $"IDLE {idle}";
            if (panelOnlineCard?.Controls.Count > 0 && panelOnlineCard.Controls[0] is Label lOnline) lOnline.Text = $"ONLINE {online}";
            if (panelOfflineCard?.Controls.Count > 0 && panelOfflineCard.Controls[0] is Label lOffline) lOffline.Text = $"OFFLINE {offline}";

            listViewAccounts.Invalidate();
        }

        public void RefreshAccountsList()
        {
            if (AccountManager.AccountsList == null) return;

            listViewAccounts.Items.Clear();
            int index = 1;
            int idle = 0, online = 0, offline = 0;
            var allRbxProcs = System.Diagnostics.Process.GetProcessesByName("RobloxPlayerBeta");

            var relaunchAccounts = AccountManager.AccountsList.Where(a => a.GetField("AddToRelaunch") == "true").ToList();
            if (relaunchAccounts.Count == 0) relaunchAccounts = AccountManager.AccountsList;

            foreach (var acc in relaunchAccounts)
            {
                System.Diagnostics.Process rbxProc = null;
                if (!string.IsNullOrEmpty(acc.BrowserTrackerID))
                {
                    rbxProc = allRbxProcs.FirstOrDefault(p =>
                    {
                        try { return !p.HasExited && p.GetCommandLine().Contains(acc.BrowserTrackerID); }
                        catch { return false; }
                    });
                }

                bool isRunning = rbxProc != null && !rbxProc.HasExited;

                string statusText = "IDLE";
                if (isRunning)
                {
                    statusText = "ONLINE";
                    online++;
                }
                else if (acc.GetField("Status")?.ToUpper() == "OFFLINE" || acc.GetField("IsOffline") == "true")
                {
                    statusText = "OFFLINE";
                    offline++;
                }
                else
                {
                    statusText = "IDLE";
                    idle++;
                }

                string pidText = isRunning ? rbxProc.Id.ToString() : "-";
                string uptimeText = "-";
                if (isRunning)
                {
                    try
                    {
                        TimeSpan duration = DateTime.Now - rbxProc.StartTime;
                        uptimeText = duration.TotalHours >= 1
                            ? $"{(int)duration.TotalHours}h {duration.Minutes:D2}m {duration.Seconds:D2}s"
                            : $"{duration.Minutes:D2}m {duration.Seconds:D2}s";
                    }
                    catch { }
                }

                string savedPlaceId = acc.GetField("SavedPlaceId") ?? txtPlaceId?.Text ?? "2753915549";
                string savedJobId = acc.GetField("SavedJobId") ?? txtJobId?.Text ?? "";
                if (string.IsNullOrEmpty(savedJobId)) savedJobId = "None";

                ListViewItem item = new ListViewItem(index.ToString());
                item.SubItems.Add(acc.Username);
                item.SubItems.Add(savedPlaceId);
                item.SubItems.Add(savedJobId);
                item.SubItems.Add(statusText);
                item.SubItems.Add(pidText);
                item.SubItems.Add(uptimeText);

                listViewAccounts.Items.Add(item);
                index++;
            }

            if (panelIdleCard?.Controls.Count > 0 && panelIdleCard.Controls[0] is Label lIdle) lIdle.Text = $"IDLE {idle}";
            if (panelOnlineCard?.Controls.Count > 0 && panelOnlineCard.Controls[0] is Label lOnline) lOnline.Text = $"ONLINE {online}";
            if (panelOfflineCard?.Controls.Count > 0 && panelOfflineCard.Controls[0] is Label lOffline) lOffline.Text = $"OFFLINE {offline}";

            UpdateSelectedFooter();
        }

        #region Actions & Window Management
        private void BtnMinimizeWindows_Click(object sender, EventArgs e)
        {
            foreach (var proc in System.Diagnostics.Process.GetProcessesByName("RobloxPlayerBeta"))
            {
                if (proc.MainWindowHandle != IntPtr.Zero)
                    ShowWindow(proc.MainWindowHandle, SW_MINIMIZE);
            }
        }

        private void BtnCloseRoblox_Click(object sender, EventArgs e)
        {
            foreach (var proc in System.Diagnostics.Process.GetProcessesByName("RobloxPlayerBeta"))
            {
                try { proc.Kill(); } catch { }
            }
            RefreshAccountsList();
        }

        private void BtnArrangeWindows_Click(object sender, EventArgs e)
        {
            var processes = System.Diagnostics.Process.GetProcessesByName("RobloxPlayerBeta")
                .Where(p => p.MainWindowHandle != IntPtr.Zero).ToList();

            if (processes.Count == 0) return;

            Rectangle screen = Screen.PrimaryScreen.WorkingArea;
            int count = processes.Count;
            int cols = (int)Math.Ceiling(Math.Sqrt(count));
            int rows = (int)Math.Ceiling((double)count / cols);

            int width = screen.Width / cols;
            int height = screen.Height / rows;

            for (int i = 0; i < count; i++)
            {
                int r = i / cols;
                int c = i % cols;
                MoveWindow(processes[i].MainWindowHandle, screen.X + (c * width), screen.Y + (r * height), width, height, true);
            }
        }
        #endregion

        #region Custom Drawing with Row Status Highlighting & Selection Accent Border
        private void ListViewAccounts_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush backBrush = new SolidBrush(Color.FromArgb(25, 28, 42)))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(148, 163, 184)))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
                e.Graphics.DrawString(e.Header.Text, new Font("Segoe UI", 8.5F, FontStyle.Bold), textBrush, e.Bounds.X + 8, e.Bounds.Y + 6);
            }
        }

        private void ListViewAccounts_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            bool isSelected = e.Item.Selected;

            int rowIdx = e.Item.Index;
            Color bgColor = (rowIdx % 2 == 0) ? Color.FromArgb(18, 22, 34) : Color.FromArgb(24, 28, 42);

            if (isSelected)
            {
                bgColor = Color.FromArgb(32, 48, 78);
            }

            // Fill row background frame
            Rectangle rowBounds = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            using (SolidBrush rowBrush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(rowBrush, rowBounds);
            }

            if (isSelected)
            {
                using (Pen borderPen = new Pen(Color.FromArgb(0, 242, 254), 1.5f))
                {
                    e.Graphics.DrawRectangle(borderPen, rowBounds.X + 1, rowBounds.Y + 1, rowBounds.Width - 2, rowBounds.Height - 2);
                }
            }

            // Bottom gridline
            using (Pen gridPen = new Pen(Color.FromArgb(15, 255, 255, 255), 1f))
            {
                e.Graphics.DrawLine(gridPen, e.Bounds.X, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }
        }

        private void ListViewAccounts_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            string statusStr = e.Item.SubItems.Count > 4 ? e.Item.SubItems[4].Text : "IDLE";
            Color textColor = Color.White;

            if (e.ColumnIndex == 4) // Status Column
            {
                Color badgeColor;
                string labelText;

                if (statusStr == "ONLINE")
                {
                    badgeColor = Color.FromArgb(34, 197, 94); // Green
                    labelText = "ONLINE";
                }
                else if (statusStr == "OFFLINE")
                {
                    badgeColor = Color.FromArgb(239, 68, 68); // Red
                    labelText = "OFFLINE";
                }
                else // IDLE
                {
                    badgeColor = Color.FromArgb(234, 179, 8); // Yellow
                    labelText = "IDLE";
                }

                int dotX = e.Bounds.X + 8;
                int dotY = e.Bounds.Y + (e.Bounds.Height - 8) / 2;
                using (SolidBrush dotBrush = new SolidBrush(badgeColor))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillEllipse(dotBrush, dotX, dotY, 8, 8);
                }

                using (SolidBrush textBrush = new SolidBrush(badgeColor))
                using (Font font = new Font("Segoe UI", 9F, FontStyle.Bold))
                {
                    e.Graphics.DrawString(labelText, font, textBrush, dotX + 14, e.Bounds.Y + 4);
                }
            }
            else
            {
                if (e.ColumnIndex == 0) textColor = Color.FromArgb(148, 163, 184);
                using (SolidBrush textBrush = new SolidBrush(textColor))
                {
                    e.Graphics.DrawString(e.SubItem.Text, e.Item.Font, textBrush, e.Bounds.X + 6, e.Bounds.Y + 4);
                }
            }
        }
        #endregion
    }
}

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

        public AutoRejoinControl()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(15, 17, 26);
            this.Dock = DockStyle.Fill;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            InitializeComponents();
            RefreshAccountsList();
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
                Width = 350,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(20, 24, 36)
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

            // Auto Relaunch Header & Switch
            chkAutoRelaunch = new CheckBox
            {
                Text = "⚡ Auto Relaunch",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, top),
                AutoSize = true,
                Checked = AccountManager.Watcher.Get<bool>("AutoRejoinEnabled")
            };
            chkAutoRelaunch.CheckedChanged += (s, e) =>
            {
                AccountManager.Watcher.Set("AutoRejoinEnabled", chkAutoRelaunch.Checked.ToString().ToLower());
            };
            rightControlPanel.Controls.Add(chkAutoRelaunch);
            top += 45;

            // placeId Field
            Label lblPlace = new Label { Text = "placeId :", ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(20, top + 3), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            txtPlaceId = new TextBox { Location = new Point(90, top), Size = new Size(220, 25), BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Text = AccountManager.CurrentPlaceId };
            rightControlPanel.Controls.AddRange(new Control[] { lblPlace, txtPlaceId });
            top += 40;

            // JobId Field
            Label lblJob = new Label { Text = "JobId :", ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(20, top + 3), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            txtJobId = new TextBox { Location = new Point(90, top), Size = new Size(220, 25), BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Text = string.IsNullOrEmpty(AccountManager.CurrentJobId) ? "None" : AccountManager.CurrentJobId };
            rightControlPanel.Controls.AddRange(new Control[] { lblJob, txtJobId });
            top += 40;

            // Save PlaceId & JobId Button (Saves according to Selection if selected, otherwise all relaunch accounts)
            btnSavePlaceJob = new Button
            {
                Text = "💾 บันทึกตั้งค่า PlaceID & JobID",
                Location = new Point(20, top),
                Size = new Size(290, 34),
                BackColor = Color.FromArgb(0, 230, 118),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSavePlaceJob.FlatAppearance.BorderSize = 0;
            btnSavePlaceJob.MakeRounded(10);
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
            top += 45;

            // Action Buttons (Arrange, Minimize, Close - Rounded)
            btnArrangeWindows = new Button { Text = "🖼️ Arrange Roblox Windows", Location = new Point(20, top), Size = new Size(290, 32), BackColor = Color.FromArgb(45, 52, 75), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnArrangeWindows.FlatAppearance.BorderSize = 0;
            btnArrangeWindows.MakeRounded(10);
            btnArrangeWindows.Click += BtnArrangeWindows_Click;
            top += 40;

            btnMinimizeWindows = new Button { Text = "🔽 Minimize Windows", Location = new Point(20, top), Size = new Size(140, 32), BackColor = Color.FromArgb(45, 52, 75), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnMinimizeWindows.FlatAppearance.BorderSize = 0;
            btnMinimizeWindows.MakeRounded(10);
            btnMinimizeWindows.Click += BtnMinimizeWindows_Click;

            btnCloseRoblox = new Button { Text = "❌ Close Roblox", Location = new Point(170, top), Size = new Size(140, 32), BackColor = Color.FromArgb(180, 40, 40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCloseRoblox.FlatAppearance.BorderSize = 0;
            btnCloseRoblox.MakeRounded(10);
            btnCloseRoblox.Click += BtnCloseRoblox_Click;

            rightControlPanel.Controls.AddRange(new Control[] { btnArrangeWindows, btnMinimizeWindows, btnCloseRoblox });
            top += 50;

            // Roblox CPU & Hardware Settings Header
            Label lblCpuHeader = new Label { Text = "ROBLOX CPU & RESOURCE SETTINGS", ForeColor = Color.FromArgb(0, 242, 254), Location = new Point(20, top), AutoSize = true, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
            top += 25;

            Label lblCpu = new Label { Text = "CPU Core:", ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(20, top + 3), AutoSize = true };
            comboCpuCore = new ComboBox { Location = new Point(90, top), Size = new Size(70, 25), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
            comboCpuCore.Items.AddRange(new object[] { "ALL", "CORE 1", "CORE 2", "CORE 4" });
            comboCpuCore.SelectedIndex = 0;

            Label lblRam = new Label { Text = "RAM:", ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(175, top + 3), AutoSize = true };
            comboRamLimit = new ComboBox { Location = new Point(220, top), Size = new Size(90, 25), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White };
            comboRamLimit.Items.AddRange(new object[] { "MAX", "RAM 1GB", "RAM 2GB", "RAM 4GB" });
            comboRamLimit.SelectedIndex = 0;

            rightControlPanel.Controls.AddRange(new Control[] { lblCpuHeader, lblCpu, comboCpuCore, lblRam, comboRamLimit });
            top += 45;

            // Timer & Delays Inputs
            Label lblDelaysHeader = new Label { Text = "TIMERS & DELAY (SECONDS)", ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(20, top), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold) };
            top += 22;

            int colW = 85;
            Label l1 = new Label { Text = "DELAY", ForeColor = Color.Gray, Location = new Point(20, top), Size = new Size(colW, 15), Font = new Font("Segoe UI", 7.5F) };
            Label l2 = new Label { Text = "OPEN RBLX", ForeColor = Color.Gray, Location = new Point(115, top), Size = new Size(colW, 15), Font = new Font("Segoe UI", 7.5F) };
            Label l3 = new Label { Text = "OFFLINETIME", ForeColor = Color.Gray, Location = new Point(210, top), Size = new Size(colW, 15), Font = new Font("Segoe UI", 7.5F) };
            top += 18;

            txtDelay = new TextBox { Text = "5", Location = new Point(20, top), Size = new Size(colW, 25), BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White, TextAlign = HorizontalAlignment.Center };
            txtDelayOpenRblx = new TextBox { Text = "10", Location = new Point(115, top), Size = new Size(colW, 25), BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White, TextAlign = HorizontalAlignment.Center };
            txtOfflineTime = new TextBox { Text = "60", Location = new Point(210, top), Size = new Size(colW, 25), BackColor = Color.FromArgb(30, 35, 50), ForeColor = Color.White, TextAlign = HorizontalAlignment.Center };

            rightControlPanel.Controls.AddRange(new Control[] { lblDelaysHeader, l1, l2, l3, txtDelay, txtDelayOpenRblx, txtOfflineTime });
        }

        private void ListViewAccounts_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedFooter();
        }

        private void UpdateSelectedFooter()
        {
            int selectedCount = listViewAccounts.SelectedItems.Count;
            int totalCount = listViewAccounts.Items.Count;
            lblSelectedCount.Text = $"เลือกอยู่ {selectedCount} รายการ / ทั้งหมด {totalCount} รายการ";
        }

        public void RefreshAccountsList()
        {
            if (AccountManager.AccountsList == null) return;

            listViewAccounts.Items.Clear();
            int index = 1;
            int idle = 0, online = 0, offline = 0;

            // Only accounts marked for Rejoin ("AddToRelaunch" == "true") or all accounts if none marked
            var relaunchAccounts = AccountManager.AccountsList.Where(a => a.GetField("AddToRelaunch") == "true").ToList();
            if (relaunchAccounts.Count == 0) relaunchAccounts = AccountManager.AccountsList;

            foreach (var acc in relaunchAccounts)
            {
                var rbxProc = RobloxWatcher.Instances.FirstOrDefault(rp =>
                {
                    try { return rp.RbxProcess != null && !rp.RbxProcess.HasExited && rp.RbxProcess.GetCommandLine().Contains(acc.BrowserTrackerID); } catch { return false; }
                });

                bool isRunning = rbxProc != null && !rbxProc.RbxProcess.HasExited;

                // Status definition: ONLINE (Running), OFFLINE (Explicit offline/error if offline field set), IDLE (Paused/Waiting)
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

                string pidText = isRunning ? rbxProc.RbxProcess.Id.ToString() : "-";
                string uptimeText = "-";
                if (isRunning)
                {
                    TimeSpan duration = DateTime.Now - rbxProc.RbxProcess.StartTime;
                    uptimeText = duration.TotalHours >= 1 ? $"{(int)duration.TotalHours}h {duration.Minutes}m" : $"{duration.Minutes}m {duration.Seconds}s";
                }

                string savedPlaceId = acc.GetField("SavedPlaceId") ?? txtPlaceId?.Text ?? "2753915549";
                string savedJobId = acc.GetField("SavedJobId") ?? txtJobId?.Text ?? "None";

                ListViewItem item = new ListViewItem(index.ToString());
                item.SubItems.Add(acc.Username);
                item.SubItems.Add(savedPlaceId);
                item.SubItems.Add(string.IsNullOrEmpty(savedJobId) ? "None" : savedJobId);
                item.SubItems.Add(statusText);
                item.SubItems.Add(pidText);
                item.SubItems.Add(uptimeText);

                listViewAccounts.Items.Add(item);
                index++;
            }

            if (panelIdleCard?.Controls[0] is Label lIdle) lIdle.Text = $"IDLE {idle}";
            if (panelOnlineCard?.Controls[0] is Label lOnline) lOnline.Text = $"ONLINE {online}";
            if (panelOfflineCard?.Controls[0] is Label lOffline) lOffline.Text = $"OFFLINE {offline}";

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
            string statusStr = e.Item.SubItems.Count > 4 ? e.Item.SubItems[4].Text : "IDLE";
            bool isSelected = e.Item.Selected;

            Color statusColor;
            Color bgColor;
            Color borderColor;

            if (statusStr == "ONLINE")
            {
                statusColor = Color.FromArgb(34, 197, 94); // Green
                bgColor = Color.FromArgb(20, 36, 28);
                borderColor = Color.FromArgb(34, 197, 94);
            }
            else if (statusStr == "OFFLINE")
            {
                statusColor = Color.FromArgb(239, 68, 68); // Red
                bgColor = Color.FromArgb(38, 22, 26);
                borderColor = Color.FromArgb(239, 68, 68);
            }
            else // IDLE
            {
                statusColor = Color.FromArgb(234, 179, 8); // Yellow
                bgColor = Color.FromArgb(35, 32, 22);
                borderColor = Color.FromArgb(234, 179, 8);
            }

            if (isSelected)
            {
                bgColor = Color.FromArgb(
                    Math.Min(255, bgColor.R + 25),
                    Math.Min(255, bgColor.G + 25),
                    Math.Min(255, bgColor.B + 35)
                );
            }

            // Fill row background frame
            Rectangle rowBounds = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 1, e.Bounds.Width - 4, e.Bounds.Height - 2);
            using (SolidBrush rowBrush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(rowBrush, rowBounds);
            }

            // Draw status accent frame / border (thick border when Selected)
            int borderWidth = isSelected ? 2 : 1;
            using (Pen borderPen = new Pen(isSelected ? Color.FromArgb(0, 242, 254) : Color.FromArgb(40, borderColor), borderWidth))
            {
                e.Graphics.DrawRectangle(borderPen, rowBounds);
            }
        }

        private void ListViewAccounts_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            string statusStr = e.Item.SubItems.Count > 4 ? e.Item.SubItems[4].Text : "IDLE";
            Color textColor = Color.White;

            if (e.ColumnIndex == 4) // Status Column
            {
                Color badgeColor;
                string iconText;

                if (statusStr == "ONLINE")
                {
                    badgeColor = Color.FromArgb(34, 197, 94); // Green
                    iconText = "🟢 ONLINE";
                }
                else if (statusStr == "OFFLINE")
                {
                    badgeColor = Color.FromArgb(239, 68, 68); // Red
                    iconText = "🔴 OFFLINE";
                }
                else // IDLE
                {
                    badgeColor = Color.FromArgb(234, 179, 8); // Yellow
                    iconText = "🟡 IDLE";
                }

                using (SolidBrush textBrush = new SolidBrush(badgeColor))
                {
                    e.Graphics.DrawString(iconText, new Font("Segoe UI", 9F, FontStyle.Bold), textBrush, e.Bounds.X + 6, e.Bounds.Y + 4);
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

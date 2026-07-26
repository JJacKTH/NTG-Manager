using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace RBX_Alt_Manager.Classes
{
    public class NTGAccountGrid : UserControl
    {
        private readonly Panel headerPanel;
        private readonly FlowLayoutPanel rowsPanel;
        private readonly List<NTGAccountRow> rowControls = new List<NTGAccountRow>();

        public Account SelectedAccount { get; private set; }
        public event EventHandler AccountDoubleClicked;

        public void InvalidateRows()
        {
            foreach (var row in rowControls)
            {
                row.Invalidate();
            }
        }

        public System.Collections.IList SelectedObjects
        {
            get
            {
                var list = new System.Collections.ArrayList();
                if (SelectedAccount != null) list.Add(SelectedAccount);
                return list;
            }
        }
        public ContextMenuStrip RowContextMenuStrip { get; set; }
        public event EventHandler SelectionChanged;

        public NTGAccountGrid()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(10, 12, 20);
            this.Font = new Font("Segoe UI", 9f);

            // Header Panel
            headerPanel = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(14, 18, 30),
                Padding = new Padding(0)
            };
            headerPanel.Paint += HeaderPanel_Paint;

            // Scrollable Rows Container Panel
            rowsPanel = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.FromArgb(10, 12, 20),
                Padding = new Padding(0)
            };
            rowsPanel.SizeChanged += (s, e) => UpdateRowWidths();

            this.Controls.Add(rowsPanel);
            this.Controls.Add(headerPanel);
        }

        private void HeaderPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush bg = new SolidBrush(Color.FromArgb(14, 18, 30)))
            {
                g.FillRectangle(bg, headerPanel.ClientRectangle);
            }

            int w = headerPanel.Width;
            int col0W = 50;
            int col1W = 220;
            int col2W = 140;

            using (Font font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (Brush brush = new SolidBrush(Color.FromArgb(123, 132, 163)))
            using (Pen borderPen = new Pen(Color.FromArgb(25, 255, 255, 255)))
            {
                // Column 0 (#)
                g.DrawString("#", font, brush, 16, 10);
                g.DrawLine(borderPen, col0W, 0, col0W, 36);

                // Column 1 (Avatar & Username)
                g.DrawString(LanguageManager.GetText("ColAvatarUsername"), font, brush, col0W + 12, 10);
                g.DrawLine(borderPen, col0W + col1W, 0, col0W + col1W, 36);

                // Column 2 (Status)
                g.DrawString(LanguageManager.GetText("ColStatus"), font, brush, col0W + col1W + col2W, 10);
                g.DrawLine(borderPen, col0W + col1W + col2W, 0, col0W + col1W + col2W, 36);

                // Column 3 (Description)
                g.DrawString(LanguageManager.GetText("ColDescription"), font, brush, col0W + col1W + col2W + 12, 10);

                // Bottom line
                g.DrawLine(borderPen, 0, 35, w, 35);
            }
        }

        public void SetAccounts(List<Account> accounts)
        {
            rowsPanel.SuspendLayout();
            rowsPanel.Controls.Clear();
            rowControls.Clear();

            if (accounts != null)
            {
                int index = 1;
                foreach (var acc in accounts)
                {
                    var row = new NTGAccountRow(index++, acc);
                    row.Width = Math.Max(rowsPanel.ClientSize.Width - 4, 600);
                    row.ContextMenuStrip = this.RowContextMenuStrip;
                    row.RowSelected += Row_RowSelected;
                    row.RowDoubleClicked += (s, acc) => AccountDoubleClicked?.Invoke(this, EventArgs.Empty);
                    rowControls.Add(row);
                    rowsPanel.Controls.Add(row);
                }
            }

            rowsPanel.ResumeLayout();
        }

        private void UpdateRowWidths()
        {
            int targetW = Math.Max(rowsPanel.ClientSize.Width - 4, 600);
            foreach (Control ctrl in rowsPanel.Controls)
            {
                ctrl.Width = targetW;
            }
            headerPanel.Invalidate();
        }

        private void Row_RowSelected(object sender, Account acc)
        {
            SelectedAccount = acc;
            foreach (var r in rowControls)
            {
                r.SetSelected(r.AccountItem == acc);
            }
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SelectAccount(Account acc)
        {
            Row_RowSelected(this, acc);
        }
    }

    public class NTGAccountRow : UserControl
    {
        public int IndexNum { get; }
        public Account AccountItem { get; }
        public bool IsSelected { get; private set; }

        public event EventHandler<Account> RowSelected;

        public event EventHandler<Account> RowDoubleClicked;

        public NTGAccountRow(int index, Account account)
        {
            this.DoubleBuffered = true;
            this.IndexNum = index;
            this.AccountItem = account;
            this.Height = 52;
            this.BackColor = Color.FromArgb(10, 12, 20);
            this.Cursor = Cursors.Hand;

            this.MouseClick += Row_MouseClick;
            this.MouseDoubleClick += (s, e) => RowDoubleClicked?.Invoke(this, AccountItem);
        }

        private void Row_MouseClick(object sender, MouseEventArgs e)
        {
            RowSelected?.Invoke(this, AccountItem);

            if (e.Button == MouseButtons.Right && this.ContextMenuStrip != null)
            {
                this.ContextMenuStrip.Show(this, e.Location);
            }
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle r = this.ClientRectangle;

            // Background
            using (SolidBrush bg = new SolidBrush(IsSelected ? Color.FromArgb(40, 123, 44, 191) : Color.FromArgb(10, 12, 20)))
            {
                g.FillRectangle(bg, r);
            }

            // Left Cyan Bar on Select
            if (IsSelected)
            {
                using (SolidBrush barBrush = new SolidBrush(Color.FromArgb(0, 242, 254)))
                {
                    g.FillRectangle(barBrush, new Rectangle(0, 0, 3, r.Height));
                }
            }

            // Bottom Separator Line
            using (Pen linePen = new Pen(Color.FromArgb(15, 255, 255, 255)))
            {
                g.DrawLine(linePen, 0, r.Height - 1, r.Width, r.Height - 1);
            }

            int col0W = 50;
            int col1W = 220;
            int col2W = 140;

            // Column 0: Index (#)
            string indexStr = IndexNum.ToString();
            using (Font font = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (Brush brush = new SolidBrush(Color.FromArgb(123, 132, 163)))
            {
                SizeF sz = g.MeasureString(indexStr, font);
                g.DrawString(indexStr, font, brush, (col0W - sz.Width) / 2, (r.Height - sz.Height) / 2);
            }

            // Column 1: Avatar & Username
            int imgX = col0W + 8;
            int imgY = (r.Height - 38) / 2;

            Image avatarImg = null;
            if (AccountItem != null && AccountItem.UserID > 0)
            {
                avatarImg = NTGAccountRenderer.GetAvatar(AccountItem.UserID);
            }

            if (avatarImg != null)
            {
                g.DrawImage(avatarImg, imgX, imgY, 38, 38);
            }
            else
            {
                DrawPlaceholder(g, imgX, imgY, 38, 38, AccountItem?.Username);
            }

            string username = AccountItem?.Username ?? "Unknown";
            string alias = string.IsNullOrEmpty(AccountItem?.Alias) ? $"@{username}" : $"@{AccountItem.Alias}";
            int textX = imgX + 46;

            using (Font nameFont = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (Font subFont = new Font("Segoe UI", 8.0f, FontStyle.Regular))
            using (Brush nameBrush = new SolidBrush(Color.FromArgb(240, 243, 254)))
            using (Brush subBrush = new SolidBrush(Color.FromArgb(123, 132, 163)))
            {
                g.DrawString(username, nameFont, nameBrush, textX, 8);
                g.DrawString(alias, subFont, subBrush, textX, 26);
            }

            // Column 2: Status Glow Pill
            int col2X = col0W + col1W + 8;
            string statusText = LanguageManager.GetText("StatusOffline");
            Color statusColor = Color.FromArgb(123, 132, 163);
            Color statusBg = Color.FromArgb(20, 255, 255, 255);
            Color statusBorder = Color.FromArgb(40, 255, 255, 255);

            if (AccountItem != null && !AccountItem.Valid)
            {
                statusText = LanguageManager.GetText("StatusCookieDead");
                statusColor = Color.FromArgb(255, 0, 85);
                statusBg = Color.FromArgb(30, 255, 0, 85);
                statusBorder = Color.FromArgb(80, 255, 0, 85);
            }
            else if (AccountItem != null && AccountItem.Presence != null)
            {
                switch (AccountItem.Presence.userPresenceType)
                {
                    case UserPresenceType.InGame:
                        statusText = LanguageManager.GetText("StatusFarming");
                        statusColor = Color.FromArgb(0, 242, 254);
                        statusBg = Color.FromArgb(30, 0, 242, 254);
                        statusBorder = Color.FromArgb(80, 0, 242, 254);
                        break;
                    case UserPresenceType.InStudio:
                        statusText = LanguageManager.GetText("StatusInStudio");
                        statusColor = Color.FromArgb(157, 78, 221);
                        statusBg = Color.FromArgb(30, 157, 78, 221);
                        statusBorder = Color.FromArgb(80, 157, 78, 221);
                        break;
                    case UserPresenceType.Online:
                        statusText = LanguageManager.GetText("StatusOnline");
                        statusColor = Color.FromArgb(0, 230, 118);
                        statusBg = Color.FromArgb(30, 0, 230, 118);
                        statusBorder = Color.FromArgb(80, 0, 230, 118);
                        break;
                }
            }

            int pillW = 105;
            int pillH = 26;
            int pillY = (r.Height - pillH) / 2;
            Rectangle pillRect = new Rectangle(col2X, pillY, pillW, pillH);

            using (GraphicsPath path = GetRoundedPath(pillRect, 13))
            using (SolidBrush bgB = new SolidBrush(statusBg))
            using (Pen borderP = new Pen(statusBorder, 1f))
            using (SolidBrush textB = new SolidBrush(statusColor))
            using (Font font = new Font("Segoe UI", 8.25f, FontStyle.Bold))
            {
                g.FillPath(bgB, path);
                g.DrawPath(borderP, path);

                int dotSize = 6;
                int dotX = col2X + 10;
                int dotY = pillY + (pillH - dotSize) / 2;
                using (SolidBrush dotBrush = new SolidBrush(statusColor))
                {
                    g.FillEllipse(dotBrush, dotX, dotY, dotSize, dotSize);
                }

                SizeF sz = g.MeasureString(statusText, font);
                g.DrawString(statusText, font, textB, dotX + dotSize + 6, pillY + (pillH - sz.Height) / 2);
            }

            // Column 3: Description / Resources
            int col3X = col0W + col1W + col2W + 8;
            string desc = AccountItem?.Description ?? string.Empty;

            if (string.IsNullOrEmpty(desc))
            {
                using (Font font = new Font("Segoe UI", 8.5f, FontStyle.Italic))
                using (Brush brush = new SolidBrush(Color.FromArgb(90, 100, 130)))
                {
                    g.DrawString("- No Description -", font, brush, col3X, (r.Height - 16) / 2);
                }
            }
            else
            {
                using (Font font = new Font("Segoe UI", 8.75f, FontStyle.Regular))
                using (Brush textBrush = new SolidBrush(Color.FromArgb(200, 210, 235)))
                {
                    g.DrawString(desc, font, textBrush, new RectangleF(col3X, 8, r.Width - col3X - 8, r.Height - 14));
                }
            }
        }

        private static void DrawPlaceholder(Graphics g, int x, int y, int w, int h, string username)
        {
            Rectangle rect = new Rectangle(x, y, w, h);
            using (GraphicsPath path = GetRoundedPath(rect, 10))
            using (LinearGradientBrush bg = new LinearGradientBrush(rect, Color.FromArgb(30, 40, 65), Color.FromArgb(18, 22, 36), 45f))
            using (Pen borderPen = new Pen(Color.FromArgb(50, 255, 255, 255), 1.5f))
            {
                g.FillPath(bg, path);
                g.DrawPath(borderPen, path);

                string initial = !string.IsNullOrEmpty(username) ? username.Substring(0, 1).ToUpper() : "?";
                using (Font font = new Font("Segoe UI", 13f, FontStyle.Bold))
                using (Brush brush = new SolidBrush(Color.FromArgb(0, 242, 254)))
                {
                    SizeF sz = g.MeasureString(initial, font);
                    g.DrawString(initial, font, brush, x + (w - sz.Width) / 2, y + (h - sz.Height) / 2);
                }
            }
        }

        public static GraphicsPath GetRoundedPath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

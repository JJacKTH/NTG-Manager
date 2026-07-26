using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Newtonsoft.Json.Linq;

namespace RBX_Alt_Manager.Classes
{
    public class NTGAccountRenderer : BaseRenderer
    {
        private static readonly ConcurrentDictionary<long, Image> AvatarCache = new ConcurrentDictionary<long, Image>();
        private static readonly ConcurrentDictionary<long, byte> PendingAvatars = new ConcurrentDictionary<long, byte>();
        private static readonly HttpClient HttpClient = new HttpClient();

        public static Image GetAvatar(long userId)
        {
            if (userId <= 0) return null;
            if (AvatarCache.TryGetValue(userId, out Image img)) return img;
            PreloadAvatar(userId);
            return null;
        }

        public static void PreloadAvatar(long userId)
        {
            if (userId <= 0 || AvatarCache.ContainsKey(userId) || !PendingAvatars.TryAdd(userId, 0)) return;

            Task.Run(async () =>
            {
                try
                {
                    string url = $"https://thumbnails.roblox.com/v1/users/avatar-headshot?userIds={userId}&size=150x150&format=Png&isCircular=false";
                    string json = await HttpClient.GetStringAsync(url);

                    // ponytail: robust JObject parsing for thumbnail URL
                    JObject obj = JObject.Parse(json);
                    string imgUrl = obj["data"]?[0]?["imageUrl"]?.ToString();
                    if (!string.IsNullOrEmpty(imgUrl))
                    {
                        byte[] imgBytes = await HttpClient.GetByteArrayAsync(imgUrl);
                        using MemoryStream ms = new MemoryStream(imgBytes);
                        Image rounded = MakeRoundedImage(Image.FromStream(ms), 38, 38, 10);
                        AvatarCache[userId] = rounded;
                        AccountManager.Instance?.InvokeIfRequired(() =>
                        {
                            AccountManager.Instance.AccountsView?.Invalidate();
                        });
                    }
                }
                catch { }
            });
        }

        private static Image MakeRoundedImage(Image img, int width, int height, int cornerRadius)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                using (GraphicsPath path = GetRoundedRectanglePath(new Rectangle(0, 0, width, height), cornerRadius))
                {
                    g.SetClip(path);
                    g.DrawImage(img, 0, 0, width, height);
                }

                using (Pen borderPen = new Pen(Color.FromArgb(50, 255, 255, 255), 1.5f))
                {
                    g.ResetClip();
                    using (GraphicsPath path = GetRoundedRectanglePath(new Rectangle(0, 0, width - 1, height - 1), cornerRadius))
                    {
                        g.DrawPath(borderPen, path);
                    }
                }
            }
            return bmp;
        }

        private static GraphicsPath GetRoundedRectanglePath(Rectangle rect, int cornerRadius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = cornerRadius * 2;
            Rectangle arc = new Rectangle(rect.X, rect.Y, diameter, diameter);

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.X;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private int currentColumnIndex = 0;

        public override bool RenderSubItem(DrawListViewSubItemEventArgs e, Graphics g, Rectangle r, object rowObject)
        {
            this.RowObject = rowObject;
            this.Event = e;
            if (e != null)
            {
                this.ListItem = e.Item as OLVListItem;
                this.SubItem = e.SubItem as OLVListSubItem;
                currentColumnIndex = e.ColumnIndex;
                if (this.ListView is ObjectListView olv && e.ColumnIndex >= 0 && e.ColumnIndex < olv.Columns.Count)
                {
                    this.Column = olv.GetColumn(e.ColumnIndex);
                }
            }
            Render(g, r);
            return true;
        }

        public override void Render(Graphics g, Rectangle r)
        {
            Account account = RowObject as Account;
            if (account == null)
            {
                base.Render(g, r);
                return;
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Determine exact item selection state
            bool isRowSelected = (Event != null && Event.Item != null) ? Event.Item.Selected : IsItemSelected;

            // Draw solid opaque background for selected & normal states to prevent default WinForms text overlay
            Color bgClr = isRowSelected ? Color.FromArgb(22, 32, 54) : Color.FromArgb(10, 12, 20);
            using (SolidBrush cellBg = new SolidBrush(bgClr))
            {
                g.FillRectangle(cellBg, r);
            }

            if (isRowSelected && (Column?.Index == 0 || currentColumnIndex == 0))
            {
                using (SolidBrush barBrush = new SolidBrush(Color.FromArgb(0, 242, 254)))
                {
                    g.FillRectangle(barBrush, new Rectangle(r.X, r.Y + 2, 3, r.Height - 4));
                }
            }

            using (Pen linePen = new Pen(Color.FromArgb(15, 255, 255, 255)))
            {
                g.DrawLine(linePen, r.X, r.Bottom - 1, r.Right, r.Bottom - 1);
            }

            // Identify column directly by Index or currentColumnIndex fallback
            int colIdx = Column != null ? Column.Index : currentColumnIndex;

            bool isCol0 = (colIdx == 0);
            bool isCol1 = (colIdx == 1);
            bool isCol2 = (colIdx == 2);
            bool isCol3 = (colIdx == 3);

            // Column 0: Row Index (#)
            if (isCol0)
            {
                int index = (ListItem?.Index ?? 0) + 1;
                string indexStr = index.ToString();
                using (Font font = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                using (Brush brush = new SolidBrush(Color.FromArgb(123, 132, 163)))
                {
                    SizeF sz = g.MeasureString(indexStr, font);
                    g.DrawString(indexStr, font, brush, r.X + (r.Width - sz.Width) / 2, r.Y + (r.Height - sz.Height) / 2);
                }
            }
            // Column 1: Avatar & Username
            else if (isCol1)
            {
                int imgX = r.X + 8;
                int imgY = r.Y + (r.Height - 38) / 2;

                if (account.UserID > 0)
                {
                    if (AvatarCache.TryGetValue(account.UserID, out Image avatarImg))
                    {
                        g.DrawImage(avatarImg, imgX, imgY, 38, 38);
                    }
                    else
                    {
                        PreloadAvatar(account.UserID);
                        DrawPlaceholderAvatar(g, imgX, imgY, 38, 38, account.Username);
                    }
                }
                else
                {
                    DrawPlaceholderAvatar(g, imgX, imgY, 38, 38, account.Username);
                }

                // Draw Username & DisplayName / Alias
                int textX = imgX + 46;
                string username = account.Username ?? "Unknown";
                string alias = string.IsNullOrEmpty(account.Alias) ? $"@{username}" : $"@{account.Alias}";

                using (Font nameFont = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                using (Font subFont = new Font("Segoe UI", 8.0f, FontStyle.Regular))
                using (Brush nameBrush = new SolidBrush(Color.FromArgb(240, 243, 254)))
                using (Brush subBrush = new SolidBrush(Color.FromArgb(123, 132, 163)))
                {
                    g.DrawString(username, nameFont, nameBrush, textX, r.Y + 8);
                    g.DrawString(alias, subFont, subBrush, textX, r.Y + 26);
                }
            }
            // Column 2: Status Glow Pill (or PID tag depending on column setup)
            else if (isCol2)
            {
                // Check if Column 2 is PID or Status
                string headerName = Column?.Text ?? "";
                if (headerName.Contains("PID"))
                {
                    // Render PID tag
                    int procId = 0;
                    try
                    {
                        var proc = System.Diagnostics.Process.GetProcessesByName("RobloxPlayerBeta")
                            .FirstOrDefault(p => !p.HasExited);
                        if (proc != null) procId = proc.Id;
                    }
                    catch { }

                    string pidStr = procId > 0 ? $"PID: {procId}" : "PID: -";
                    Color pidClr = procId > 0 ? Color.FromArgb(0, 242, 254) : Color.FromArgb(123, 132, 163);
                    Color pidBg = procId > 0 ? Color.FromArgb(30, 0, 242, 254) : Color.FromArgb(20, 255, 255, 255);
                    Color pidBorder = procId > 0 ? Color.FromArgb(80, 0, 242, 254) : Color.FromArgb(40, 255, 255, 255);

                    int tagW = 90;
                    int tagH = 24;
                    Rectangle tagRect = new Rectangle(r.X + 8, r.Y + (r.Height - tagH) / 2, tagW, tagH);
                    using (GraphicsPath path = GetRoundedRectanglePath(tagRect, 6))
                    using (SolidBrush bgB = new SolidBrush(pidBg))
                    using (Pen borderP = new Pen(pidBorder, 1f))
                    using (SolidBrush textB = new SolidBrush(pidClr))
                    using (Font font = new Font("Segoe UI", 8.25f, FontStyle.Bold))
                    {
                        g.FillPath(bgB, path);
                        g.DrawPath(borderP, path);
                        SizeF sz = g.MeasureString(pidStr, font);
                        g.DrawString(pidStr, font, textB, tagRect.X + (tagW - sz.Width) / 2, tagRect.Y + (tagH - sz.Height) / 2);
                    }
                }
                else
                {
                    string statusText = LanguageManager.GetText("StatusOffline");
                    Color statusColor = Color.FromArgb(123, 132, 163);
                    Color statusBg = Color.FromArgb(20, 255, 255, 255);
                    Color statusBorder = Color.FromArgb(40, 255, 255, 255);

                    if (!account.Valid)
                    {
                        statusText = LanguageManager.GetText("StatusCookieDead");
                        statusColor = Color.FromArgb(255, 0, 85); // Dead Red
                        statusBg = Color.FromArgb(30, 255, 0, 85);
                        statusBorder = Color.FromArgb(80, 255, 0, 85);
                    }
                    else if (account.Presence != null)
                    {
                        switch (account.Presence.userPresenceType)
                        {
                            case UserPresenceType.InGame:
                                statusText = LanguageManager.GetText("StatusFarming");
                                statusColor = Color.FromArgb(0, 242, 254); // Cyan
                                statusBg = Color.FromArgb(30, 0, 242, 254);
                                statusBorder = Color.FromArgb(80, 0, 242, 254);
                                break;
                            case UserPresenceType.InStudio:
                                statusText = LanguageManager.GetText("StatusInStudio");
                                statusColor = Color.FromArgb(157, 78, 221); // Violet
                                statusBg = Color.FromArgb(30, 157, 78, 221);
                                statusBorder = Color.FromArgb(80, 157, 78, 221);
                                break;
                            case UserPresenceType.Online:
                                statusText = LanguageManager.GetText("StatusOnline");
                                statusColor = Color.FromArgb(0, 230, 118);
                                statusBg = Color.FromArgb(30, 0, 230, 118);
                                statusBorder = Color.FromArgb(80, 0, 230, 118);
                                break;
                            default:
                                statusText = LanguageManager.GetText("StatusOffline");
                                statusColor = Color.FromArgb(123, 132, 163);
                                statusBg = Color.FromArgb(20, 255, 255, 255);
                                statusBorder = Color.FromArgb(40, 255, 255, 255);
                                break;
                        }
                    }

                    int pillW = 105;
                    int pillH = 26;
                    int pillX = r.X + 8;
                    int pillY = r.Y + (r.Height - pillH) / 2;

                    Rectangle pillRect = new Rectangle(pillX, pillY, pillW, pillH);
                    using (GraphicsPath path = GetRoundedRectanglePath(pillRect, 13))
                    using (SolidBrush bgBrush = new SolidBrush(statusBg))
                    using (Pen borderPen = new Pen(statusBorder, 1f))
                    using (SolidBrush textBrush = new SolidBrush(statusColor))
                    using (Font font = new Font("Segoe UI Emoji", 8.25f, FontStyle.Bold))
                    {
                        g.FillPath(bgBrush, path);
                        g.DrawPath(borderPen, path);

                        // Glow dot
                        int dotSize = 6;
                        int dotX = pillX + 10;
                        int dotY = pillY + (pillH - dotSize) / 2;
                        using (SolidBrush dotBrush = new SolidBrush(statusColor))
                        {
                            g.FillEllipse(dotBrush, dotX, dotY, dotSize, dotSize);
                        }

                        SizeF sz = g.MeasureString(statusText, font);
                        g.DrawString(statusText, font, textBrush, dotX + dotSize + 6, pillY + (pillH - sz.Height) / 2);
                    }
                }
            }
            // Column 3: Description (Resources & Tags)
            else if (isCol3)
            {
                string desc = account.Description ?? string.Empty;
                int startX = r.X + 8;

                if (string.IsNullOrEmpty(desc))
                {
                    using (Font font = new Font("Segoe UI Emoji", 8.5f, FontStyle.Italic))
                    using (Brush brush = new SolidBrush(Color.FromArgb(90, 100, 130)))
                    {
                        g.DrawString("- No Description -", font, brush, startX, r.Y + (r.Height - 16) / 2);
                    }
                }
                else
                {
                    // Render description items as custom styled pills/tags if contains comma or key-values
                    string[] parts = desc.Split(new char[] { ',', '|', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    int currentX = startX;
                    int tagH = 24;
                    int tagY = r.Y + (r.Height - tagH) / 2;

                    if (parts.Length > 1)
                    {
                        foreach (string part in parts)
                        {
                            string itemStr = part.Trim();
                            if (string.IsNullOrEmpty(itemStr)) continue;

                            Color tagBg = Color.FromArgb(20, 255, 255, 255);
                            Color tagBorder = Color.FromArgb(40, 255, 255, 255);
                            Color tagTextClr = Color.FromArgb(220, 225, 245);

                            if (itemStr.ToLower().Contains("lv") || itemStr.ToLower().Contains("เลเวล")) { tagTextClr = Color.FromArgb(255, 183, 3); tagBg = Color.FromArgb(25, 255, 183, 3); }
                            else if (itemStr.ToLower().Contains("gem")) { tagTextClr = Color.FromArgb(0, 242, 254); tagBg = Color.FromArgb(25, 0, 242, 254); }
                            else if (itemStr.ToLower().Contains("gold") || itemStr.ToLower().Contains("coin")) { tagTextClr = Color.FromArgb(255, 215, 0); tagBg = Color.FromArgb(25, 255, 215, 0); }
                            else if (itemStr.ToLower().Contains("robux")) { tagTextClr = Color.FromArgb(157, 78, 221); tagBg = Color.FromArgb(25, 157, 78, 221); }

                            using (Font font = new Font("Segoe UI Emoji", 8.25f, FontStyle.Bold))
                            {
                                SizeF sz = g.MeasureString(itemStr, font);
                                int tagW = (int)sz.Width + 16;

                                if (currentX + tagW > r.Right - 8) break;

                                Rectangle tagRect = new Rectangle(currentX, tagY, tagW, tagH);
                                using (GraphicsPath path = GetRoundedRectanglePath(tagRect, 6))
                                using (SolidBrush bgB = new SolidBrush(tagBg))
                                using (Pen borderP = new Pen(tagBorder, 1f))
                                using (SolidBrush textB = new SolidBrush(tagTextClr))
                                {
                                    g.FillPath(bgB, path);
                                    g.DrawPath(borderP, path);
                                    g.DrawString(itemStr, font, textB, currentX + 8, tagY + (tagH - sz.Height) / 2);
                                }
                                currentX += tagW + 6;
                            }
                        }
                    }
                    else
                    {
                        using (Font font = new Font("Segoe UI", 8.75f, FontStyle.Regular))
                        using (Brush textBrush = new SolidBrush(Color.FromArgb(200, 210, 235)))
                        {
                            g.DrawString(desc, font, textBrush, new RectangleF(startX, r.Y + 8, r.Width - 16, r.Height - 14));
                        }
                    }
                }
            }
        }

        private static void DrawPlaceholderAvatar(Graphics g, int x, int y, int w, int h, string username)
        {
            Rectangle rect = new Rectangle(x, y, w, h);
            using (GraphicsPath path = GetRoundedRectanglePath(rect, 10))
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
    }
}

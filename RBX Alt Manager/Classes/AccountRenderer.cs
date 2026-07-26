using System;
using System.Drawing;

namespace RBX_Alt_Manager.Classes
{
    public class AccountRenderer : BrightIdeasSoftware.BaseRenderer
    {
        public override void Render(Graphics g, Rectangle r)
        {
            Account account = RowObject as Account;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (IsItemSelected)
            {
                using (SolidBrush selBrush = new SolidBrush(Color.FromArgb(0x63, 0x66, 0xF1)))
                {
                    g.FillRectangle(selBrush, r);
                }
            }
            else
            {
                base.Render(g, r);
            }

            if (account == null) return;

            TimeSpan diff = DateTime.Now - account.LastUse;
            bool isOld = diff.TotalDays > 20;

            int dotX = r.X + (int)(4f * Program.Scale);
            int dotY = r.Y + (r.Height / 2) - (int)(3f * Program.Scale);
            int dotSize = (int)(6f * Program.Scale);

            if (isOld)
            {
                diff -= TimeSpan.FromDays(20);
                using (Brush b = new SolidBrush(Color.FromArgb(255, 255, 204, 77).Lerp(Color.FromArgb(255, 250, 26, 13), (float)Utilities.MapValue(diff.TotalSeconds, 0, 864000, 0, 1).Clamp(0, 1))))
                    g.FillEllipse(b, new Rectangle(dotX, dotY, dotSize, dotSize));
                dotX += dotSize + 3;
            }

            if (AccountManager.General.Get<bool>("ShowPresence") && account.Presence != null)
            {
                Color statusColor = Presence.Colors.ContainsKey(account.Presence.userPresenceType) 
                    ? Presence.Colors[account.Presence.userPresenceType] 
                    : Color.Gray;

                using (Brush b = new SolidBrush(statusColor))
                    g.FillEllipse(b, new Rectangle(dotX, dotY, dotSize, dotSize));
            }
        }
    }
}
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace RBX_Alt_Manager.Classes
{
    public class NTGHeaderRenderer : HeaderFormatStyle
    {
        public NTGHeaderRenderer()
        {
            HeaderStateStyle normalStyle = new HeaderStateStyle()
            {
                BackColor = Color.FromArgb(14, 18, 30),
                ForeColor = Color.FromArgb(123, 132, 163),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                FrameColor = Color.FromArgb(20, 0, 242, 254)
            };

            this.Normal = normalStyle;
            this.Hot = new HeaderStateStyle()
            {
                BackColor = Color.FromArgb(20, 25, 42),
                ForeColor = Color.FromArgb(0, 242, 254),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                FrameColor = Color.FromArgb(60, 0, 242, 254)
            };
            this.Pressed = normalStyle;
        }
    }
}

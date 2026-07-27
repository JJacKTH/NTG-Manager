using FastColoredTextBoxNS;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace RBX_Alt_Manager.Forms
{
    public partial class ThemeEditor : Form
    {
        public static Color AccountBackground = Color.FromArgb(0x0F, 0x17, 0x2A);
        public static Color AccountForeground = Color.FromArgb(0xF8, 0xFA, 0xFC);

        public static Color ButtonsBackground = Color.FromArgb(0x1E, 0x29, 0x3B);
        public static Color ButtonsForeground = Color.FromArgb(0xF8, 0xFA, 0xFC);
        public static Color ButtonsBorder = Color.FromArgb(0x63, 0x66, 0xF1);
        public static FlatStyle ButtonStyle = FlatStyle.Flat;

        public static Color FormsBackground = Color.FromArgb(0x09, 0x0D, 0x16);
        public static Color FormsForeground = Color.FromArgb(0xF8, 0xFA, 0xFC);
        public static bool UseDarkTopBar = true;
        public static bool ShowHeaders = true;

        public static Color TextBoxesBackground = Color.FromArgb(0x1E, 0x29, 0x3B);
        public static Color TextBoxesForeground = Color.FromArgb(0xF8, 0xFA, 0xFC);
        public static Color TextBoxesBorder = Color.FromArgb(0x47, 0x55, 0x69);

        public static Color LabelBackground = Color.Transparent;
        public static Color LabelForeground = Color.FromArgb(0xF8, 0xFA, 0xFC);
        public static bool LabelTransparent = true;
        
        public static bool LightImages = false;
        // public static bool UseNormalTabControls = false;

        public static string ToHexString(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        private static IniFile ThemeIni;
        private static IniSection Theme;

        public ThemeEditor()
        {
            AccountManager.SetDarkBar(Handle);

            InitializeComponent();
            this.Rescale();
        }

        public void ApplyTheme()
        {
            BackColor = FormsBackground;
            ForeColor = FormsForeground;

            foreach (Control control in this.Controls)
            {
                if (control is Button || control is CheckBox)
                {
                    if (control is Button)
                    {
                        Button b = control as Button;
                        b.FlatStyle = ButtonStyle;
                        b.FlatAppearance.BorderColor = ButtonsBorder;
                    }

                    if (!(control is CheckBox)) control.BackColor = ButtonsBackground;
                    control.ForeColor = ButtonsForeground;
                }
                else if (control is TextBox || control is RichTextBox)
                {
                    if (control is Classes.BorderedTextBox)
                    {
                        Classes.BorderedTextBox b = control as Classes.BorderedTextBox;
                        b.BorderColor = TextBoxesBorder;
                    }

                    if (control is Classes.BorderedRichTextBox)
                    {
                        Classes.BorderedRichTextBox b = control as Classes.BorderedRichTextBox;
                        b.BorderColor = TextBoxesBorder;
                    }

                    control.BackColor = TextBoxesBackground;
                    control.ForeColor = TextBoxesForeground;
                }
                else if (control is Label)
                {
                    control.BackColor = LabelTransparent ? Color.Transparent : LabelBackground;
                    control.ForeColor = LabelForeground;
                }
                else if (control is ListBox)
                {
                    control.BackColor = ButtonsBackground;
                    control.ForeColor = ButtonsForeground;
                }
            }
        }

        public static void LoadTheme()
        {
            ThemeIni ??= File.Exists(Path.Combine(Environment.CurrentDirectory, "RAMTheme.ini")) ? new IniFile("RAMTheme.ini") : new IniFile();

            Theme = ThemeIni.Section(Assembly.GetExecutingAssembly().GetName().Name);

            // bool.TryParse(Theme.Get("DisableCustomTabs"), out UseNormalTabControls);

            // if (!Theme.Exists("DisableCustomTabs")) { Theme.Set("DisableCustomTabs", "false"); ThemeIni.Save("RAMTheme.ini"); }

            if (Theme.Exists("AccountsBG")) AccountBackground = ColorTranslator.FromHtml(Theme.Get("AccountsBG"));
            if (Theme.Exists("AccountsFG")) AccountForeground = ColorTranslator.FromHtml(Theme.Get("AccountsFG"));

            if (Theme.Exists("ButtonsBG")) ButtonsBackground = ColorTranslator.FromHtml(Theme.Get("ButtonsBG"));
            if (Theme.Exists("ButtonsFG")) ButtonsForeground = ColorTranslator.FromHtml(Theme.Get("ButtonsFG"));
            if (Theme.Exists("ButtonsBC")) ButtonsBorder = ColorTranslator.FromHtml(Theme.Get("ButtonsBC"));
            if (Theme.Exists("ButtonStyle") && Enum.TryParse(Theme.Get("ButtonStyle"), out FlatStyle BS)) ButtonStyle = BS;

            if (Theme.Exists("FormsBG")) FormsBackground = ColorTranslator.FromHtml(Theme.Get("FormsBG"));
            if (Theme.Exists("FormsFG")) FormsForeground = ColorTranslator.FromHtml(Theme.Get("FormsFG"));
            if (Theme.Exists("DarkTopBar") && bool.TryParse(Theme.Get("DarkTopBar"), out bool DarkTopBar)) UseDarkTopBar = DarkTopBar;
            if (Theme.Exists("ShowHeaders") && bool.TryParse(Theme.Get("ShowHeaders"), out bool bShowHeaders)) ShowHeaders = bShowHeaders;

            if (Theme.Exists("TextBoxesBG")) TextBoxesBackground = ColorTranslator.FromHtml(Theme.Get("TextBoxesBG"));
            if (Theme.Exists("TextBoxesFG")) TextBoxesForeground = ColorTranslator.FromHtml(Theme.Get("TextBoxesFG"));
            if (Theme.Exists("TextBoxesBC")) TextBoxesBorder = ColorTranslator.FromHtml(Theme.Get("TextBoxesBC"));

            if (Theme.Exists("TextBoxesBG") && !Theme.Exists("LabelsTransparent")) LabelTransparent = false; // support old themes
            if (Theme.Exists("LabelsBC")) LabelBackground = ColorTranslator.FromHtml(Theme.Get("LabelsBC")); else LabelBackground = TextBoxesBackground;
            if (Theme.Exists("LabelsFC")) LabelForeground = ColorTranslator.FromHtml(Theme.Get("LabelsFC")); else LabelForeground = TextBoxesForeground;
            if (Theme.Exists("LabelsTransparent") && bool.TryParse(Theme.Get("LabelsTransparent"), out bool bLabelTransparent)) LabelTransparent = bLabelTransparent;

            if (!Theme.Exists("LightImages")) Theme.Set("LightImages", FormsBackground.GetBrightness() < 0.5 ? "true" : "false");
            if (bool.TryParse(Theme.Get("LightImages"), out bool bLightImages)) LightImages = bLightImages;
        }

        public static void SaveTheme()
        {
            ThemeIni ??= File.Exists(Path.Combine(Environment.CurrentDirectory, "RAMTheme.ini")) ? new IniFile("RAMTheme.ini") : new IniFile();
            Theme ??= ThemeIni.Section(Assembly.GetExecutingAssembly().GetName().Name);

            Theme.Set("AccountsBG", ToHexString(AccountBackground));
            Theme.Set("AccountsFG", ToHexString(AccountForeground));

            Theme.Set("ButtonsBG", ToHexString(ButtonsBackground));
            Theme.Set("ButtonsFG", ToHexString(ButtonsForeground));
            Theme.Set("ButtonsBC", ToHexString(ButtonsBorder));
            Theme.Set("ButtonStyle", ButtonStyle.ToString());

            Theme.Set("FormsBG", ToHexString(FormsBackground));
            Theme.Set("FormsFG", ToHexString(FormsForeground));
            Theme.Set("DarkTopBar", UseDarkTopBar.ToString());
            Theme.Set("ShowHeaders", ShowHeaders.ToString());

            Theme.Set("TextBoxesBG", ToHexString(TextBoxesBackground));
            Theme.Set("TextBoxesFG", ToHexString(TextBoxesForeground));
            Theme.Set("TextBoxesBC", ToHexString(TextBoxesBorder));

            Theme.Set("LabelsBC", ToHexString(LabelBackground));
            Theme.Set("LabelsFC", ToHexString(LabelForeground));
            Theme.Set("LabelsTransparent", LabelTransparent.ToString());

            Theme.Set("LightImages", LightImages.ToString());

            ThemeIni.Save("RAMTheme.ini");
        }

        public static void ApplyPresetTheme(int themeIndex)
        {
            switch (themeIndex)
            {
                case 0: // ธีมที่ 1: Slate Blue (โทนเทาอมฟ้าสไตล์ซอฟต์)
                    FormsBackground = Color.FromArgb(0x11, 0x15, 0x22); 
                    FormsForeground = Color.FromArgb(0xF8, 0xFA, 0xFC);
                    AccountBackground = Color.FromArgb(0x18, 0x20, 0x30); 
                    AccountForeground = Color.FromArgb(0xF8, 0xFA, 0xFC);
                    ButtonsBackground = Color.FromArgb(0x0D, 0x94, 0x88);
                    ButtonsForeground = Color.FromArgb(0xFF, 0xFF, 0xFF);
                    ButtonsBorder = Color.FromArgb(0x14, 0xB8, 0xA6);
                    TextBoxesBackground = Color.FromArgb(0x1E, 0x26, 0x38);
                    TextBoxesForeground = Color.FromArgb(0xF8, 0xFA, 0xFC);
                    TextBoxesBorder = Color.FromArgb(0x33, 0x41, 0x55);
                    LabelForeground = Color.FromArgb(0xE2, 0xE8, 0xF0);
                    UseDarkTopBar = true;
                    LightImages = false;
                    break;

                case 1: // ธีมที่ 2: Warm Gray (โทนอุ่น สบายสายตา)
                    FormsBackground = Color.FromArgb(0x1A, 0x18, 0x17); 
                    FormsForeground = Color.FromArgb(0xF5, 0xF5, 0xF4);
                    AccountBackground = Color.FromArgb(0x24, 0x22, 0x20); 
                    AccountForeground = Color.FromArgb(0xF5, 0xF5, 0xF4);
                    ButtonsBackground = Color.FromArgb(0x57, 0x70, 0x52); 
                    ButtonsForeground = Color.FromArgb(0xFF, 0xFF, 0xFF);
                    ButtonsBorder = Color.FromArgb(0x6A, 0x88, 0x64);
                    TextBoxesBackground = Color.FromArgb(0x2E, 0x2B, 0x29);
                    TextBoxesForeground = Color.FromArgb(0xF5, 0xF5, 0xF4);
                    TextBoxesBorder = Color.FromArgb(0x44, 0x40, 0x3C);
                    LabelForeground = Color.FromArgb(0xE7, 0xE5, 0xE4);
                    UseDarkTopBar = true;
                    LightImages = false;
                    break;

                case 2: // ธีมที่ 3: Cool Neutral Modern (โทนเทากลาง มินิมอลสไตล์ macOS)
                    FormsBackground = Color.FromArgb(0x16, 0x18, 0x1D); 
                    FormsForeground = Color.FromArgb(0xF8, 0xFA, 0xFC);
                    AccountBackground = Color.FromArgb(0x1E, 0x22, 0x2B); 
                    AccountForeground = Color.FromArgb(0xF8, 0xFA, 0xFC);
                    ButtonsBackground = Color.FromArgb(0x25, 0x63, 0xEB); 
                    ButtonsForeground = Color.FromArgb(0xFF, 0xFF, 0xFF);
                    ButtonsBorder = Color.FromArgb(0x3B, 0x82, 0xF6);
                    TextBoxesBackground = Color.FromArgb(0x26, 0x2A, 0x35);
                    TextBoxesForeground = Color.FromArgb(0xF8, 0xFA, 0xFC);
                    TextBoxesBorder = Color.FromArgb(0x3B, 0x42, 0x52);
                    LabelForeground = Color.FromArgb(0xF1, 0xF5, 0xF9);
                    UseDarkTopBar = true;
                    LightImages = false;
                    break;

                case 3: // ธีมที่ 4: Mid-tone Pastel (โทนพาสเทลกลางๆ ดูสดใส)
                    FormsBackground = Color.FromArgb(0x18, 0x15, 0x24); 
                    FormsForeground = Color.FromArgb(0xF8, 0xFA, 0xFC);
                    AccountBackground = Color.FromArgb(0x22, 0x1C, 0x33); 
                    AccountForeground = Color.FromArgb(0xF8, 0xFA, 0xFC);
                    ButtonsBackground = Color.FromArgb(0x0D, 0x94, 0x88); 
                    ButtonsForeground = Color.FromArgb(0xFF, 0xFF, 0xFF);
                    ButtonsBorder = Color.FromArgb(0x2D, 0xD4, 0xBF);
                    TextBoxesBackground = Color.FromArgb(0x2B, 0x24, 0x3E);
                    TextBoxesForeground = Color.FromArgb(0xF8, 0xFA, 0xFC);
                    TextBoxesBorder = Color.FromArgb(0x4C, 0x3D, 0x6B);
                    LabelForeground = Color.FromArgb(0xF3, 0xE8, 0xFF);
                    UseDarkTopBar = true;
                    LightImages = false;
                    break;
            }

            AccountManager.Watcher.Set("SelectedThemeIndex", themeIndex.ToString());
            AccountManager.Instance?.ApplyTheme();
            SaveTheme();
        }

        private void SetBG_Click(object sender, EventArgs e) { }

        private void SetFG_Click(object sender, EventArgs e)
        {
            string Selected = Selection.SelectedItem as string;

            if (string.IsNullOrEmpty(Selected)) return;

            if (SelectColor.ShowDialog() == DialogResult.OK)
            {
                switch (Selected)
                {
                    case "Accounts":
                        AccountForeground = SelectColor.Color;
                        break;

                    case "Buttons":
                        ButtonsForeground = SelectColor.Color;
                        break;

                    case "Forms":
                        FormsForeground = SelectColor.Color;
                        break;

                    case "Text Boxes":
                        TextBoxesForeground = SelectColor.Color;
                        break;

                    case "Labels":
                        LabelForeground = SelectColor.Color;
                        break;
                }

                AccountManager.Instance.ApplyTheme();
                SaveTheme();
            }
        }

        private void ThemeEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void ShowControls(params Control[] controls)
        {
            SetBorder.Visible = false;
            ChangeStyle.Visible = false;
            HideHeaders.Visible = false;
            ToggleDarkTopBar.Visible = false;
            ToggleTransparentBG.Visible = false;

            foreach (Control control in controls)
                control.Visible = true;
        }

        private void Selection_SelectedIndexChanged(object sender, EventArgs e)
        {
            string Selected = Selection.SelectedItem as string;

            if (string.IsNullOrEmpty(Selected)) return;

            if (Selected == "Buttons")
                ShowControls(SetBorder, ChangeStyle);
            else if (Selected == "Text Boxes")
                ShowControls(SetBorder);
            else if (Selected == "Accounts")
                ShowControls(HideHeaders);
            else if (Selected == "Forms")
                ShowControls(ToggleDarkTopBar);
            else if (Selected == "Labels")
                ShowControls(ToggleTransparentBG);
            else
                ShowControls();
        }

        private void SetBorder_Click(object sender, EventArgs e)
        {
            string Selected = Selection.SelectedItem as string;

            if (string.IsNullOrEmpty(Selected)) return;

            if (SelectColor.ShowDialog() == DialogResult.OK)
            {
                switch (Selected)
                {
                    case "Buttons":
                        ButtonsBorder = SelectColor.Color;
                        break;

                    case "Text Boxes":
                        TextBoxesBorder = SelectColor.Color;
                        break;
                }

                AccountManager.Instance.ApplyTheme();
                SaveTheme();
            }
        }

        private void ChangeStyle_Click(object sender, EventArgs e)
        {
            ButtonStyle = ButtonStyle.Next();
            AccountManager.Instance.ApplyTheme();
            SaveTheme();
        }

        private void HideHeaders_Click(object sender, EventArgs e)
        {
            ShowHeaders = !ShowHeaders;
            AccountManager.Instance.ApplyTheme();
            SaveTheme();
        }

        private void ToggleTransparentBG_Click(object sender, EventArgs e)
        {
            LabelTransparent = !LabelTransparent;
            AccountManager.Instance.ApplyTheme();
            SaveTheme();
        }

        private void ToggleDarkTopBar_Click(object sender, EventArgs e)
        {
            UseDarkTopBar = !UseDarkTopBar;
            SaveTheme();
            MessageBox.Show("This option requires RAM to be restarted.\nThis may not work on older versions of windows.\nEnabled: " + (UseDarkTopBar ? "True" : "false"), "Roblox Account Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
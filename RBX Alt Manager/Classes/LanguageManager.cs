using System;
using System.Collections.Generic;

namespace RBX_Alt_Manager.Classes
{
    public enum Language
    {
        TH,
        EN
    }

    public static class LanguageManager
    {
        public static Language CurrentLanguage { get; set; } = Language.EN;

        public static event Action LanguageChanged;

        public static void ToggleLanguage()
        {
            CurrentLanguage = CurrentLanguage == Language.EN ? Language.TH : Language.EN;
            LanguageChanged?.Invoke();
        }

        // ponytail: clean dictionary lookup for text translations
        public static string GetText(string key) =>
            Translations.TryGetValue(key, out var langDict) && langDict.TryGetValue(CurrentLanguage, out var text) ? text : key;

        private static readonly Dictionary<string, Dictionary<Language, string>> Translations = new Dictionary<string, Dictionary<Language, string>>()
        {
            // App Title & Header
            { "AppTitle", new Dictionary<Language, string> { { Language.TH, "NTG Manager 2026" }, { Language.EN, "NTG Manager 2026" } } },
            { "BrandTag", new Dictionary<Language, string> { { Language.TH, "ULTIMATE" }, { Language.EN, "ULTIMATE" } } },
            { "LangButton", new Dictionary<Language, string> { { Language.TH, "🇹🇭 ภาษาไทย" }, { Language.EN, "🇺🇸 English" } } },

            // Sidebar Menu
            { "MenuMain", new Dictionary<Language, string> { { Language.TH, "เมนูหลัก" }, { Language.EN, "MAIN MENU" } } },
            { "NavAccounts", new Dictionary<Language, string> { { Language.TH, "👥 จัดการบัญชี" }, { Language.EN, "👥 Accounts Manager" } } },
            { "NavGames", new Dictionary<Language, string> { { Language.TH, "🎮 เกมล่าสุด" }, { Language.EN, "🎮 Recent Games" } } },
            { "NavAutoJoiner", new Dictionary<Language, string> { { Language.TH, "⚡ Auto Joiner" }, { Language.EN, "⚡ Auto Joiner" } } },
            { "NavVipServers", new Dictionary<Language, string> { { Language.TH, "🖥️ เซิร์ฟเวอร์ VIP" }, { Language.EN, "🖥️ VIP Servers" } } },
            { "MenuSystem", new Dictionary<Language, string> { { Language.TH, "ระบบ" }, { Language.EN, "SYSTEM" } } },
            { "NavSecurity", new Dictionary<Language, string> { { Language.TH, "🛡️ ความปลอดภัย" }, { Language.EN, "🛡️ Security & Cookies" } } },
            { "NavWatcher", new Dictionary<Language, string> { { Language.TH, "👁️ ตั้งค่า Watcher" }, { Language.EN, "👁️ Roblox Watcher" } } },
            { "NavSettings", new Dictionary<Language, string> { { Language.TH, "⚙️ ตั้งค่า" }, { Language.EN, "⚙️ Settings" } } },

            // KPI Badges
            { "KpiTotalAccounts", new Dictionary<Language, string> { { Language.TH, "ไอดีทั้งหมด" }, { Language.EN, "Total Accounts" } } },
            { "KpiFarmingActive", new Dictionary<Language, string> { { Language.TH, "กำลังฟาร์ม (Farming)" }, { Language.EN, "Farming Active" } } },
            { "KpiTotalRobux", new Dictionary<Language, string> { { Language.TH, "Robux รวมทั้งหมด" }, { Language.EN, "Total Robux" } } },
            { "KpiDeadLocked", new Dictionary<Language, string> { { Language.TH, "คุกกี้มีปัญหา/โดนแขวน" }, { Language.EN, "Dead/Locked" } } },

            // Table Columns
            { "ColNum", new Dictionary<Language, string> { { Language.TH, "#" }, { Language.EN, "#" } } },
            { "ColAvatarUsername", new Dictionary<Language, string> { { Language.TH, "Avatar และ Username" }, { Language.EN, "Avatar & Username" } } },
            { "ColPID", new Dictionary<Language, string> { { Language.TH, "PID Process" }, { Language.EN, "PID Process" } } },
            { "ColStatus", new Dictionary<Language, string> { { Language.TH, "Status" }, { Language.EN, "Status" } } },
            { "ColDescription", new Dictionary<Language, string> { { Language.TH, "Description (ทรัพยากรตัวละคร)" }, { Language.EN, "Description (Stats & Details)" } } },

            // Action Buttons
            { "BtnSearchPlaceholder", new Dictionary<Language, string> { { Language.TH, "🔍 ค้นหาตามชื่อ หรือ Display Name..." }, { Language.EN, "🔍 Search Username or Display Name..." } } },
            { "BtnRefreshAvatars", new Dictionary<Language, string> { { Language.TH, "🔄 รีเฟรชอวาตาร์" }, { Language.EN, "🔄 Refresh Avatars" } } },
            { "BtnAddAccount", new Dictionary<Language, string> { { Language.TH, "➕ เพิ่มบัญชีใหม่" }, { Language.EN, "➕ Add Account" } } },
            { "BtnDeleteSelected", new Dictionary<Language, string> { { Language.TH, "🗑️ ลบบัญชีที่เลือก" }, { Language.EN, "🗑️ Delete Selected" } } },
            { "BtnLaunch", new Dictionary<Language, string> { { Language.TH, "🚀 เข้าเล่นเกม (Launch)" }, { Language.EN, "🚀 Launch Selected" } } },
            { "BtnOpenBrowser", new Dictionary<Language, string> { { Language.TH, "🌐 เปิดเว็บเบราว์เซอร์" }, { Language.EN, "🌐 Open Browser" } } },
            { "BtnAccountUtility", new Dictionary<Language, string> { { Language.TH, "🛠️ Account Utility" }, { Language.EN, "🛠️ Account Utility" } } },
            { "BtnAccountControl", new Dictionary<Language, string> { { Language.TH, "🎛️ Account Control" }, { Language.EN, "🎛️ Account Control" } } },

            // Status Bar
            { "StatusReady", new Dictionary<Language, string> { { Language.TH, "เลือก {0} บัญชี" }, { Language.EN, "Selected {0} account(s)" } } },

            // Status Tags
            { "StatusFarming", new Dictionary<Language, string> { { Language.TH, "FARMING" }, { Language.EN, "FARMING" } } },
            { "StatusOnline", new Dictionary<Language, string> { { Language.TH, "ONLINE" }, { Language.EN, "ONLINE" } } },
            { "StatusInStudio", new Dictionary<Language, string> { { Language.TH, "IN STUDIO" }, { Language.EN, "IN STUDIO" } } },
            { "StatusOffline", new Dictionary<Language, string> { { Language.TH, "OFFLINE" }, { Language.EN, "OFFLINE" } } },
            { "StatusCookieDead", new Dictionary<Language, string> { { Language.TH, "COOKIE DEAD" }, { Language.EN, "COOKIE DEAD" } } },

            // Details Panel Labels
            { "LabelPlaceID", new Dictionary<Language, string> { { Language.TH, "Place ID (รหัสเกม):" }, { Language.EN, "Place ID:" } } },
            { "LabelJobID", new Dictionary<Language, string> { { Language.TH, "Job ID / Private Server:" }, { Language.EN, "Job ID / Server Link:" } } },
            { "LabelAlias", new Dictionary<Language, string> { { Language.TH, "ชื่อเรียก (Alias):" }, { Language.EN, "Account Alias:" } } },
            { "LabelDescription", new Dictionary<Language, string> { { Language.TH, "รายละเอียด (Description):" }, { Language.EN, "Description / Stats:" } } },
            { "BtnSaveAlias", new Dictionary<Language, string> { { Language.TH, "บันทึก Alias" }, { Language.EN, "Save Alias" } } },
            { "BtnSaveDesc", new Dictionary<Language, string> { { Language.TH, "บันทึก Desc" }, { Language.EN, "Save Desc" } } },

            // Notifications
            { "PromptSelectAccount", new Dictionary<Language, string> { { Language.TH, "กรุณาเลือกบัญชีที่ต้องการเข้าเล่น" }, { Language.EN, "Please select an account to launch." } } },
            { "PromptConfirmDelete", new Dictionary<Language, string> { { Language.TH, "คุณแน่ใจหรือไม่ว่าต้องการลบบัญชีที่เลือก ({0} บัญชี)?" }, { Language.EN, "Are you sure you want to delete selected account(s) ({0})?" } } },
            { "ConfirmTitle", new Dictionary<Language, string> { { Language.TH, "ยืนยันการทำรายการ" }, { Language.EN, "Confirm Action" } } }
        };
    }
}

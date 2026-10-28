using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PalKeybindManagerPanel
{
    internal sealed class MappingRow
    {
        public string Id;
        public string Kind;
        public int Index;
        public string Name;
        public string ChineseName;
        public string OriginalKey;
        public string CurrentKey;
        public string Scale;
        public string Modifiers;
        public bool Editable;
        public bool Overridden;
    }

    internal sealed class MainForm : Form
    {
        private static readonly Dictionary<string, string> ChineseNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Jump", "跳跃" },
            { "ResetVR", "重置 VR 视角" },
            { "WeaponReady", "瞄准／举起武器" },
            { "WeaponUse", "攻击／使用武器" },
            { "ThrowPal", "投掷并召唤帕鲁" },
            { "Coop", "互动／协作操作" },
            { "Reload", "装填弹药" },
            { "ReloadAndCoop", "装填／互动（手柄共用）" },
            { "ChangeWeaponNext", "切换下一把武器" },
            { "ChangeWeaponPrev", "切换上一把武器" },
            { "UIINputGuardCancel", "界面取消／返回" },
            { "RidingSkill1", "骑乘技能 1" },
            { "RidingSkill2", "骑乘技能 2" },
            { "RidingSkill3", "骑乘技能 3" },
            { "RidingSkill3_GamePad", "骑乘技能 3（手柄）" },
            { "RidingSkill3_Additional_GamePad", "骑乘技能 3 追加键（手柄）" },
            { "Crouch", "蹲下" },
            { "Rolling", "翻滚" },
            { "RollingAndCrouch", "翻滚／蹲下（手柄共用）" },
            { "Dash", "疾跑" },
            { "DashController", "疾跑（手柄）" },
            { "AutoRun", "自动奔跑" },
            { "VoiceChatPushToTalk", "按键说话" },
            { "ThrowObject", "投掷物品／帕鲁球" },
            { "PartnerPalInstructions", "伙伴帕鲁指令" },
            { "DebugWindow", "调试窗口（隐藏功能）" },
            { "ChangePalPrev", "切换上一只帕鲁" },
            { "CancelThrowAim", "取消投掷瞄准" },
            { "ChangeThrowObjectPrev", "切换上一种投掷物" },
            { "RadialMenuCancel", "关闭环形菜单" },
            { "DismantleBuildObject", "拆除建筑" },
            { "CancelDismantleMode", "退出拆除模式" },
            { "OtomoChangeIncrement", "切换下一只伙伴帕鲁" },
            { "OtomoChangeDecrement", "切换上一只伙伴帕鲁" },
            { "OpenConstructionMenu", "打开建造菜单" },
            { "SphereChange", "切换帕鲁球" },
            { "GrapplingCancel", "取消抓钩" },
            { "Interact2", "互动操作 2" },
            { "Interact3", "互动操作 3" },
            { "Interact4", "互动操作 4" },
            { "ChangeFishingBaitNext", "切换下一种鱼饵" },
            { "ChangeFishingBaitPrev", "切换上一种鱼饵" },
            { "ChangeWeaponNext_GamePad", "切换下一把武器（手柄）" },
            { "DirectAttackOrder", "伙伴直接攻击指令（隐藏）" },
            { "MoveForward", "前进／后退" },
            { "MoveRight", "向右／向左移动" },
            { "Turn", "水平转动视角（鼠标）" },
            { "LookUp", "垂直转动视角（鼠标）" },
            { "TurnRate", "水平转动视角（手柄／VR）" },
            { "LookUpRate", "垂直转动视角（手柄）" },
            { "MoveForwardController", "前进／后退（手柄）" },
            { "MoveRightController", "左右移动（手柄）" }
        };

        private static readonly Dictionary<string, string> ChineseKeyNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "None", "（已禁用）" },
            { "__DISABLED__", "（已禁用）" },
            { "SpaceBar", "空格" },
            { "LeftMouseButton", "鼠标左键" },
            { "RightMouseButton", "鼠标右键" },
            { "MiddleMouseButton", "鼠标中键" },
            { "ThumbMouseButton", "鼠标侧键 1" },
            { "ThumbMouseButton2", "鼠标侧键 2" },
            { "MouseScrollUp", "滚轮向上" },
            { "MouseScrollDown", "滚轮向下" },
            { "LeftShift", "左 Shift" },
            { "RightShift", "右 Shift" },
            { "LeftControl", "左 Ctrl" },
            { "RightControl", "右 Ctrl" },
            { "LeftAlt", "左 Alt" },
            { "RightAlt", "右 Alt" },
            { "BackSpace", "退格键" },
            { "Enter", "回车键" },
            { "Escape", "Esc" },
            { "Delete", "Delete（删除）" },
            { "Insert", "Insert（插入）" },
            { "Home", "Home（行首）" },
            { "End", "End（行尾）" },
            { "PageUp", "Page Up（上翻页）" },
            { "PageDown", "Page Down（下翻页）" },
            { "Left", "方向键左" },
            { "Right", "方向键右" },
            { "Up", "方向键上" },
            { "Down", "方向键下" },
            { "Zero", "0" },
            { "One", "1" },
            { "Two", "2" },
            { "Three", "3" },
            { "Four", "4" },
            { "Five", "5" },
            { "Six", "6" },
            { "Seven", "7" },
            { "Eight", "8" },
            { "Nine", "9" },
            { "Comma", "逗号 ," },
            { "Period", "句号 ." },
            { "Slash", "斜杠 /" },
            { "Backslash", "反斜杠 \\" },
            { "Semicolon", "分号 ;" },
            { "Apostrophe", "单引号 '" },
            { "LeftBracket", "左方括号 [" },
            { "RightBracket", "右方括号 ]" },
            { "Hyphen", "减号 -" },
            { "Equals", "等号 =" },
            { "Tilde", "波浪号 ~" },
            { "MouseX", "鼠标水平移动" },
            { "MouseY", "鼠标垂直移动" }
        };

        private readonly string root;
        private readonly string mappingsPath;
        private readonly string configPath;
        private readonly string panelLogPath;
        private readonly Dictionary<string, string> overrides = new Dictionary<string, string>();
        private readonly List<MappingRow> mappings = new List<MappingRow>();
        private readonly DataGridView grid = new DataGridView();
        private readonly TextBox search = new TextBox();
        private readonly Label status = new Label();
        private readonly Button changeButton = new Button();
        private readonly Button disableButton = new Button();
        private readonly Button resetButton = new Button();
        private readonly Button resetAllButton = new Button();
        private DateTime mappingsStamp = DateTime.MinValue;
        private MappingRow capturing;

        public MainForm(string rootDirectory)
        {
            root = rootDirectory;
            mappingsPath = Path.Combine(root, "mappings.tsv");
            configPath = Path.Combine(root, "config.tsv");
            panelLogPath = Path.Combine(root, "panel.log");

            Text = "PalKeybindManager - 帕鲁键位管理器";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(980, 680);
            MinimumSize = new Size(720, 440);
            TopMost = false;
            ShowInTaskbar = true;
            KeyPreview = true;
            BackColor = Color.FromArgb(28, 30, 34);
            ForeColor = Color.WhiteSmoke;
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            BuildUi();
            LoadConfig();
            ReloadMappings(true);

            FormClosing += OnFormClosing;
            KeyDown += OnPanelKeyDown;
            MouseDown += OnPanelMouseDown;
            MouseWheel += OnPanelMouseWheel;
        }

        private void BuildUi()
        {
            var toolbar = new FlowLayoutPanel();
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 42;
            toolbar.Padding = new Padding(8, 7, 8, 4);
            toolbar.WrapContents = false;
            toolbar.BackColor = Color.FromArgb(40, 43, 48);

            var searchLabel = new Label();
            searchLabel.Text = "搜索：";
            searchLabel.AutoSize = true;
            searchLabel.Margin = new Padding(0, 6, 2, 0);
            toolbar.Controls.Add(searchLabel);

            search.Width = 205;
            search.Margin = new Padding(0, 2, 10, 0);
            search.TextChanged += delegate { ApplyFilter(); };
            toolbar.Controls.Add(search);

            ConfigureButton(changeButton, "更改键位", StartCapture);
            ConfigureButton(disableButton, "禁用", DisableSelected);
            ConfigureButton(resetButton, "恢复原始", ResetSelected);
            ConfigureButton(resetAllButton, "全部恢复", ResetAll);
            toolbar.Controls.Add(changeButton);
            toolbar.Controls.Add(disableButton);
            toolbar.Controls.Add(resetButton);
            toolbar.Controls.Add(resetAllButton);

            grid.Dock = DockStyle.Fill;
            grid.BackgroundColor = Color.FromArgb(24, 26, 30);
            grid.BorderStyle = BorderStyle.None;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.RowHeadersVisible = false;
            grid.AutoGenerateColumns = false;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 56, 62);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.DefaultCellStyle.BackColor = Color.FromArgb(31, 34, 39);
            grid.DefaultCellStyle.ForeColor = Color.Gainsboro;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(48, 93, 145);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.CellDoubleClick += delegate { StartCapture(); };
            grid.MouseDown += OnPanelMouseDown;
            grid.MouseWheel += OnPanelMouseWheel;

            AddColumn("类型", 68);
            AddColumn("中文操作", 230);
            AddColumn("内部名称", 220);
            AddColumn("原始键位", 130);
            AddColumn("当前键位", 130);
            AddColumn("修饰/方向", 105);
            AddColumn("状态", 82);

            status.Dock = DockStyle.Bottom;
            status.Height = 34;
            status.Padding = new Padding(9, 7, 5, 0);
            status.BackColor = Color.FromArgb(40, 43, 48);
            status.ForeColor = Color.LightSteelBlue;
            status.Text = "请在游戏退出后编辑。修改保存到 Mod，下次从 Steam 启动游戏时生效。";

            Controls.Add(grid);
            Controls.Add(status);
            Controls.Add(toolbar);
        }

        private void ConfigureButton(Button button, string text, Action action)
        {
            button.Text = text;
            button.AutoSize = true;
            button.Height = 27;
            button.Margin = new Padding(3, 0, 3, 0);
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.FromArgb(62, 67, 74);
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderColor = Color.FromArgb(88, 93, 100);
            button.Click += delegate { action(); };
        }

        private void AddColumn(string title, int width)
        {
            var column = new DataGridViewTextBoxColumn();
            column.HeaderText = title;
            column.Width = width;
            if (title == "中文操作") column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.Columns.Add(column);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            CancelCapture(null);
        }

        private void LoadConfig()
        {
            overrides.Clear();
            if (!File.Exists(configPath)) return;
            foreach (string raw in File.ReadAllLines(configPath, Encoding.UTF8))
            {
                string line = raw.TrimEnd('\r', '\n');
                if (line.Length == 0 || line.StartsWith("#")) continue;
                string[] parts = line.Split('\t');
                if (parts.Length >= 2) overrides[parts[0]] = parts[1];
            }
        }

        private void SaveConfig()
        {
            var lines = new List<string>();
            lines.Add("# PalKeybindManager overrides v1");
            lines.Add("# saved_utc=" + DateTime.UtcNow.ToString("o"));
            lines.Add("# id<TAB>key; __DISABLED__ means unbound");
            foreach (var pair in overrides.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
                lines.Add(pair.Key + "\t" + pair.Value);
            File.WriteAllLines(configPath, lines.ToArray(), new UTF8Encoding(false));
        }

        private void ReloadMappings(bool force)
        {
            if (!File.Exists(mappingsPath))
            {
                if (force) status.Text = "等待 UE4SS 生成键位列表……进入标题界面后约 5 秒完成。";
                return;
            }
            DateTime stamp = File.GetLastWriteTimeUtc(mappingsPath);
            if (!force && stamp == mappingsStamp) return;
            mappingsStamp = stamp;
            LoadConfig();

            string selectedId = SelectedMapping() == null ? null : SelectedMapping().Id;
            mappings.Clear();
            foreach (string raw in File.ReadAllLines(mappingsPath, Encoding.UTF8))
            {
                if (raw.Length == 0 || raw.StartsWith("#")) continue;
                string[] p = raw.Split('\t');
                if (p.Length < 10) continue;
                int index;
                if (!Int32.TryParse(p[2], out index)) continue;
                var row = new MappingRow();
                row.Id = p[0];
                row.Kind = p[1];
                row.Index = index;
                row.Name = p[3];
                row.ChineseName = ChineseNameFor(row.Name);
                row.OriginalKey = p[4];
                row.CurrentKey = p[5];
                row.Scale = p[6];
                row.Modifiers = p[7];
                row.Editable = p[8] == "1";
                row.Overridden = p[9] == "1";
                string savedKey;
                if (overrides.TryGetValue(row.Id, out savedKey))
                {
                    row.CurrentKey = savedKey == "__DISABLED__" ? "None" : savedKey;
                    row.Overridden = true;
                }
                else
                {
                    row.CurrentKey = row.OriginalKey;
                    row.Overridden = false;
                }
                if (IsPcRow(row)) mappings.Add(row);
            }
            PopulateGrid(selectedId);
            status.Text = String.Format("已读取 {0} 条键盘/鼠标映射；修改会自动保存，重启游戏后安全应用。", mappings.Count);
        }

        private static bool IsPcRow(MappingRow row)
        {
            string key = row.OriginalKey ?? "";
            string[] excluded = { "Gamepad_", "Vive_", "Oculus", "MixedReality_", "Daydream_", "ValveIndex_", "MagicLeap_", "MotionController_", "Touch", "PS5_", "Steam_", "Virtual_" };
            foreach (string prefix in excluded) if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        private void PopulateGrid(string selectedId)
        {
            grid.Rows.Clear();
            foreach (MappingRow mapping in mappings)
            {
                string type = mapping.Kind == "A" ? "按键" : "方向轴";
                string extra = mapping.Kind == "A" ? mapping.Modifiers : AxisDirection(mapping.Scale);
                string state = !mapping.Editable ? "只读" : (mapping.CurrentKey == "None" ? "已禁用" : (mapping.Overridden ? "已覆盖" : "原始"));
                int rowIndex = grid.Rows.Add(type, mapping.ChineseName, mapping.Name, DisplayKey(mapping.OriginalKey), DisplayKey(mapping.CurrentKey), extra, state);
                DataGridViewRow row = grid.Rows[rowIndex];
                row.Tag = mapping;
                if (!mapping.Editable) row.DefaultCellStyle.ForeColor = Color.Gray;
                if (mapping.Overridden) row.DefaultCellStyle.ForeColor = Color.LightGoldenrodYellow;
                if (mapping.Id == selectedId) row.Selected = true;
            }
            ApplyFilter();
        }

        private static string AxisDirection(string scale)
        {
            double value;
            if (!Double.TryParse(scale, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value)) return scale;
            return value < 0 ? "负方向 " + scale : "正方向 " + scale;
        }

        private static string ChineseNameFor(string internalName)
        {
            string value;
            return ChineseNames.TryGetValue(internalName ?? "", out value) ? value : "未翻译操作";
        }

        private static string DisplayKey(string key)
        {
            string value;
            return ChineseKeyNames.TryGetValue(key ?? "", out value) ? value : key;
        }

        private void ApplyFilter()
        {
            string needle = search.Text.Trim();
            grid.ClearSelection();
            foreach (DataGridViewRow row in grid.Rows)
            {
                MappingRow mapping = row.Tag as MappingRow;
                bool visible = mapping != null && (needle.Length == 0 || mapping.Name.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0 || mapping.ChineseName.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0 || mapping.CurrentKey.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0);
                row.Visible = visible;
            }
        }

        private MappingRow SelectedMapping()
        {
            if (grid.SelectedRows.Count == 0) return null;
            return grid.SelectedRows[0].Tag as MappingRow;
        }

        private void StartCapture()
        {
            MappingRow row = SelectedMapping();
            if (row == null) { status.Text = "请先选择一条键位。"; return; }
            if (!row.Editable) { status.Text = "该条目是手柄、VR 或鼠标视角轴，本个人版不修改。"; return; }
            capturing = row;
            status.Text = "正在修改 “" + row.ChineseName + "（" + row.Name + "）”：请按一个键盘键、鼠标按键或滚动滚轮。Esc 取消。";
            Focus();
        }

        private void CancelCapture(string message)
        {
            capturing = null;
            if (!String.IsNullOrEmpty(message)) status.Text = message;
        }

        private void OnPanelKeyDown(object sender, KeyEventArgs e)
        {
            if (capturing == null)
            {
                if (e.KeyCode == Keys.Escape) Close();
                return;
            }
            e.Handled = true;
            e.SuppressKeyPress = true;
            if (e.KeyCode == Keys.Escape) { CancelCapture("已取消改键。"); return; }
            string key = UnrealKeyFromKeyEvent(e);
            if (String.IsNullOrEmpty(key)) { status.Text = "这个键暂不支持，请换一个常规键盘键。"; return; }
            CommitCapturedKey(key);
        }

        private void OnPanelMouseDown(object sender, MouseEventArgs e)
        {
            if (capturing == null) return;
            string key = null;
            if (e.Button == MouseButtons.Left) key = "LeftMouseButton";
            else if (e.Button == MouseButtons.Right) key = "RightMouseButton";
            else if (e.Button == MouseButtons.Middle) key = "MiddleMouseButton";
            else if (e.Button == MouseButtons.XButton1) key = "ThumbMouseButton";
            else if (e.Button == MouseButtons.XButton2) key = "ThumbMouseButton2";
            if (key != null) CommitCapturedKey(key);
        }

        private void OnPanelMouseWheel(object sender, MouseEventArgs e)
        {
            if (capturing == null) return;
            CommitCapturedKey(e.Delta > 0 ? "MouseScrollUp" : "MouseScrollDown");
        }

        private void CommitCapturedKey(string key)
        {
            if (capturing == null) return;
            if (String.Equals(capturing.Name, "Jump", StringComparison.OrdinalIgnoreCase) &&
                (key == "MouseScrollUp" || key == "MouseScrollDown"))
            {
                status.Text = "Palworld 当前不会可靠响应滚轮跳跃，请为 Jump 选择空格或其他键盘/鼠标按键。";
                MessageBox.Show(this, "已经实机确认：即使 Action Mapping 修改成功并释放其他滚轮绑定，Palworld 的 Jump 仍不会响应 MouseScrollUp/MouseScrollDown。\n\n为避免保存无效配置，本面板已阻止该组合。", "不支持滚轮跳跃", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            List<MappingRow> conflicts = mappings.Where(m => m.Id != capturing.Id && m.Editable && String.Equals(m.CurrentKey, key, StringComparison.OrdinalIgnoreCase)).ToList();
            if (conflicts.Count > 0)
            {
                string names = String.Join("、", conflicts.Take(8).Select(m => m.ChineseName + "（" + m.Name + "）").ToArray());
                DialogResult result = MessageBox.Show(this, "键位 “" + DisplayKey(key) + "（" + key + "）” 已被以下操作使用：\n\n" + names + "\n\n仍要保存吗？", "键位冲突", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes) { status.Text = "保留原键位，请重新选择。"; return; }
            }
            overrides[capturing.Id] = key;
            capturing.CurrentKey = key;
            capturing.Overridden = true;
            string name = capturing.ChineseName + "（" + capturing.Name + "）";
            SaveConfig();
            CancelCapture("已保存 “" + name + "” → " + DisplayKey(key) + "；请重启游戏后使用。游玩中不会强制重建键位。");
            PopulateGrid(capturing == null ? null : capturing.Id);
        }

        private void DisableSelected()
        {
            MappingRow row = SelectedMapping();
            if (row == null || !row.Editable) { status.Text = "请选择一条可编辑键位。"; return; }
            DialogResult result = MessageBox.Show(this, "确定禁用 “" + row.ChineseName + "（" + row.Name + "）” 的这一条映射吗？", "禁用键位", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;
            overrides[row.Id] = "__DISABLED__";
            row.CurrentKey = "None";
            row.Overridden = true;
            SaveConfig();
            PopulateGrid(row.Id);
            status.Text = "已保存禁用设置；重启游戏后生效。";
        }

        private void ResetSelected()
        {
            MappingRow row = SelectedMapping();
            if (row == null || !row.Editable) { status.Text = "请选择一条可编辑键位。"; return; }
            overrides.Remove(row.Id);
            row.CurrentKey = row.OriginalKey;
            row.Overridden = false;
            SaveConfig();
            PopulateGrid(row.Id);
            status.Text = "已保存 “" + row.ChineseName + "” 的恢复设置；重启游戏后生效。";
        }

        private void ResetAll()
        {
            if (MessageBox.Show(this, "确定清除本 Mod 保存的全部键位覆盖吗？\n游戏原始键位将在约 1 秒后恢复。", "全部恢复", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            overrides.Clear();
            SaveConfig();
            foreach (MappingRow row in mappings) { row.CurrentKey = row.OriginalKey; row.Overridden = false; }
            PopulateGrid(null);
            status.Text = "已清除全部 Mod 键位覆盖；重启游戏后恢复。";
        }

        private static string UnrealKeyFromKeyEvent(KeyEventArgs e)
        {
            int vk = e.KeyValue;
            if (vk == 160) return "LeftShift";
            if (vk == 161) return "RightShift";
            if (vk == 162) return "LeftControl";
            if (vk == 163) return "RightControl";
            if (vk == 164) return "LeftAlt";
            if (vk == 165) return "RightAlt";
            if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z) return e.KeyCode.ToString();
            if (e.KeyCode >= Keys.F1 && e.KeyCode <= Keys.F24) return e.KeyCode.ToString();
            string[] digits = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) return digits[e.KeyCode - Keys.D0];
            if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9) return "NumPad" + digits[e.KeyCode - Keys.NumPad0];
            var map = new Dictionary<Keys, string>
            {
                { Keys.Space, "SpaceBar" }, { Keys.Tab, "Tab" }, { Keys.Back, "BackSpace" }, { Keys.Enter, "Enter" },
                { Keys.Insert, "Insert" }, { Keys.Delete, "Delete" }, { Keys.Home, "Home" }, { Keys.End, "End" },
                { Keys.PageUp, "PageUp" }, { Keys.PageDown, "PageDown" }, { Keys.Left, "Left" }, { Keys.Right, "Right" },
                { Keys.Up, "Up" }, { Keys.Down, "Down" }, { Keys.CapsLock, "CapsLock" }, { Keys.NumLock, "NumLock" },
                { Keys.Scroll, "ScrollLock" }, { Keys.PrintScreen, "PrintScreen" }, { Keys.Pause, "Pause" },
                { Keys.LShiftKey, "LeftShift" }, { Keys.RShiftKey, "RightShift" }, { Keys.ShiftKey, "LeftShift" },
                { Keys.LControlKey, "LeftControl" }, { Keys.RControlKey, "RightControl" }, { Keys.ControlKey, "LeftControl" },
                { Keys.LMenu, "LeftAlt" }, { Keys.RMenu, "RightAlt" }, { Keys.Menu, "LeftAlt" },
                { Keys.Multiply, "Multiply" }, { Keys.Add, "Add" }, { Keys.Subtract, "Subtract" }, { Keys.Decimal, "Decimal" }, { Keys.Divide, "Divide" },
                { Keys.OemSemicolon, "Semicolon" }, { Keys.Oemplus, "Equals" }, { Keys.Oemcomma, "Comma" }, { Keys.OemMinus, "Hyphen" },
                { Keys.OemPeriod, "Period" }, { Keys.OemQuestion, "Slash" }, { Keys.Oemtilde, "Tilde" }, { Keys.OemOpenBrackets, "LeftBracket" },
                { Keys.OemPipe, "Backslash" }, { Keys.OemCloseBrackets, "RightBracket" }, { Keys.OemQuotes, "Apostrophe" }
            };
            string value;
            return map.TryGetValue(e.KeyCode, out value) ? value : null;
        }

        private void Log(string message)
        {
            try { File.AppendAllText(panelLogPath, "[" + DateTime.UtcNow.ToString("o") + "] " + message + Environment.NewLine, new UTF8Encoding(false)); }
            catch { }
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            string root = args.Length > 0 ? args[0] : Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar)).FullName;
            if (Process.GetProcessesByName("Palworld-Win64-Shipping").Length > 0)
            {
                MessageBox.Show("为兼容当前 OptiScaler/帧生成环境，键位面板只能在 Palworld 完全退出后使用。\n\n请先退出游戏，再重新运行本面板。", "PalKeybindManager 安全保护", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            bool created;
            using (var mutex = new Mutex(true, "Local\\PalKeybindManagerPanel", out created))
            {
                if (!created) return;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(root));
            }
        }
    }
}

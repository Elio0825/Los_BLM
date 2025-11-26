using AEAssist.CombatRoutine;
using AEAssist.CombatRoutine.View.JobView.HotkeyResolver;
using AEAssist.Helper;
using Dalamud.Bindings.ImGui;
using Los.ModernJobViewFramework;
using Los;
using los.BLM.QtUI.Hotkey;
using los.BLM.SlotResolver.Data;
using PatchouliTC.Common;

namespace los.BLM.QtUI
{
    public static class Qt
    {
        public static JobViewWindow Instance { get; private set; }
        private static MacroManager MacroMan;

        private static Dictionary<string, bool> _currQtStatesDict =
            BlackMageSetting.Instance.QtStatesCasual;

        private static readonly List<QtInfo> _qtKeys =
        [
            new("起手", "opener", false, null, ""),
            new("通晓", "Polyglot", true, null, ""),
            new("爆发药", "Potion", false, null, ""),
            new("黑魔纹", "LeyLines", true, null, ""),
            new("魔泉", "Sharpcast", true, null, ""),
            new("Dot", "Dot", true, null, "关闭后不会使用雷系技能"),
            new("智能AOE", "SmartAOE", false, null, ""),
            new("AOE", "AOE", true, null, "开关所有 AOE"),
            new("倾泻资源", "Dump", false, null, "清空通晓"),
            new("Boss上天", "BossFly", false, null, "Boss 上天逻辑"),
            new("TTK", "TTK", false, null, ""),
            new("起手不三连", "NoTriple", false, null, "只对普通循环有效"),
            new("双星灵魔泉", "DoubleSharpcast", false, null, ""),
            new("即刻进冰", "Triplecast", true, null, "使用即刻进冰"),
            new("三连进冰", "TripleB", true, null, "使用三连进冰"),
            new("三连走位", "TripleC", true, null, "开启后将在走位时自动使用"),
            new("脱战转圈", "zhuanquan", true, null, "脱战5秒自动重置为冰 随开随关 不然概率卡一个G")
        ];
        private static readonly bool _forceNextSlots = BlackMageSetting.Instance.ForceNextSlotsOnHKs;

        private static readonly List<HotKeyInfo> _hkResolvers =
        [
            new("LB",   "LB",     new HotKeyResolver_法系LB()),
            new("爆发药", "Pot",   new HotKeyResolver_Potion()),
            new("疾跑", "Sprint", new HotKeyResolver_疾跑()),
            new("魔罩", "Manaward",
                new HotKeyResolver_NormalSpell(Skill.魔罩, SpellTargetType.Self, _forceNextSlots)
            ),
            new("昏乱", "Addle",
                new HotKeyResolver_NormalSpell(Skill.昏乱, SpellTargetType.Target, _forceNextSlots)
            ),
            new("沉稳咏唱", "Surecast",
                new HotKeyResolver_NormalSpell(Skill.沉稳, SpellTargetType.Self, _forceNextSlots)
            ),
            new("黑魔纹", "LeyLines",
                new HotKeyResolver_NormalSpell(Skill.黑魔纹, SpellTargetType.Self, _forceNextSlots)
            ),
            new("三连咏唱", "Triplecast",
                new HotKeyResolver_NormalSpell(Skill.三连, SpellTargetType.Self, _forceNextSlots)
            ),
            new("即刻咏唱", "Swiftcast",
                new HotKeyResolver_NormalSpell(Skill.即刻, SpellTargetType.Self, _forceNextSlots)
            ),
            new("以太步(鼠标)", "AetherMo", new AetherStepMouseHotkey()),
        ];

        public static void Build()
        {
            Instance = new JobViewWindow(
                BlackMageSetting.Instance.JobViewSave,
                BlackMageSetting.Instance.Save,
                "Los"
            );

            Instance.SetUpdateAction(OnUIUpdate);
            ReadmeTab.Build(Instance);

            SettingTab.Build(Instance);
            ChangelogTab.Build(Instance);

            MacroMan = new MacroManager(Instance, "/blm", _qtKeys, _hkResolvers, true);

            MacroMan.Init();
            Qt.LoadQtStates(); 
        }

        private static void OnUIUpdate()
        {
            _currQtStatesDict = BlackMageSetting.Instance.IsHardCoreMode
                ? BlackMageSetting.Instance.QtStatesHardCore
                : BlackMageSetting.Instance.QtStatesCasual;

            AetherStepHotkeyWindow.Update();
        }

        public static void SaveQtStates()
        {
            foreach (string key in Instance.GetQtArray())
                _currQtStatesDict[key] = Instance.GetQt(key);

            BlackMageSetting.Instance.Save();
            LogHelper.Print(" QT 已保存");
        }

        public static void LoadQtStates()
        {
            foreach (var kv in _currQtStatesDict)
                Instance.SetQt(kv.Key, kv.Value);

            if (BlackMageSetting.Instance.Debug)
                LogHelper.Print("BlackMage QT 已加载");
        }
    }

    public static class BlackMageQT
    {
        public static bool GetQt(string name)
            => Qt.Instance.GetQt(name);

        public static void SetQt(string name, bool value)
            => Qt.Instance.SetQt(name, value);

        public static bool ReverseQt(string name)
        {
            bool v = Qt.Instance.GetQt(name);
            Qt.Instance.SetQt(name, !v);
            return !v;
        }

        public static string[] GetQtArray()
            => Qt.Instance.GetQtArray();

        public static void Reset()
        {
            Qt.Instance.SetQt("opener", true);
            Qt.Instance.SetQt("LeyLines", true);
            Qt.Instance.SetQt("Sharpcast", true);
            Qt.Instance.SetQt("Dot", true);
            Qt.Instance.SetQt("AOE", true);
            Qt.Instance.SetQt("Polyglot", true);
            Qt.Instance.SetQt("SmartAOE", false);
            Qt.Instance.SetQt("Dump", false);
            Qt.Instance.SetQt("BossFly", false);
            Qt.Instance.SetQt("TTK", false);
            Qt.Instance.SetQt("NoTriple", false);
            Qt.Instance.SetQt("DoubleSharpcast", false);
            Qt.Instance.SetQt("Triplecast", true);
            Qt.Instance.SetQt("TripleB", true);
            Qt.Instance.SetQt("TripleC", true);
            Qt.Instance.SetQt("zhuanquan", true);
        }

        public static void NewDefault(string name, bool value)
        {
        }

        public static void SetDefaultFromNow()
        {
        }
    }
}
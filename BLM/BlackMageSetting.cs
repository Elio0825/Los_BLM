using AEAssist.Helper;
using AEAssist.IO;
using Los.ModernJobViewFramework;

namespace los.BLM
{
    public class BlackMageSetting
    {
        public static BlackMageSetting Instance;

        private static string _path;

       
        public static void Build(string settingPath)
        {
            _path = Path.Combine(settingPath, $"{nameof(BlackMageSetting)}.json");

            if (!File.Exists(_path))
            {
                Instance = new BlackMageSetting();
                Instance.Save();
                return;
            }

            try
            {
                Instance = JsonHelper.FromJson<BlackMageSetting>(File.ReadAllText(_path));
            }
            catch (Exception e)
            {
                Instance = new BlackMageSetting();
                LogHelper.Error(e.ToString());
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonHelper.ToJson(this));
        }

        
        /// 动画锁模式：0 = 未开启减少动画锁，1 = 开启减少动画锁
        /// 只用于 ACR 内部判断能否在普通GCD 上单插能力技，与 AnimLock 数值本身无关
        public int 动画锁模式 = 0;
        public bool ForceCast = false;
        public bool ForceNextSlotsOnHKs = false;

        public bool NoPosDrawInTN = false;
        public int PosDrawStyle = 2;

        public bool RestoreQtSet = true;
        public bool AutoSetCasual = true;
        public bool IsHardCoreMode = false;

        public bool CommandWindowOpen = true;
        public bool ShowToast = false;
        public bool Debug = false;
        public bool TimelineDebug = false;
        
        public bool 调试窗口
        {
            get => Debug;
            set => Debug = value;
        }

        public bool 锁定以太步窗口 = false;
        public int 以太步IconSize = 47;
        public bool 以太步窗口显示 = true;
        
        public bool TimeLinesDebug
        {
            get => TimelineDebug;
            set => TimelineDebug = value;
        }

        
        public double 起手预读时间 = 3.5;
        public bool 核爆起手 = false;
        public bool 标准57 = false;
        public bool 开挂循环 = false;
        public int 起手选择 = 0;
        public bool 提前黑魔纹 = false;
        public Dictionary<string, bool> QtStatesHardCore;
        public Dictionary<string, bool> QtStatesCasual; 
        public bool 压缩冰悖论 { get; set; } = true;

        public bool 压缩火悖论 { get; set; } = true;
        //以太步面板
        public bool ShowAetherStepWindow = true;

        // 以太步面板尺寸
        public float AetherStepIconSize = 36f;

       
        private BlackMageSetting()
        {
            ResetQtStates(true);
            ResetQtStates(false);
        }

        
        public void ResetQtStates(bool isHardCoreMode)
        {
            if (isHardCoreMode)
            {
                QtStatesHardCore = new Dictionary<string, bool>
                {
                    ["起手"] = true,
                    ["通晓"] = true,
                    ["爆发药"] = false,
                    ["黑魔纹"] = false,
                    ["魔泉"] = true,
                    ["Dot"] = true,
                    ["智能AOE"] = false,
                    ["AOE"] = true,
                    ["倾泻资源"] = false,
                    ["Boss上天"] = false,
                    ["TTK"] = false,
                    ["起手不三连"] = false,
                    ["即刻进冰"] = false,
                    ["三连走位"] = false,
                    ["三连进冰"] = false,
                    ["脱战转圈"] = false,
                };
            }
            else
            {
                QtStatesCasual = new Dictionary<string, bool>
                {
                    ["起手"] = false,
                    ["通晓系技能"] = true,
                    ["爆发药"] = false,
                    ["黑魔纹"] = true,
                    ["魔泉"] = true,
                    ["Dot"] = true,
                    ["智能AOE"] = true,
                    ["AOE"] = true,
                    ["倾泻资源"] = false,
                    ["Boss上天"] = false,
                    ["TTK"] = false,
                    ["起手不三连"] = false,
                    ["即刻进冰"] = true,
                    ["三连走位"] = true,
                    ["三连进冰"] = true,
                    ["脱战转圈"] = true,
                };
            }
        }

        
        public JobViewSave JobViewSave { get; set; } = new()
        {
            
            ShowHotkey = true,
            CurrentTheme = ModernTheme.ThemePreset.BLM,
            QtLineCount = 3,
            QtUnVisibleList =
            [
                
            ],
        };
    }
}
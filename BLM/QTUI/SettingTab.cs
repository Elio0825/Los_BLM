using System.Numerics;
using Dalamud.Bindings.ImGui;
using Los;
using Los.ModernJobViewFramework;
using los.BLM;

namespace los.BLM.QtUI;

public static class SettingTab
{
    // 主题颜色
    private static readonly Vector4 ColorPrimary = new(0.4f, 0.7f, 1.0f, 1.0f);      // 主色调 - 蓝色
    private static readonly Vector4 ColorSuccess = new(0.3f, 0.8f, 0.4f, 1.0f);      // 成功色 - 绿色
    private static readonly Vector4 ColorWarning = new(1.0f, 0.7f, 0.2f, 1.0f);      // 警告色 - 橙色
    private static readonly Vector4 ColorDanger = new(0.9f, 0.3f, 0.3f, 1.0f);       // 危险色 - 红色
    private static readonly Vector4 ColorInfo = new(0.5f, 0.8f, 0.9f, 1.0f);         // 信息色 - 青色
    private static readonly Vector4 ColorTextDim = new(0.6f, 0.6f, 0.6f, 1.0f);      // 次要文字
    private static readonly Vector4 ColorSectionBg = new(0.15f, 0.15f, 0.18f, 0.5f); // 分组背景
    
    public static void Build(JobViewWindow instance)
    {
        instance.AddTab("⚙ 设置", _ => Draw());
    }

    private static void Draw()
    {
        // 使用滚动区域包裹整个内容
        ImGui.BeginChild("##SettingsScrollRegion", new Vector2(0, 0), false);
        
        DrawHeader();
        ImGui.Spacing();
        
        // 模式&QT设置永远置顶，无需下拉栏
        DrawModeAndQtSection();
        
        // 其他设置项
        DrawAnimLockSection();
        DrawOpenerSection();
        DrawLoopSection();
        DrawAetherStepSection();
        DrawSmallWindowSection();
        DrawDpiScalingSection();
        
        ImGui.Spacing();
        ImGui.EndChild();
    }
    
    /// <summary>
    /// 绘制标题
    /// </summary>
    private static void DrawHeader()
    {
        // 使用稍大的缩放来模拟大字体效果
        var originalScale = ImGui.GetFont().Scale;
        ImGui.GetFont().Scale = 1.3f;
        ImGui.TextColored(ColorPrimary, "🔮 黑魔法师 设置面板");
        ImGui.GetFont().Scale = originalScale;
        
        ImGui.PushStyleColor(ImGuiCol.Separator, ColorPrimary);
        ImGui.Separator();
        ImGui.PopStyleColor();
        
        ImGui.Spacing();
    }
    
    /// <summary>
    /// 绘制分组容器
    /// </summary>
    private static void BeginSection()
    {
        ImGui.PushStyleColor(ImGuiCol.ChildBg, ColorSectionBg);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 8f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1f);
        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.3f, 0.3f, 0.35f, 0.5f));
    }
    
    private static void EndSection()
    {
        ImGui.PopStyleColor(2);
        ImGui.PopStyleVar(2);
    }
    
    /// <summary>
    /// 绘制分组标题
    /// </summary>
    private static void DrawSectionTitle(string icon, string title, Vector4 color)
    {
        var originalScale = ImGui.GetFont().Scale;
        ImGui.GetFont().Scale = 1.15f;
        ImGui.TextColored(color, $"{icon} {title}");
        ImGui.GetFont().Scale = originalScale;
        ImGui.Spacing();
    }
    
    /// <summary>
    /// 绘制提示文本
    /// </summary>
    private static void DrawHelpText(string text)
    {
        ImGui.PushTextWrapPos(ImGui.GetContentRegionAvail().X);
        ImGui.TextColored(ColorTextDim, text);
        ImGui.PopTextWrapPos();
    }
    
    /// <summary>
    /// 绘制帮助标记（?）
    /// </summary>
    private static void DrawHelpMarker(string description)
    {
        ImGui.TextColored(ColorInfo, "(?)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(description);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }
    
    /// <summary>
    /// 小窗口显示设置
    /// </summary>
    private static void DrawSmallWindowSection()
    {
        if (!ImGui.CollapsingHeader("📱 小窗口设置"))
            return;
        
        BeginSection();
        ImGui.Indent(10);
        
        if (GlobalSetting.Instance != null)
        {
            bool showInfo = GlobalSetting.Instance.小窗显示时间轴和职能;
            if (ImGui.Checkbox("##SmallWindowShowInfo", ref showInfo))
            {
                GlobalSetting.Instance.小窗显示时间轴和职能 = showInfo;
                GlobalSetting.Instance.Save();
            }
            
            ImGui.SameLine();
            ImGui.Text("小窗模式显示时间轴和职能设置");
            
            ImGui.SameLine();
            DrawHelpMarker("控制窗口收起时是否显示额外信息\n\n关闭后，收起窗口时将不显示\"当前时间轴\"和\"职能设置\"信息");
            }

        ImGui.Unindent(10);
        EndSection();
    }
    
    /// <summary>
    /// DPI 缩放信息显示
    /// </summary>
    private static void DrawDpiScalingSection()
    {
        if (!ImGui.CollapsingHeader("🖥️ 屏幕适配信息"))
            return;

        BeginSection();
        ImGui.Indent(10);

        var displaySize = ImGui.GetIO().DisplaySize;
        var scale = DpiScaling.GetSmartScale();
        
        // 分辨率信息
        ImGui.TextColored(ColorPrimary, "📐 当前分辨率：");
        ImGui.SameLine();
        ImGui.Text($"{(int)displaySize.X} × {(int)displaySize.Y}");
        
        ImGui.TextColored(ColorSuccess, "🔍 自动缩放：");
        ImGui.SameLine();
        ImGui.Text($"{scale:P0} ({scale:F2}×)");
        
        ImGui.SameLine();
        DrawHelpMarker("插件已根据您的屏幕分辨率自动调整UI缩放：\n\n支持的分辨率：\n• 1080p (1920×1080): 100%\n• 2K/1440p (2560×1440): 133%\n• 4K (3840×2160): 200%\n\n如果UI过大或过小，请在游戏设置中调整「UI 缩放」");
        
        ImGui.Spacing();
        
        // 刷新按钮
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.5f, 0.8f, 0.8f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.6f, 0.9f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.1f, 0.4f, 0.7f, 1.0f));
        
        if (ImGui.Button("🔄 刷新缩放检测", new Vector2(140, 26)))
        {
            DpiScaling.RefreshScale();
        }
        
        ImGui.PopStyleColor(3);
        
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("切换显示器或修改分辨率后点击此按钮重新检测");
    }

        ImGui.Unindent(10);
        EndSection();
    }

  
    /// <summary>
    /// 模式和QT设置 - 永远置顶，只显示三个按钮，无需标题栏
    /// </summary>
    private static void DrawModeAndQtSection()
    {
        var setting = BlackMageSetting.Instance;
        bool isHard = setting.IsHardCoreMode;

        // 直接显示三个按钮，无需卡片容器和标题
        // 日随模式按钮 - 增大尺寸
        ImGui.PushStyleColor(ImGuiCol.Button, isHard ? new Vector4(0.2f, 0.2f, 0.25f, 0.8f) : ColorSuccess);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, isHard ? new Vector4(0.25f, 0.25f, 0.3f, 1.0f) : new Vector4(0.4f, 0.9f, 0.5f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6f);
        
        if (ImGui.Button(isHard ? "  🌟 日随  " : "✓ 🌟 日随", new Vector2(160, 36)))
        {
            setting.IsHardCoreMode = false;
            setting.Save();
            ApplyQtFromSettingToUi();
        }
        ImGui.PopStyleVar();
        ImGui.PopStyleColor(2);
        
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextColored(ColorSuccess, "🌟 日随模式");
            ImGui.TextWrapped("适用于日常副本和随机任务");
            ImGui.EndTooltip();
        }

        ImGui.SameLine();
        ImGui.Spacing();
        ImGui.SameLine();

        // 高难模式按钮 - 增大尺寸
        ImGui.PushStyleColor(ImGuiCol.Button, !isHard ? new Vector4(0.2f, 0.2f, 0.25f, 0.8f) : ColorDanger);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, !isHard ? new Vector4(0.25f, 0.25f, 0.3f, 1.0f) : new Vector4(1.0f, 0.4f, 0.4f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6f);
        
        if (ImGui.Button(!isHard ? "  🔥 高难  " : "✓ 🔥 高难", new Vector2(160, 36)))
        {
            setting.IsHardCoreMode = true;
            setting.Save();
            ApplyQtFromSettingToUi();
        }
        ImGui.PopStyleVar();
        ImGui.PopStyleColor(2);
        
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextColored(ColorDanger, "🔥 高难模式");
            ImGui.TextWrapped("适用于高难度副本和团队战斗");
            ImGui.EndTooltip();
        }

        ImGui.SameLine();
        DrawHelpMarker("每种模式会保存独立的QT配置");

        ImGui.Spacing();

        // 重置按钮
        ImGui.PushStyleColor(ImGuiCol.Button, ColorWarning with { W = 0.6f });
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ColorWarning);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ColorWarning with { W = 0.8f });
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4f);
        
        if (ImGui.Button("🔄 重置当前模式的QT开关", new Vector2(-1, 32)))
        {
            setting.ResetQtStates(setting.IsHardCoreMode);
            setting.Save();
            ApplyQtFromSettingToUi();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(3);
        
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextColored(ColorWarning, "⚠️ 注意");
            ImGui.TextWrapped("将当前模式的所有QT开关恢复为作者预设配置");
            ImGui.EndTooltip();
        }

        ImGui.Spacing();
    }
    
    private static void ApplyQtFromSettingToUi()
    {
        var setting = BlackMageSetting.Instance;

        
        var src = setting.IsHardCoreMode
            ? setting.QtStatesHardCore
            : setting.QtStatesCasual;

        
        if (Qt.Instance == null || src == null)
            return;

        foreach (var kv in src)
        {
            
            Qt.Instance.SetQt(kv.Key, kv.Value);
        }
    }
    /// <summary>
    /// 以太步窗口设置
    /// </summary>
    private static void DrawAetherStepSection()
    {
        var setting = BlackMageSetting.Instance;

        if (!ImGui.CollapsingHeader("✨ 以太步窗口"))
            return;

        BeginSection();
        ImGui.Indent(10);
        
        // 开关
        bool enable = setting.ShowAetherStepWindow;
        
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.2f, 0.2f, 0.25f, 0.8f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.3f, 0.3f, 0.35f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.CheckMark, ColorSuccess);
        
        if (ImGui.Checkbox("##AetherStepEnable", ref enable))
        {
            setting.ShowAetherStepWindow = enable;
            setting.Save();
        }

        ImGui.PopStyleColor(3);
        
        ImGui.SameLine();
        ImGui.Text("显示以太步窗口");
        
        ImGui.SameLine();
        DrawHelpMarker("开启后会显示以太步技能的可视化面板，方便快速使用以太步技能");

        ImGui.Spacing();
        
        // 图标大小滑块
        ImGui.Text("图标大小：");
        ImGui.SameLine();
       
        float size = setting.AetherStepIconSize;
        
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.2f, 0.2f, 0.25f, 0.8f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.3f, 0.3f, 0.35f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.SliderGrab, ColorPrimary);
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, ColorPrimary with { W = 1.0f });
        ImGui.PushStyleVar(ImGuiStyleVar.GrabMinSize, 15f);
        
        ImGui.SetNextItemWidth(200);
        if (ImGui.SliderFloat("##AetherStepIconSize", ref size, 24f, 64f, "%.0f px"))
        {
            setting.AetherStepIconSize = size;
            setting.Save();
        }
        
        ImGui.PopStyleVar();
        ImGui.PopStyleColor(4);

        ImGui.Unindent(10);
        EndSection();
        }

    /// <summary>
    /// 动画锁设置
    /// </summary>
    private static void DrawAnimLockSection()
    {
        // 默认不展开
        if (!ImGui.CollapsingHeader("⚡ 动画锁设置"))
            return;

        var setting = BlackMageSetting.Instance;
        int mode = setting.动画锁模式;

        BeginSection();
        ImGui.Indent(10);

        string label = mode switch
        {
            0 => "❌ 未开启减少动画锁",
            1 => "✓ 已开启减少动画锁",
            _ => "❌ 未开启减少动画锁"
        };

        ImGui.Text("动画锁模式：");
        ImGui.SameLine();
        
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.2f, 0.2f, 0.25f, 0.8f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.3f, 0.3f, 0.35f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4f);
        
        ImGui.SetNextItemWidth(200);
        if (ImGui.BeginCombo("##AnimLockMode", label))
        {
            // 未开启选项
            ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.3f, 0.3f, 0.35f, 0.8f));
            if (ImGui.Selectable("❌ 未开启减少动画锁", mode == 0))
            {
                setting.动画锁模式 = 0;
                setting.Save();
            }

            if (mode == 0 && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("标准模式：使用游戏默认的动画锁定");
            }
            
            // 开启选项
            ImGui.PushStyleColor(ImGuiCol.Header, ColorSuccess with { W = 0.8f });
            if (ImGui.Selectable("✓ 开启减少动画锁", mode == 1))
            {
                setting.动画锁模式 = 1;
                setting.Save();
            }

            if (mode == 1 && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("优化模式：需要配合第三方插件使用");
            }
            
            ImGui.PopStyleColor(2);
            ImGui.EndCombo();
        }
        
        ImGui.PopStyleVar();
        ImGui.PopStyleColor(2);
        
        ImGui.SameLine();
        DrawHelpMarker("开启动画锁优化可以提升技能释放流畅度\n\n需要安装：\n• FuckAnimationLock 三插\n• DR 能力技动画减少");

        if (setting.动画锁模式 == 1)
        {
            ImGui.Spacing();
            ImGui.TextColored(ColorWarning, "⚠️ 需要：FuckAnimationLock 三插 + DR 能力技动画减少");
        }

        ImGui.Unindent(10);
        EndSection();
    }

    /// <summary>
    /// 起手设置
    /// </summary>
    private static void DrawOpenerSection()
    {
        // 默认不展开
        if (!ImGui.CollapsingHeader("🎯 起手设置"))
            return;

        var setting = BlackMageSetting.Instance;
        int openerIndex = setting.起手选择;

        BeginSection();
        ImGui.Indent(10);

        string openerLabel = openerIndex switch
        {
            0 => "❌ 不启用起手",
            1 => "🔄 按等级自动选择",
            2 => "Lv.70 起手",
            3 => "Lv.80 起手",
            4 => "Lv.90 起手",
            5 => "Lv.100 标准5+7",
            6 => "Lv.100 核爆起手",
            7 => "Lv.100 5+7挂B",
            _ => "🔄 按等级自动选择"
        };
        
        ImGui.Text("起手方案：");
        ImGui.SameLine();
        
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.2f, 0.2f, 0.25f, 0.8f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.3f, 0.3f, 0.35f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4f);
        
        ImGui.SetNextItemWidth(250);
        if (ImGui.BeginCombo("##OpenerSelect", openerLabel))
        {
            // 不启用
            if (ImGui.Selectable("❌ 不启用起手", openerIndex == 0))
            {
                setting.起手选择 = 0;
                setting.标准57 = false;
                setting.核爆起手 = false;
                setting.开挂循环 = false;
                setting.Save();
            }

            ImGui.Separator();
            
            // 自动选择
            ImGui.PushStyleColor(ImGuiCol.Header, ColorSuccess with { W = 0.6f });
            if (ImGui.Selectable("🔄 按等级自动选择", openerIndex == 1))
            {
                setting.起手选择 = 1;
                setting.标准57 = true;
                setting.核爆起手 = false;
                setting.开挂循环 = false;
                setting.Save();
            }
            ImGui.PopStyleColor();
            
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("根据当前等级自动选择合适的起手");
            }
            
            ImGui.Separator();
            ImGui.TextDisabled("--- 等级专用起手 ---");

            // 各等级起手
            string[] openerNames = ["Lv.70 起手", "Lv.80 起手", "Lv.90 起手"];
            for (int i = 0; i < 3; i++)
            {
                if (ImGui.Selectable($"   {openerNames[i]}", openerIndex == i + 2))
            {
                    setting.起手选择 = i + 2;
                setting.Save();
            }
            }
            
            ImGui.Separator();
            ImGui.TextDisabled("--- Lv.100 起手方案 ---");

            // 100级起手
            if (ImGui.Selectable("   标准 5+7", openerIndex == 5))
            {
                setting.起手选择 = 5;
                setting.标准57 = true;
                setting.核爆起手 = false;
                setting.开挂循环 = false;
                setting.Save();
            }

            if (ImGui.Selectable("   核爆起手", openerIndex == 6))
            {
                setting.起手选择 = 6;
                setting.标准57 = false;
                setting.核爆起手 = true;
                setting.开挂循环 = false;
                setting.Save();
            }

            if (ImGui.Selectable("   5+7 挂B起手", openerIndex == 7))
            {
                setting.起手选择 = 7;
                setting.标准57 = false;
                setting.核爆起手 = false;
                setting.开挂循环 = true;
                setting.Save();
            }

            ImGui.EndCombo();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(2);
        
        ImGui.SameLine();
        DrawHelpMarker("起手说明：\n\n• 不启用起手：所有等级都不执行起手\n• 自动选择：根据等级自动匹配合适的起手\n• 等级专用：强制使用指定等级的起手\n• 100级方案：需要100级才会生效\n\n提示：你也可以在QT面板关闭「起手」开关来禁用起手");

        ImGui.Unindent(10);
        EndSection();
    }

    /// <summary>
    /// 循环设置
    /// </summary>
    private static void DrawLoopSection()
    {
        // 默认不展开
        if (!ImGui.CollapsingHeader("🔁 循环设置"))
            return;

        var setting = BlackMageSetting.Instance;

        BeginSection();
        ImGui.Indent(10);

        // 压缩冰悖论
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.2f, 0.5f, 0.8f, 0.3f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.3f, 0.6f, 0.9f, 0.5f));
        ImGui.PushStyleColor(ImGuiCol.CheckMark, new Vector4(0.4f, 0.8f, 1.0f, 1.0f));
        
        bool compressIce = setting.压缩冰悖论;
        if (ImGui.Checkbox("##CompressIce", ref compressIce))
        {
            setting.压缩冰悖论 = compressIce;
            setting.Save();
        }
        
        ImGui.PopStyleColor(3);
        
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(0.5f, 0.8f, 1.0f, 1.0f), "❄️ 压缩冰悖论");
        
        ImGui.SameLine();
        DrawHelpMarker("❄️ 冰悖论优化\n\n只在以下情况使用冰悖论：\n• 角色移动时\n• 冰澈收尾后\n\n优点：\n更高效的循环，避免浪费瞬发机会\n\n说明：\n冰阶段保留悖论，在移动或冰澈收尾时使用");

        ImGui.Spacing();

        // 压缩火悖论
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.8f, 0.3f, 0.2f, 0.3f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.9f, 0.4f, 0.3f, 0.5f));
        ImGui.PushStyleColor(ImGuiCol.CheckMark, new Vector4(1.0f, 0.5f, 0.3f, 1.0f));
        
        bool compressFire = setting.压缩火悖论;
        if (ImGui.Checkbox("##CompressFire", ref compressFire))
        {
            setting.压缩火悖论 = compressFire;
            setting.Save();
        }
        
        ImGui.PopStyleColor(3);
        
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(1.0f, 0.6f, 0.3f, 1.0f), "🔥 压缩火悖论");
        
        ImGui.SameLine();
        DrawHelpMarker("🔥 火悖论优化\n\n只在以下情况使用火悖论：\n• 角色移动时\n• 火四循环后期\n\n优点：\n最大化火阶段伤害输出\n\n说明：\n火阶段保留悖论，在移动或火四收尾时使用");
        
        ImGui.SameLine();
        ImGui.Spacing();
        ImGui.SameLine();
        ImGui.TextColored(ColorSuccess, "✨");
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("推荐：建议同时开启两个压缩选项以获得最优输出循环\n这是高水平玩家常用的优化策略");
        }

        ImGui.Unindent(10);
        EndSection();
    }
}

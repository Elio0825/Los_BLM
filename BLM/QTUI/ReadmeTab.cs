using System.Numerics;
using Dalamud.Bindings.ImGui;
using Los.ModernJobViewFramework;

namespace los.BLM.QtUI;

public static class ReadmeTab
{
    // 主题颜色
    private static readonly Vector4 ColorPrimary = new(0.4f, 0.7f, 1.0f, 1.0f);
    private static readonly Vector4 ColorSuccess = new(0.3f, 0.8f, 0.4f, 1.0f);
    private static readonly Vector4 ColorWarning = new(1.0f, 0.7f, 0.2f, 1.0f);
    private static readonly Vector4 ColorInfo = new(0.5f, 0.8f, 0.9f, 1.0f);
    private static readonly Vector4 ColorDanger = new(0.9f, 0.3f, 0.3f, 1.0f);
    private static readonly Vector4 ColorTextDim = new(0.6f, 0.6f, 0.6f, 1.0f);
    
    public static void Build(JobViewWindow instance)
    {
        instance.AddTab("📖 说明", _ => Draw());
    }

    private static void Draw()
    {
        ImGui.BeginChild("##ReadmeScrollRegion", new Vector2(0, 0), false);

        // 标题
        DrawCompactHeader();
        
        // 紧凑版内容
        DrawCompactContent();

        ImGui.EndChild();
    }
    
    /// <summary>
    /// 绘制紧凑标题
    /// </summary>
    private static void DrawCompactHeader()
    {
        var originalScale = ImGui.GetFont().Scale;
        ImGui.GetFont().Scale = 1.2f;
        ImGui.TextColored(ColorPrimary, "🔮 Los 黑魔法师 ACR 使用说明");
        ImGui.GetFont().Scale = originalScale;
        
        ImGui.PushStyleColor(ImGuiCol.Separator, ColorPrimary);
        ImGui.Separator();
        ImGui.PopStyleColor();
        ImGui.Spacing();
    }
    
    /// <summary>
    /// 绘制紧凑内容
    /// </summary>
    private static void DrawCompactContent()
    {
        // 推荐设置
        ImGui.TextColored(ColorWarning, "⚡ 推荐设置（动画锁优化）");
        ImGui.Spacing();
        ImGui.Indent(10);
        ImGui.BulletText("建议开启「减少动画锁」模式");
        ImGui.BulletText("需要安装 FuckAnimationLock 三插");
        ImGui.BulletText("需要开启 DR 能力技动画减少");
        ImGui.Unindent(10);
        
        ImGui.Spacing();
        ImGui.Spacing();
        
        // 全局设置
        ImGui.TextColored(ColorInfo, "⚙️ AEAssist 全局设置建议");
        ImGui.Spacing();
        ImGui.Indent(10);
        ImGui.BulletText("关闭「能力技不卡GCD」");
        ImGui.BulletText("设置「提前使用GCD」为 50ms");
        ImGui.Unindent(10);
        
        ImGui.Spacing();
        ImGui.Spacing();
        
        // 热键说明
        ImGui.TextColored(ColorSuccess, "⌨️ 可用宏命令");
        ImGui.Spacing();
        ImGui.Indent(10);
        ImGui.BulletText("/blm AetherMo_hk - 以太步（鼠标位置）");
        ImGui.BulletText("/blm LeyLines_hk - 黑魔纹");
        ImGui.BulletText("/blm Triplecast_hk - 三连咏唱");
        ImGui.BulletText("/blm Swiftcast_hk - 即刻咏唱");
        ImGui.Unindent(10);
        
        ImGui.Spacing();
        ImGui.Spacing();
        
        // 常见问题
        ImGui.TextColored(ColorDanger, "❓ 常见问题");
        ImGui.Spacing();
        ImGui.Indent(10);
        
        ImGui.Text("Q: 为什么ACR不释放技能？");
        ImGui.Indent(5);
        ImGui.TextColored(ColorTextDim, "A: 检查目标是否有效，确认ACR已启用，检查QT开关状态");
        ImGui.Unindent(5);
        
        ImGui.Spacing();
        
        ImGui.Text("Q: 起手序列不执行？");
        ImGui.Indent(5);
        ImGui.TextColored(ColorTextDim, "A: 确认已在设置中启用起手，检查QT面板的「起手」开关");
        ImGui.Unindent(5);
        
        ImGui.Spacing();
        
        ImGui.Text("Q: 技能释放不流畅？");
        ImGui.Indent(5);
        ImGui.TextColored(ColorTextDim, "A: 尝试开启「减少动画锁」模式，调整全局GCD提前量");
        ImGui.Unindent(5);
        
        ImGui.Spacing();
        
        ImGui.Text("Q: 如何自定义循环？");
        ImGui.Indent(5);
        ImGui.TextColored(ColorTextDim, "A: 使用QT面板的各种开关组合，调整压缩悖论等设置");
        ImGui.Unindent(5);
        
        ImGui.Unindent(10);
        
        ImGui.Spacing();
        ImGui.Spacing();
        
        // 作者信息
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.TextColored(ColorPrimary, "👤 作者");
        ImGui.SameLine();
        ImGui.Text("Los");
        ImGui.SameLine();
        ImGui.Spacing();
        ImGui.SameLine();
        ImGui.TextColored(ColorSuccess, "版本");
        ImGui.SameLine();
        ImGui.Text("支持 1-100 级");
        ImGui.Spacing();
        ImGui.TextColored(ColorTextDim, "感谢使用 Los 黑魔法师 ACR！");
    }
}
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Los.ModernJobViewFramework;

namespace los.BLM.QtUI;

public static class ChangelogTab
{
    public static void Build(JobViewWindow instance)
    {
        instance.AddTab("更新日志", _ => Draw());
    }

    private static void Draw()
    {
        ImGui.BeginChild("##ChangelogScroll", new Vector2(0, 0), false);

        ImGui.TextUnformatted("Los BLM 更新日志");
        ImGui.Separator();

        
        ImGui.TextWrapped("v1.0.1 - 2025/11/20");
        ImGui.BulletText("修复火四");

        ImGui.Separator();

        ImGui.TextWrapped("v1.0.0 - 2025/11/20");
        ImGui.BulletText("初版发布");
        ImGui.BulletText("优化群体 50–57 蓝量兜底逻辑。");

        

        ImGui.EndChild();
    }
}
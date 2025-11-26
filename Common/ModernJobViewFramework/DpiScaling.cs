using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace Los.ModernJobViewFramework;

/// <summary>
/// DPI 感知的智能缩放系统
/// 自动适配 1080p、2K、4K 等不同分辨率屏幕
/// </summary>
public static class DpiScaling
{
    /// <summary>
    /// 缓存的缩放因子
    /// </summary>
    private static float _cachedScale = 0f;
    
    /// <summary>
    /// 上次检测的显示器尺寸
    /// </summary>
    private static Vector2 _lastDisplaySize = Vector2.Zero;

    /// <summary>
    /// 获取智能缩放因子
    /// 根据屏幕分辨率自动计算最佳缩放比例
    /// </summary>
    public static float GetSmartScale()
    {
        Vector2 displaySize = ImGui.GetIO().DisplaySize;
        
        // 如果显示尺寸发生变化，重新计算缩放因子
        if (displaySize != _lastDisplaySize || _cachedScale == 0f)
        {
            _cachedScale = CalculateScaleFactor(displaySize);
            _lastDisplaySize = displaySize;
        }

        return _cachedScale;
    }

    /// <summary>
    /// 根据分辨率计算缩放因子
    /// </summary>
    private static float CalculateScaleFactor(Vector2 displaySize)
    {
        float width = displaySize.X;
        float height = displaySize.Y;

        // 基准：1080p (1920x1080) = 1.0x
        const float baseWidth = 1920f;
        const float baseHeight = 1080f;

        // 计算宽度和高度的缩放比例
        float widthScale = width / baseWidth;
        float heightScale = height / baseHeight;

        // 使用较小的一个作为基准，避免UI过大
        float rawScale = Math.Min(widthScale, heightScale);

        // 应用合理的缩放范围限制
        // 最小 0.7x (低于 1080p 的小屏幕)
        // 最大 2.5x (超过 4K 的超高分屏)
        float clampedScale = Math.Clamp(rawScale, 0.7f, 2.5f);

        // 对常见分辨率进行优化
        clampedScale = OptimizeForCommonResolutions(width, height, clampedScale);

        return clampedScale;
    }

    /// <summary>
    /// 针对常见分辨率进行优化
    /// </summary>
    private static float OptimizeForCommonResolutions(float width, float height, float scale)
    {
        // 1080p (1920x1080) - 标准缩放
        if (IsResolution(width, height, 1920, 1080))
            return 1.0f;

        // 1440p/2K (2560x1440) - 133% 缩放
        if (IsResolution(width, height, 2560, 1440))
            return 1.33f;

        // 1600p (2560x1600) - 140% 缩放
        if (IsResolution(width, height, 2560, 1600))
            return 1.4f;

        // 4K (3840x2160) - 200% 缩放
        if (IsResolution(width, height, 3840, 2160))
            return 2.0f;

        // 5K (5120x2880) - 250% 缩放
        if (IsResolution(width, height, 5120, 2880))
            return 2.5f;

        // 1366x768 (笔记本常见分辨率) - 80% 缩放
        if (IsResolution(width, height, 1366, 768))
            return 0.8f;

        // 1600x900 - 90% 缩放
        if (IsResolution(width, height, 1600, 900))
            return 0.9f;

        // 其他分辨率使用计算的缩放值
        return scale;
    }

    /// <summary>
    /// 检查是否为指定分辨率（允许小范围误差）
    /// </summary>
    private static bool IsResolution(float actualWidth, float actualHeight, float targetWidth, float targetHeight)
    {
        const float tolerance = 50f; // 允许50像素的误差
        return Math.Abs(actualWidth - targetWidth) < tolerance 
            && Math.Abs(actualHeight - targetHeight) < tolerance;
    }

    /// <summary>
    /// 获取缩放后的尺寸
    /// </summary>
    public static Vector2 ScaleSize(Vector2 size)
    {
        return size * GetSmartScale();
    }

    /// <summary>
    /// 获取缩放后的值
    /// </summary>
    public static float ScaleValue(float value)
    {
        return value * GetSmartScale();
    }

    /// <summary>
    /// 获取当前分辨率描述
    /// </summary>
    public static string GetResolutionDescription()
    {
        Vector2 displaySize = ImGui.GetIO().DisplaySize;
        float width = displaySize.X;
        float height = displaySize.Y;
        float scale = GetSmartScale();

        string resolution;
        
        if (IsResolution(width, height, 3840, 2160))
            resolution = "4K";
        else if (IsResolution(width, height, 2560, 1440))
            resolution = "2K/1440p";
        else if (IsResolution(width, height, 1920, 1080))
            resolution = "1080p";
        else
            resolution = $"{(int)width}x{(int)height}";

        return $"{resolution} (缩放: {scale:P0})";
    }

    /// <summary>
    /// 强制刷新缩放因子
    /// </summary>
    public static void RefreshScale()
    {
        _cachedScale = 0f;
        _lastDisplaySize = Vector2.Zero;
    }
}


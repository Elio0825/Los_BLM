using AEAssist.CombatRoutine;
using Los;
using los.BLM.QtUI;
using Los.BLM;


namespace los.BLM;

public class 黑魔acr : IRotationEntry
{
    public string AuthorName { get; set; } = "Los";

    public Rotation Build(string settingFolder)
    {
        BlackMageACR.Init(settingFolder);
        return BlackMageACR.Build();
    }

    public IRotationUI GetRotationUI()
    {
        return Qt.Instance;
    }

    public void OnDrawSetting()
    {
        
    }

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        Qt.Instance.Dispose();
        los.BLM.SlotResolver.BattleData.Reset();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
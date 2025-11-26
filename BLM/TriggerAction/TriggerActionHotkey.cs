using AEAssist.CombatRoutine.Trigger;
using AEAssist.GUI;
using AEAssist.Helper;
using los.BLM.QtUI;

namespace los.BLM.Triggers;

public class TriggerActionHotkey : ITriggerAction {
    public string DisplayName { get; } = "黑魔/Hotkey";
    public string Remark { get; set; } = "";

    public string Key = "";
    public bool Value;

    // 辅助数据 因为是private 所以不存档
    private int _selectIndex;
    private readonly string[] _hotkeyArray;
  
    public TriggerActionHotkey()
    {
        _hotkeyArray = Qt.Instance.GetHotkeyArray();
    }

    public bool Draw() {
        _selectIndex = Array.IndexOf(_hotkeyArray, Key);
        if (_selectIndex == -1) _selectIndex = 0;

        ImGuiHelper.LeftCombo("使用Hotkey", ref _selectIndex, _hotkeyArray);
        Key = _hotkeyArray[_selectIndex];
        return true;
    }

    public bool Handle() {
        Qt.Instance.SetHotkey(Key);
        if (BlackMageSetting.Instance.TimelineDebug) {
            LogHelper.Print("轴控", $"使用hotkey => {Key}");
        }

        return true;
    }
}

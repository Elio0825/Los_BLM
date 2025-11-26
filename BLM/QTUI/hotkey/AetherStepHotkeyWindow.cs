using System;
using System.Collections.Generic;
using System.Numerics;
using AEAssist;
using AEAssist.CombatRoutine;
using AEAssist.Helper;
using AEAssist.MemoryApi;                    
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures.TextureWraps; 
using Dalamud.Game.ClientState.Objects.Types;
using los.BLM.Helper;

namespace los.BLM.QtUI.Hotkey
{
    public static class AetherStepHotkeyWindow
    {
        
        private static readonly Dictionary<uint, string> JobAbbrById = new()
        {
            { 19, "PLD" }, // paladin
            { 20, "MNK" }, // monk
            { 21, "WAR" }, // warrior
            { 22, "DRG" }, // dragoon
            { 23, "BRD" }, // bard
            { 24, "WHM" }, // white mage
            { 25, "BLM" }, // black mage
            { 27, "SMN" }, // summoner
            { 28, "SCH" }, // scholar
            { 30, "NIN" }, // ninja
            { 31, "MCH" }, // machinist
            { 32, "DRK" }, // dark knight
            { 33, "AST" }, // astrologian
            
        };

        private enum AetherRole
        {
            Tank,
            DPS,
            Healer
        }

        
        private static readonly MemApiIcon IconApi = Core.Resolve<MemApiIcon>();

        
        private static AetherRole GetRole(IBattleChara member)
        {
            if (PartyHelper.CastableTanks.Contains(member))
                return AetherRole.Tank;

            if (PartyHelper.CastableHealers.Contains(member))
                return AetherRole.Healer;

            return AetherRole.DPS;
        }

        private static (Vector4 normal, Vector4 hover, Vector4 active) GetRoleColors(AetherRole role)
        {
            return role switch
            {
                // Tank 蓝色
                AetherRole.Tank => (
                    new Vector4(0.25f, 0.45f, 1.00f, 1.0f),
                    new Vector4(0.35f, 0.55f, 1.00f, 1.0f),
                    new Vector4(0.15f, 0.35f, 0.85f, 1.0f)
                ),
                
                // Healer 绿色
                AetherRole.Healer => (
                    new Vector4(0.20f, 0.85f, 0.35f, 1.0f),
                    new Vector4(0.30f, 0.95f, 0.45f, 1.0f),
                    new Vector4(0.10f, 0.70f, 0.25f, 1.0f)
                ),

                // DPS 红色
                _ => (
                    new Vector4(0.90f, 0.25f, 0.25f, 1.0f),
                    new Vector4(1.00f, 0.35f, 0.35f, 1.0f),
                    new Vector4(0.80f, 0.15f, 0.15f, 1.0f)
                ),
            };
        }

        
        private static void DrawJobIconPlaceholder(AetherRole role, float size)
        {
            var (normal, _, _) = GetRoleColors(role);
            ImGui.ColorButton("##jobIcon", normal,
                ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoDragDrop,
                new Vector2(size, size));
        }

        
        

        private static void DrawJobIcon(IBattleChara member, AetherRole role, float size)
        {
            try
            {
                
                var classJobRef = member.ClassJob;

                if (classJobRef.RowId == 0)
                {
                    
                    DrawJobIconPlaceholder(role, size);
                    return;
                }

                var jobId = (int)classJobRef.RowId;
                if (!Enum.IsDefined(typeof(Jobs), jobId))
                {
                    DrawJobIconPlaceholder(role, size);
                    return;
                }

                var jobEnum = (Jobs)jobId;
                uint iconId = IconApi.GetJobIconIdByJob(jobEnum);
                if (iconId == 0)
                {
                    DrawJobIconPlaceholder(role, size);
                    return;
                }
                
                if (!IconApi.GetIconTexture((int)iconId, out IDalamudTextureWrap texWrap))
                {
                    DrawJobIconPlaceholder(role, size);
                    return;
                }
                
                ImGui.Image(texWrap.Handle, new Vector2(size, size));
            }
            catch
            {
                DrawJobIconPlaceholder(role, size);
            }
        }









        public static void Update()
        {
            var setting = BlackMageSetting.Instance;
            if (!setting.ShowAetherStepWindow)
                return;

            PartyHelper.UpdateAllies();
            var party = PartyHelper.Party;
            if (party.Count <= 1)
                return;

            ImGui.Begin("以太步面板", ImGuiWindowFlags.AlwaysAutoResize);

            ImGui.Text($"队伍人数: {party.Count}");
            ImGui.Separator();
            ImGui.Text("点击按钮，对对应队友施放以太步：");
            ImGui.Spacing();

            var iconSize   = setting.AetherStepIconSize > 0 ? setting.AetherStepIconSize : 36f;
            var buttonSize = new Vector2(-1, iconSize * 0.9f);

            // 收集“除了自己以外”的队友
            var entries = new List<(int index, IBattleChara member, AetherRole role)>();
            for (int i = 0; i < party.Count && i <= 7; i++)
            {
                var member = party[i];
                if (member == Core.Me)
                    continue;

                var role = GetRole(member);
                entries.Add((i, member, role));
            }

            // 排序：T -> DPS -> H，同一类中按名字排序
            entries.Sort((a, b) =>
            {
                int Order(AetherRole r) => r switch
                {
                    AetherRole.Tank   => 0,
                    AetherRole.DPS    => 1,
                    AetherRole.Healer => 2,
                    _                 => 3
                };

                int oa = Order(a.role);
                int ob = Order(b.role);
                if (oa != ob)
                    return oa.CompareTo(ob);

                return string.Compare(a.member.Name.TextValue, b.member.Name.TextValue,
                    StringComparison.Ordinal);
            });

            // 画每一行：左“职业图标” + 右彩色按钮
            foreach (var (index, member, role) in entries)
            {
                ImGui.PushID(index);

                // 左边：职业图标（拿不到时自动退回彩色方块）
                DrawJobIcon(member, role, iconSize);
                ImGui.SameLine();

                // 右边按钮保持不变
                var (normal, hover, active) = GetRoleColors(role);
                ImGui.PushStyleColor(ImGuiCol.Button,        normal);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, hover);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive,  active);

                var label = $"以太步 → {member.Name.TextValue}";
                if (ImGui.Button(label, buttonSize))
                {
                    var hk = new AetherStepHotkey(index);
                    if (hk.Check() == 0)
                        hk.Run();
                }

                ImGui.PopStyleColor(3);
                ImGui.PopID();
            }

            ImGui.End();
        }
    }
}

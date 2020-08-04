using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Data;
using MCM.Abstractions.Settings.Base.Global;

namespace MinorClanTroopRecruitment.Settings
{
    class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => "MinorClanTroopRecruitment";
        public override string DisplayName => "Minor Clan Troop Recruitment"; //{typeof(MCMUISettings).Assembly.GetName().Version.ToString(3)}";

        [SettingPropertyDropdown("Mercenary Spawn Towns", Order = 1, RequireRestart = true, HintText = "Requires a Reload to take affect if in game. Restrict the towns that Minor Clans mercenaries can spawn in.")]
        [SettingPropertyGroup("General")]
        public DefaultDropdown<string> RecruitmentSettings { get; set; } = new DefaultDropdown<string>(new string[] { "Any Culture", "Same Culture Only", "Json Same Culture", "CustomA", "CustomB", "CustomC" }, 0);

        [SettingPropertyDropdown("Update Occurrence", Order = 2, RequireRestart = true, HintText = "Requires a Reload to take affect if in game. Determines when the available troops update, weekly or daily.")]
        [SettingPropertyGroup("General")]
        public DefaultDropdown<string> UpdateTiming { get; set; } = new DefaultDropdown<string>(new string[] { "Daily", "Weekly" }, 0);

        [SettingPropertyFloatingInteger("Recruit Cost Multiplier", 1f, 4f, "0.00", Order = 3, RequireRestart = false, HintText = "Increase the recruitment cost by this many time more than their base recruitment Cost. (base troop cost * this number)")]
        [SettingPropertyGroup("General")]
        public float RecruitCostMultiplier { get; set; } = 1f;

        [SettingPropertyInteger("Min Number of Possible Recruits", 1, 15, "0", Order = 4, RequireRestart = false, HintText = "Sets the min limit on possible available troops available for recruitment.")]
        [SettingPropertyGroup("General")]
        public int MinNumberOfTroops  { get; set; } = 1;

        [SettingPropertyInteger("Max Number of Possible Recruits", 15, 30, "0", Order = 5, RequireRestart = false, HintText = "Sets the max limit on possible available troops available for recruitment.")]
        [SettingPropertyGroup("General")]
        public int MaxNumberOfTroops { get; set; } = 15;

        [SettingPropertyFloatingInteger("Troop Multiplier", 1f, 4f, "0.00", Order = 6, RequireRestart = false, HintText = "Multiplies the number of troops calculated by the min/max by this amount. (number of Troops random between min/max * this number)")]
        [SettingPropertyGroup("General")]
        public float TroopMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("Chance For Clan Mercenaries", 0f, 1f, "#0%", Order = 7, RequireRestart = false, HintText = "Percent chance for minor clan mercenaries to spawn.")]
        [SettingPropertyGroup("General")]
        public float PossibilityOfSpawn { get; set; } = 1.00f;
    }
}

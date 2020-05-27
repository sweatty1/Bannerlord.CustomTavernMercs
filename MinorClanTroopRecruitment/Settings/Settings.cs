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

        [SettingPropertyDropdown("Mercenary Spawn Towns", Order = 1, RequireRestart = true, HintText = "Restrict the towns that Minor Clans mercenaries can spawn in. Requires a Reload to take affect.")]
        [SettingPropertyGroup("General")]
        public DefaultDropdown<string> RecruitmentSettings { get; set; } = new DefaultDropdown<string>(new string[] { "Any Culture", "Same Culture Only" }, 0); // "Regional", "Custom" future option ids for xml inputs

        [SettingPropertyFloatingInteger("Recruit Cost Multiplier", 1f, 4f, "0.00", Order = 2, RequireRestart = false, HintText = "Increase the recruitment cost by this many time more than their base recruitment Cost. (base troop cost * this number)")]
        [SettingPropertyGroup("General")]
        public float RecruitCostMultiplier { get; set; } = 1f;

        [SettingPropertyInteger("Min Number of Possible Recruits", 1, 15, "0", Order = 3, RequireRestart = false, HintText = "Sets the min limit on possible avaiable troops avaiable for recruitment.")]
        [SettingPropertyGroup("General")]
        public int MinNumberOfTroops  { get; set; } = 1;

        [SettingPropertyInteger("Max Number of Possible Recruits", 15, 30, "0", Order = 4, RequireRestart = false, HintText = "Sets the max limit on possible avaiable troops avaiable for recruitment.")]
        [SettingPropertyGroup("General")]
        public int MaxNumberOfTroops { get; set; } = 15;

        [SettingPropertyFloatingInteger("Troops Multiplier", 1f, 4f, "0.00", Order = 5, RequireRestart = false, HintText = "When troops for a town are calculated multiplies the result by this amount.")]
        [SettingPropertyGroup("General")]
        public float TroopMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("Chance For Clan Mercenaries", 0f, 1f, "#0%", Order = 6, RequireRestart = false, HintText = "The chance for clan mercs for personal player experience.")]
        [SettingPropertyGroup("General")]
        public float PossibilityOfSpawn { get; set; } = 1.00f;

        //[SettingPropertyInteger("Avaiable Range of Troops Multiplier", 1, 10, "0", Order = 5, RequireRestart = false, HintText = "Multiplies the base min/max number of possible recruits.")]
        //[SettingPropertyGroup("General")]
        //public int TroopMultiplier { get; set; } = 1;

        //[SettingPropertyBool("Troops Multiplier Affect Max Only", RequireRestart = false, Order = 6, HintText = "Makes Avaiable Range Multiplier only affect the maximun possible amount.")]
        //[SettingPropertyGroup("General")]
        //public bool AffectOnlyMax { get; set; } = true;
    }
}

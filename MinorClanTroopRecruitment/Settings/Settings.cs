using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Data;
using MCM.Abstractions.Settings.Base.Global;
using System.Xml.Serialization;

namespace MinorClanTroopRecruitment.Settings
{
    class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => "MinorClanTroopRecruitment";
        public override string DisplayName => "Minor Clan Troop Recruitment"; //{typeof(MCMUISettings).Assembly.GetName().Version.ToString(3)}";

        [SettingPropertyDropdown("Mercenary Spawn Locations", Order = 1, RequireRestart = false, HintText = "Restrict the towns that Minor Clans mercenaries can spawn in.")]
        [SettingPropertyGroup("General")]
        public DefaultDropdown<string> RecruitmentSettings { get; set; } = new DefaultDropdown<string>(new string[] { "Any Culture", "Same Culture Only" }, 0); // "Regional", "Custom" future option ids for xml inputs

        [SettingPropertyFloatingInteger("Recruit Cost Multiplyer", 1f, 5f, "0", Order = 2, RequireRestart = false, HintText = "Increase the recruitment cost by this many time more than their base recruitment Cost. (base troop cost * this number)")]
        [SettingPropertyGroup("General")]
        public float RecruitCostMultiplyer { get; set; } = 1f;

        [SettingPropertyInteger("Min Number of Possible Recruits", 1, 15, "0", Order = 3, RequireRestart = false, HintText = "Minimum Number of Minor Clan Troops avaiable for recruitment.")]
        [SettingPropertyGroup("General")]
        public int MinNumberOfTroops  { get; set; } = 1;

        [SettingPropertyInteger("Max Number of Possible Recruits", 15, 30, "0", Order = 4, RequireRestart = false, HintText = "Maximun Number of Minor Clan Troops avaiable for recruitment.")]
        [SettingPropertyGroup("General")]
        public int MaxNumberOfTroops { get; set; } = 15;

        [SettingPropertyInteger("Avaiable Range of Troops Multiplyer", 1, 10, "0", Order = 5, RequireRestart = false, HintText = "Multiplies the base min/max number of possible recruits.")]
        [SettingPropertyGroup("General")]
        public int TroopMultiplyer { get; set; } = 1;

        [SettingPropertyBool("Troops Multiplyer Affect Max Only", RequireRestart = false, Order = 6, HintText = "Makes Avaiable Range Multiplyer only affect the maximun possible amount.")]
        [SettingPropertyGroup("General")]
        public bool AffectOnlyMax { get; set; } = true;
    }
}

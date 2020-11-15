using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Data;
using MCM.Abstractions.Settings.Base.Global;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bannerlord.CustomTavernMercs.Settings
{
    class Settings : AttributeGlobalSettings<Settings>
    {
        private string CustomJsonFolder => Path.Combine(TaleWorlds.Engine.Utilities.GetConfigsPath(), "ModSettings\\CustomTavernMercs");
        public override string Id => "CustomTavernMercs";
        public override string DisplayName => "Custom Tavern Mercenaries";
        private List<string> spawnOptionsProgrammatically => new List<string> { "Any Culture", "Same Culture Only" };
        public Settings()
        {
            if (!Directory.Exists(CustomJsonFolder))
            {
                Directory.CreateDirectory(CustomJsonFolder);
                string executeDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string pathToModuleData = @"..\..\ModuleData\";
                string pathToModuleDataFolder = Path.GetFullPath(Path.Combine(executeDirectoryPath, pathToModuleData));
                string[] defaultCustomExamples = Directory.GetFiles(pathToModuleDataFolder);
                foreach (string exampleFilePath in defaultCustomExamples)
                {
                    string exampleFileName = Path.GetFileName(exampleFilePath);
                    string destinationPath = Path.Combine(CustomJsonFolder, exampleFileName);
                    File.Copy(exampleFilePath, destinationPath, true);
                }
            }
            string[] customFilePaths = Directory.GetFiles(CustomJsonFolder, "*.json");
            List<string> customFiles = new List<string>();
            foreach (string customFilePath in customFilePaths)
            {
                string fileName = Path.GetFileName(customFilePath);
                customFiles.Add(fileName);
            }
            IEnumerable<string> spawnOptions = spawnOptionsProgrammatically.Concat(customFiles);
            RecruitmentSettings = new DefaultDropdown<string>(spawnOptions, 0);
        }

        [SettingPropertyDropdown("Recruitment Setting", Order = 1, RequireRestart = true, HintText = "Requires a Reload to take affect if in game. Option for spawn behavior, default or custom.")]
        [SettingPropertyGroup("General")]
        public DefaultDropdown<string> RecruitmentSettings { get; set; }

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

        [SettingPropertyFloatingInteger("Chance For Mercenaries", 0f, 1f, "#0%", Order = 7, RequireRestart = false, HintText = "Percent chance for custom mercenaries to spawn at each town. On weekly or daily occurence.")]
        [SettingPropertyGroup("General")]
        public float PossibilityOfSpawn { get; set; } = 1.00f;

        [SettingPropertyBool("Share Mercenary Spawn Point", Order = 8, RequireRestart = false, HintText = "Custom Mercenaries will spawn in the same locations as normal mercenaries instead of among the townsfolk. WARNING: May cause crash use at own risk.")]
        [SettingPropertyGroup("General")]
        public bool ShareMercenarySpawnTag { get; set; } = false;

    }
}

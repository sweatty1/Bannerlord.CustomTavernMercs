using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Bannerlord.CustomTavernMercs
{
    public class CustomMercDataHolder
    {
        public Dictionary<Town, CustomMercData> dictionaryOfMercAtTownData;

        private readonly List<string> nonJsonOptions = new List<string>() { "Any Culture", "Same Culture Only" };

        public CustomMercDataHolder()
        {
            try {
                string selectedValue = Settings.Settings.Instance.RecruitmentSettings.SelectedValue;
            }
            catch
            {
                InformationManager.DisplayMessage(new InformationMessage($"Error: Recruitment Setting was invalid. Defaulting to Any Culture. Check Options."));
                Settings.Settings.Instance.RecruitmentSettings.SelectedValue = "Any Culture";
            }
            if (nonJsonOptions.Any(op => op == Settings.Settings.Instance.RecruitmentSettings.SelectedValue))
            {
                Dictionary<Town, CustomMercData> dictionarySetUp = new Dictionary<Town, CustomMercData>();
                foreach (Town town in Town.AllTowns)
                {
                    List<TroopInfo> possibleClanTroops = PossibleTownClans(town).Select(clan => new TroopInfo(clan.BasicTroop, false, 0)).ToList();
                    dictionarySetUp.Add(town, new CustomMercData(possibleClanTroops));
                }
                dictionaryOfMercAtTownData = dictionarySetUp;
            } else
            {
                dictionaryOfMercAtTownData = CustomBuilder();
            }
        }

        private List<Clan> PossibleTownClans(Town town)
        {
            var recruitmentType = Settings.Settings.Instance.RecruitmentSettings;
            var minorClanList = new List<Clan>();
            bool anyCulture = recruitmentType.SelectedValue == "Any Culture";
            foreach (Clan clan in Clan.All)
            {
                if (clan.IsMinorFaction && clan != Clan.PlayerClan && (anyCulture || ClanIsOfTownCulture(town, clan)))
                {
                    minorClanList.Add(clan);
                }
            }
            return minorClanList;
        }

        private bool ClanIsOfTownCulture(Town town, Clan clan)
        {
            if (clan.Culture.IsMainCulture)
            {
                return town.Culture == clan.Culture;
            } else {
                // More complicated way to apply unique culture clans
                if (clan.Culture.GetCultureCode() == CultureCode.Darshi && town.Culture.GetCultureCode() == CultureCode.Aserai)
                    return true;
                else if (clan.Culture.GetCultureCode() == CultureCode.Nord && town.Culture.GetCultureCode() == CultureCode.Sturgia)
                    return true;
                else if (clan.Culture.GetCultureCode() == CultureCode.Vakken && town.Culture.GetCultureCode() == CultureCode.Sturgia)
                    return true;
                return false;
            }
        }

        private void DisplayWarningCustomJsonFailure(string reason)
        {
            InformationManager.DisplayMessage(new InformationMessage($"Warning: Interrupting Custom Mercenary Setup"));
            InformationManager.DisplayMessage(new InformationMessage($"Reason: " + reason));
        }

        private Dictionary<Town, CustomMercData> CustomBuilder()
        {
            string customJsonPath = Path.Combine(TaleWorlds.Engine.Utilities.GetConfigsPath(), "ModSettings\\CustomTavernMercs");
            string customJsonName = Settings.Settings.Instance.RecruitmentSettings.SelectedValue;
            string pathToJson = Path.Combine(customJsonPath, customJsonName);

            // set up the dictionary for custom
            Dictionary<Town, CustomMercData> dictionarySetUp = new Dictionary<Town, CustomMercData>();
            foreach (Town town in Town.AllTowns)
            {
                dictionarySetUp.Add(town, new CustomMercData(new List<TroopInfo>()));
            }

            if (File.Exists(pathToJson))
            {
                List<CustomListUnitInfo> deserializedCustomListUnitInfo;
                string customJson;
                try {
                    customJson = File.ReadAllText(pathToJson);
                } catch
                {
                    DisplayWarningCustomJsonFailure($"{customJsonName}.json ecountered an error Opening or Reading the file. Is it open in another Program?");
                    return dictionarySetUp;
                }
                deserializedCustomListUnitInfo = JsonConvert.DeserializeObject<List<CustomListUnitInfo>>(customJson);
                if (deserializedCustomListUnitInfo == null)
                {
                    DisplayWarningCustomJsonFailure($"{customJsonName}.json file was empty or lacked valid custom object.");
                    return dictionarySetUp;
                }
                IEnumerable<Town> towns;
                foreach (CustomListUnitInfo mercData in deserializedCustomListUnitInfo)
                {
                    if (mercData.TroopCharacterId == null)
                    {
                        DisplayWarningCustomJsonFailure("No TroopCharacterId present.");
                        return dictionarySetUp;
                    }
                    CharacterObject troopType = CharacterObject.Find(mercData.TroopCharacterId.ToLower());
                    if (troopType == null)
                    {
                        DisplayWarningCustomJsonFailure($"No TroopCharacter with id: {mercData.TroopCharacterId.ToLower()} was found.");
                        return dictionarySetUp;
                    }
                    if (mercData.Global)
                    {
                        towns = Town.AllTowns;
                    } else {
                        IEnumerable<Town> cultureTowns = Town.AllTowns.Where(town => mercData.Cultures.Any(culture => culture.ToLower() == town.Culture.GetName().ToString().ToLower()));
                        IEnumerable<Town> townTowns = Town.AllTowns.Where(town => mercData.Towns.Any(tNames => tNames.ToLower() == town.Name.ToString().ToLower()));
                        towns = cultureTowns.Concat(townTowns).Distinct();
                    }
                    if(towns.Count() == 0)
                    {
                        InformationManager.DisplayMessage(new InformationMessage($"Warning: No Towns listed for troop entry with id: {mercData.TroopCharacterId.ToLower()}"));
                    }
                    foreach(Town town in towns)
                    {
                        dictionarySetUp[town].PossibleMercTroopInfo.Add(new TroopInfo(troopType, mercData.CustomCost, mercData.Cost));
                    }
                }
                return dictionarySetUp;
            }
            else
            {
                DisplayWarningCustomJsonFailure($"{customJsonName}.json was not found in mod's ModuleData");
                return dictionarySetUp;
            }
        }
    }
}

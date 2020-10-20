using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace MinorClanTroopRecruitment
{
    public class MinorClanMercDataHolder
    {
        public Dictionary<Town, MinorClanMercData> dictionaryOfMercAtTownData;

        private List<string> nonJsonOptions = new List<string>() { "Any Culture", "Same Culture Only" };

        public MinorClanMercDataHolder()
        {
            if (nonJsonOptions.Any(op => op == Settings.Settings.Instance.RecruitmentSettings.SelectedValue))
            {
                Dictionary<Town, MinorClanMercData> dictionarySetUp = new Dictionary<Town, MinorClanMercData>();
                foreach (Town town in Town.AllTowns)
                {
                    List<TroopInfo> possibleClanTroops = possibleTownClans(town).Select(clan => new TroopInfo(clan.BasicTroop, false, 0)).ToList();
                    dictionarySetUp.Add(town, new MinorClanMercData(possibleClanTroops));
                }
                dictionaryOfMercAtTownData = dictionarySetUp;
            } else
            {
                dictionaryOfMercAtTownData = CustomBuilder();
            }
        }

        private List<Clan> possibleTownClans(Town town)
        {
            var recruitmentType = Settings.Settings.Instance.RecruitmentSettings;
            var minorClanList = new List<Clan>();
            if (recruitmentType.SelectedValue == "Same Culture Only")
            {
                foreach (Clan clan in Clan.All)
                {
                    if (clan.IsMinorFaction && clan != Clan.PlayerClan && clanIsOfTownCulture(town, clan))
                    {
                        minorClanList.Add(clan);
                    }
                }
                return minorClanList;
            } else {
                foreach (Clan clan in Clan.All)
                {
                    if (clan.IsMinorFaction && clan != Clan.PlayerClan)
                    {
                        minorClanList.Add(clan);
                    }
                }
                return minorClanList;
            }
        }

        private bool clanIsOfTownCulture(Town town, Clan clan)
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

        private Dictionary<Town, MinorClanMercData> CustomBuilder()
        {
            string customJsonPath = Path.Combine(TaleWorlds.Engine.Utilities.GetConfigsPath(), "ModSettings\\MinorClanTroopRecruitment\\CustomOptions");
            string customJsonName = Settings.Settings.Instance.RecruitmentSettings.SelectedValue;
            string pathToJson = Path.Combine(customJsonPath, customJsonName);

            // set up the dictionary for custom
            Dictionary<Town, MinorClanMercData> dictionarySetUp = new Dictionary<Town, MinorClanMercData>();
            foreach (Town town in Town.AllTowns)
            {
                dictionarySetUp.Add(town, new MinorClanMercData(new List<TroopInfo>()));
            }

            if (File.Exists(pathToJson))
            {
                List<CustomMercData> deserializedCustomListUnitInfo;
                string customJson;
                try {
                    customJson = File.ReadAllText(pathToJson);
                } catch
                {
                    DisplayWarningCustomJsonFailure($"{customJsonName}.json ecountered an error Opening or Reading the file. Is it open in another Program?");
                    return dictionarySetUp;
                }
                deserializedCustomListUnitInfo = JsonConvert.DeserializeObject<List<CustomMercData>>(customJson);
                if (deserializedCustomListUnitInfo == null)
                {
                    DisplayWarningCustomJsonFailure($"{customJsonName}.json file was empty or lacked valid custom object.");
                    return dictionarySetUp;
                }
                IEnumerable<Town> towns;
                foreach (CustomMercData mercData in deserializedCustomListUnitInfo)
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

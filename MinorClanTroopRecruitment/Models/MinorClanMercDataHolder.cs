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
        
        public MinorClanMercDataHolder()
        {
            if (Settings.Settings.Instance.RecruitmentSettings.SelectedValue == "Custom")
            {
                dictionaryOfMercAtTownData = CustomBuilder();
            } else
            {
                Dictionary<Town, MinorClanMercData> dictionarySetUp = new Dictionary<Town, MinorClanMercData>();
                foreach (Town town in Town.AllTowns)
                {
                    List<CharacterObject> possibleClanTroops = possibleTownClans(town).Select(clan => clan.BasicTroop).ToList();
                    dictionarySetUp.Add(town, new MinorClanMercData(possibleClanTroops));
                }
                dictionaryOfMercAtTownData = dictionarySetUp;
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
                        minorClanList.Add(clan); // don't forget hidden minor clan "Kern"
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

        private Dictionary<Town, MinorClanMercData> CustomBuilder()
        {
            string executeDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pathToJson = Path.GetFullPath(Path.Combine(executeDirectoryPath, @"..\..\ModuleData\Custom.json"));

            // set up the dictionary for custom
            Dictionary<Town, MinorClanMercData> dictionarySetUp = new Dictionary<Town, MinorClanMercData>();
            foreach (Town town in Town.AllTowns)
            {
                dictionarySetUp.Add(town, new MinorClanMercData(new List<CharacterObject>()));
            }


            if (File.Exists(pathToJson))
            {
                List<CustomMercData> deserializedCustomListUnitInfo;
                string customJson = File.ReadAllText(pathToJson);
                deserializedCustomListUnitInfo = JsonConvert.DeserializeObject<List<CustomMercData>>(customJson);
                IEnumerable<Town> towns;
                foreach (CustomMercData mercData in deserializedCustomListUnitInfo)
                {
                    CharacterObject troopType = CharacterObject.Find(mercData.TroopCharacterId.ToLower());
                    if (troopType == null)
                    {
                        // Throw Error
                    }
                    if (mercData.CustomCost)
                    {
                        //mercData.Cost;
                    }
                    if (mercData.Global)
                    {
                        towns = Town.AllTowns;
                    } else
                    {
                        IEnumerable<Town> cultureTowns = Town.AllTowns.Where(town => mercData.Cultures.Any(culture => culture.ToLower() == town.Culture.GetName().ToString().ToLower()));
                        IEnumerable<Town> townTowns = Town.AllTowns.Where(town => mercData.Towns.Any(tNames => tNames.ToLower() == town.Name.ToString().ToLower()));
                        towns = cultureTowns.Concat(townTowns).Distinct();
                    }
                    foreach(Town town in towns)
                    {
                        dictionarySetUp[town].PossibleMercTroopsTypes.Add(troopType);
                    }
                }
                return dictionarySetUp;
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage("Custom.json not found"));
                return dictionarySetUp;
            }
        }
    }
}

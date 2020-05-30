using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace MinorClanTroopRecruitment
{
    public class MinorClanMercDataHolder
    {
        public Dictionary<Town, MinorClanMercData> dictionaryOfMercAtTownData;
        public MinorClanMercDataHolder()
        {
            Dictionary<Town, MinorClanMercData> dictionarySetUp = new Dictionary<Town, MinorClanMercData>();
            foreach (Town town in Town.AllTowns)
            {   
                var possibleClans = possibleTownClans(town);
                dictionarySetUp.Add(town, new MinorClanMercData(possibleClans));
            }
            dictionaryOfMercAtTownData = dictionarySetUp;
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
    }
}

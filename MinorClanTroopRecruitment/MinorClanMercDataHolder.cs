using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace MinorClanTroopRecruitment
{
    public class MinorClanMercDataHolder
    {
        public List<Clan> minorClanList;
        public Dictionary<Town, MinorClanMercData> dictionaryOfMercAtTownData;
        public MinorClanMercDataHolder()
        {
            Dictionary<Town, MinorClanMercData> dictionarySetUp = new Dictionary<Town, MinorClanMercData>();
            foreach (Town town in Town.AllTowns)
            {
                dictionarySetUp.Add(town, new MinorClanMercData());
            }
            var minorClanList = new List<Clan>();
            foreach (Clan clan in Clan.All)
            {
                if (clan.IsMinorFaction && clan != Clan.PlayerClan)
                {
                    minorClanList.Add(clan);
                }
            }
            this.minorClanList = minorClanList;
            dictionaryOfMercAtTownData = dictionarySetUp;
        }
    }
}

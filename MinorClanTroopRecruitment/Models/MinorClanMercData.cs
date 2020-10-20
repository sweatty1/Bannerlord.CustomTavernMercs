using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace MinorClanTroopRecruitment
{
    public class MinorClanMercData
    {
		public MinorClanMercData(List<TroopInfo> possibleMercTroopsTypes)
		{
			PossibleMercTroopInfo = possibleMercTroopsTypes;
		}

		public TroopInfo TroopInfo { get; private set; }
		public int Number { get; private set; }

		public List<TroopInfo> PossibleMercTroopInfo { get; private set; }

		public CharacterObject TroopInfoCharObject()
		{
			return TroopInfo.TroopCharacterObject;
		}

		public int GetRecruitmentCost()
		{
			if (TroopInfo.HasCustomCost)
			{
				return TroopInfo.CustomCost;
			}
			else
			{
				return Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(TroopInfoCharObject(), Hero.MainHero, false);
			}
		}

		public void ChangeMercenaryType(TroopInfo newTroopInfo, int number)
		{
			if (newTroopInfo != TroopInfo)
			{
				TroopInfo = newTroopInfo;
				Number = number;
				return;
			}
			if (Number != number)
			{
				int difference = number - Number;
				ChangeMercenaryCount(difference);
			}
		}

		public void ChangeMercenaryCount(int difference)
		{
			if (difference != 0)
			{
				Number += difference;
			}
		}

		public bool HasAvailableMercenary(Occupation occupation = Occupation.NotAssigned)
		{
			return TroopInfo != null && TroopInfoCharObject() != null && Number > 0 && (occupation == Occupation.NotAssigned || TroopInfoCharObject().Occupation == occupation);
		}
	}

	public struct TroopInfo
	{
		public CharacterObject TroopCharacterObject;
		public bool HasCustomCost;
		public int CustomCost;

		public TroopInfo(CharacterObject characterObject, bool hasCustomCost, int customCost)
		{
			TroopCharacterObject = characterObject;
			HasCustomCost = hasCustomCost;
			CustomCost = customCost;
		}

		public static bool operator ==(TroopInfo troop1, TroopInfo troop2)
		{
			return troop1.Equals(troop2);
		}
		public static bool operator !=(TroopInfo troop1, TroopInfo troop2)
		{
			return !troop1.Equals(troop2);
		}
	}
}

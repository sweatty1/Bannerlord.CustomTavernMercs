using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace MinorClanTroopRecruitment
{
    public class MinorClanMercData
    {
		//public MinorClanMercData(List<CharacterObject> possibleMercTroopsTypes)
		public MinorClanMercData(List<TroopInfoStruct> possibleMercTroopsTypes)
		{
			//PossibleMercTroopsTypes = possibleMercTroopsTypes;
			PossibleMercTroopInfo = possibleMercTroopsTypes;
		}

		//public CharacterObject TroopType { get; private set; }
		//public int Number { get; private set; }

		//public bool HasCustomCost { get; private set; } = false;
		//public int CustomCost { get; private set; }
		//public List<CharacterObject> PossibleMercTroopsTypes { get; private set; }

		public TroopInfoStruct TroopInfo { get; private set; }
		public int Number { get; private set; }

		public List<TroopInfoStruct> PossibleMercTroopInfo { get; private set; }

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

		public void ChangeMercenaryType(TroopInfoStruct newTroopStruct, int number)
		{
			if (newTroopStruct != TroopInfo)
			{
				TroopInfo = newTroopStruct;
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

	public struct TroopInfoStruct
	{
		public CharacterObject TroopCharacterObject;
		public bool HasCustomCost;
		public int CustomCost;

		public TroopInfoStruct(CharacterObject characterObject, bool hasCustomCost, int customCost)
		{
			TroopCharacterObject = characterObject;
			HasCustomCost = hasCustomCost;
			CustomCost = customCost;
		}

		public static bool operator ==(TroopInfoStruct troop1, TroopInfoStruct troop2)
		{
			return troop1.Equals(troop2);
		}
		public static bool operator !=(TroopInfoStruct troop1, TroopInfoStruct troop2)
		{
			return !troop1.Equals(troop2);
		}
	}
}

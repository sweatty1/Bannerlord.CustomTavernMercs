using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace MinorClanTroopRecruitment
{
    public class MinorClanMercData
    {
		public MinorClanMercData(List<CharacterObject> possibleMercTroopsTypes)
		{
			PossibleMercTroopsTypes = possibleMercTroopsTypes;
		}

		public CharacterObject TroopType { get; private set; }
		public int Number { get; private set; }

		public bool HasCustomCost { get; private set; } = false;
		public int CustomCost { get; private set; }
		public List<CharacterObject> PossibleMercTroopsTypes { get; private set; }

		public int GetRecruitmentCost()
		{
			if (HasCustomCost)
			{
				return CustomCost;
			}
			else
			{
				return Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(TroopType, Hero.MainHero, false);
			}
		}

		public void ChangeMercenaryType(CharacterObject troopType, int number)
		{
			if (troopType != this.TroopType)
			{
				CharacterObject troopType2 = this.TroopType;
				this.TroopType = troopType;
				this.Number = number;
				return;
			}
			if (this.Number != number)
			{
				int difference = number - this.Number;
				this.ChangeMercenaryCount(difference);
			}
		}

		public void ChangeMercenaryCount(int difference)
		{
			if (difference != 0)
			{
				int number = this.Number;
				this.Number += difference;
			}
		}

		public bool HasAvailableMercenary(Occupation occupation = Occupation.NotAssigned)
		{
			return this.TroopType != null && this.Number > 0 && (occupation == Occupation.NotAssigned || this.TroopType.Occupation == occupation);
		}
	}
}

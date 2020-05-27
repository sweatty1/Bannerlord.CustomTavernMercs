using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace MinorClanTroopRecruitment
{
    public class MinorClanMercData
    {
		public MinorClanMercData(List<Clan> possibleClans)
		{
			PossibleClans = possibleClans;
		}

		public CharacterObject TroopType { get; private set; }
		public int Number { get; private set; }
		public List<Clan> PossibleClans { get; private set; }

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

using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;

namespace Bannerlord.CustomTavernMercs
{
    public class CustomMercData
    {
		public CustomMercData(List<TroopInfo> possibleMercTroopsTypes)
		{
			PossibleMercTroopInfo = possibleMercTroopsTypes;
		}

		public TroopInfo TroopInfo { get; private set; }
		public int Number { get; private set; }

		public List<TroopInfo> PossibleMercTroopInfo { get; private set; }

		public LocationCharacter LocationChar { get; private set; }

		public CharacterObject TroopInfoCharObject()
		{
			if (TroopInfo == null) { 
				return null; 
			}
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

		public bool HasAvailableMercenary()
		{
			return TroopInfo != null && TroopInfoCharObject() != null && Number > 0;
		}

		public LocationCharacter UpdateLocationChar(CultureObject culture, LocationCharacter.CharacterRelations relation)
		{
			LocationChar = CreateCustomMercenary();
			return LocationChar;
		}

		private LocationCharacter CreateCustomMercenary()
		{
			string spawnTag = Settings.Settings.Instance.ShareMercenarySpawnTag ? "spawnpoint_mercenary" : "npc_common";
			return new LocationCharacter(new AgentData(new SimpleAgentOrigin(TroopInfoCharObject(), -1, null, default(UniqueTroopDescriptor))).Monster(Campaign.Current.HumanMonsterSettlement).NoHorses(true), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddOutdoorWandererBehaviors), spawnTag, true, LocationCharacter.CharacterRelations.Neutral, null, false, false, null, false, false, true);
		}
    }

	public class TroopInfo
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
	}
}

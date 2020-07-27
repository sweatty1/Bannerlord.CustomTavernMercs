using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Localization;

namespace MinorClanTroopRecruitment
{
	internal class EnchanceBaseMercRecruitment : CampaignBehaviorBase
    {
        public override void SyncData(IDataStore dataStore) { }
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
        }

        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            this.AddDialogs(campaignGameStarter);
            this.AddGameMenus(campaignGameStarter);
        }

		protected void AddDialogs(CampaignGameStarter campaignGameStarter)
		{
			campaignGameStarter.AddPlayerLine("mercenary_recruit_talk_hire_all_past_limit", "mercenary_tavern_talk", "mercenary_tavern_talk_hire", "All right. I will hire {?PLURAL}all of you{?}you{\\?}. Here is {GOLD_AMOUNT_MOD}{GOLD_ICON} (Hires Past Party Limit)", new ConversationSentence.OnConditionDelegate(this.conversation_mercenary_recruit_accept_all_on_condition_past_limit), new ConversationSentence.OnConsequenceDelegate(this.conversation_mercenary_recruit_accept_all_on_consequence), 110, null, null);
			campaignGameStarter.AddPlayerLine("mercenary_recruit_talk_hire_some_past_limit", "mercenary_tavern_talk", "mercenary_tavern_talk_hire", "All right. But I can only hire {MERCENARY_COUNT_SOME_AFFORD} of you. Here is {GOLD_AMOUNT_SOME_AFFORD}{GOLD_ICON} (Hires Past Party Limit)", new ConversationSentence.OnConditionDelegate(this.conversation_mercenary_recruit_accept_some_on_condition_past_limit_afford), new ConversationSentence.OnConsequenceDelegate(this.conversation_mercenary_recruit_accept_some_past_limit_on_consequence), 110, null, null);
		}
		private bool conversation_mercenary_recruit_accept_all_on_condition_past_limit()
		{
			Town.TownMercenaryData mercData = MobileParty.MainParty.CurrentSettlement.Town.MercenaryData;
			int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(mercData.TroopType, Hero.MainHero, false);
			int numOfTroopPlayerCanBuy = Hero.MainHero.Gold / troopRecruitmentCost;
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			MBTextManager.SetTextVariable("PLURAL", (mercData.Number > 1) ? 1 : 0, false);
			MBTextManager.SetTextVariable("GOLD_AMOUNT_MOD", troopRecruitmentCost * mercData.Number, false);
			return numOfTroopSlotsOpen < mercData.Number && numOfTroopPlayerCanBuy >= mercData.Number;
		}

		private void conversation_mercenary_recruit_accept_all_on_consequence()
		{
			this.BuyMinorClanMercenariesInTavern(MobileParty.MainParty.CurrentSettlement.Town.MercenaryData.Number);
		}


		private bool conversation_mercenary_recruit_accept_some_on_condition_past_limit_afford()
		{
			int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(MobileParty.MainParty.CurrentSettlement.Town.MercenaryData.TroopType, Hero.MainHero, false);
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			if (Hero.MainHero.Gold >= troopRecruitmentCost && Hero.MainHero.Gold < MobileParty.MainParty.CurrentSettlement.Town.MercenaryData.Number * troopRecruitmentCost)
			{
				int numberToHire = 0;
				while (Hero.MainHero.Gold > troopRecruitmentCost * (numberToHire + 1) && MobileParty.MainParty.CurrentSettlement.Town.MercenaryData.Number > numberToHire)
				{
					numberToHire++;
				}
				if (numberToHire <= numOfTroopSlotsOpen)
				{
					return false;
				}
				MBTextManager.SetTextVariable("MERCENARY_COUNT_SOME_AFFORD", numberToHire, false);
				MBTextManager.SetTextVariable("GOLD_AMOUNT_SOME_AFFORD", troopRecruitmentCost * numberToHire, false);
				return true;
			}
			return false;
		}

		private void conversation_mercenary_recruit_accept_some_past_limit_on_consequence()
		{
			int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(MobileParty.MainParty.CurrentSettlement.Town.MercenaryData.TroopType, Hero.MainHero, false);
			int numberToHire = 0;
			while (Hero.MainHero.Gold > troopRecruitmentCost * (numberToHire + 1) && MobileParty.MainParty.CurrentSettlement.Town.MercenaryData.Number > numberToHire)
			{
				numberToHire++;
			}
			this.BuyMinorClanMercenariesInTavern(numberToHire);
		}

		private void BuyMinorClanMercenariesInTavern(int numberOfMercsToHire)
		{
			Town.TownMercenaryData mercData = MobileParty.MainParty.CurrentSettlement.Town.MercenaryData;
			mercData.ChangeMercenaryCount(-numberOfMercsToHire);
			int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(mercData.TroopType, Hero.MainHero, false);
			MobileParty.MainParty.AddElementToMemberRoster(mercData.TroopType, numberOfMercsToHire, false);
			int amount = numberOfMercsToHire * troopRecruitmentCost;
			GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, amount, false);
			CampaignEventDispatcher.Instance.OnUnitRecruited(mercData.TroopType, numberOfMercsToHire);
		}

		public void AddGameMenus(CampaignGameStarter campaignGameStarter)
		{
			campaignGameStarter.AddGameMenuOption("town_backstreet", "recruit_regular_mercenaries_party_limit", "{=*}Recruit to Party Limit {REG_MEN_COUNT_PL} {REG_MERCENARY_NAME_PL} ({REG_TOTAL_AMOUNT_PL}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(this.BuyRegMercsViaMenuConditionToPartyLimit), delegate (MenuCallbackArgs x)
			{
				BuyRegMercenariesViaGameMenuToPartyLimit();
			}, false, 1, false);
		}

		private bool BuyRegMercsViaMenuConditionToPartyLimit(MenuCallbackArgs args)
		{
			if (MobileParty.MainParty.CurrentSettlement != null && MobileParty.MainParty.CurrentSettlement.IsTown && MobileParty.MainParty.CurrentSettlement.Town.MercenaryData != null && MobileParty.MainParty.CurrentSettlement.Town.MercenaryData.Number > 0)
			{
				int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(MobileParty.MainParty.CurrentSettlement.Town.MercenaryData.TroopType, null, false);
				int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
				int numOfTroopPlayerCanBuy = Hero.MainHero.Gold / troopRecruitmentCost;
				if (numOfTroopSlotsOpen > 0 && Hero.MainHero.Gold >= troopRecruitmentCost && numOfTroopSlotsOpen < MobileParty.MainParty.CurrentSettlement.Town.MercenaryData.Number && numOfTroopSlotsOpen < numOfTroopPlayerCanBuy)
				{
					int numOfMercs = Math.Min(MobileParty.MainParty.CurrentSettlement.Town.MercenaryData.Number, numOfTroopPlayerCanBuy);
					numOfMercs = Math.Min(numOfTroopSlotsOpen, numOfMercs);
					MBTextManager.SetTextVariable("REG_MEN_COUNT_PL", numOfMercs, false);
					MBTextManager.SetTextVariable("REG_MERCENARY_NAME_PL", MobileParty.MainParty.CurrentSettlement.Town.MercenaryData.TroopType.Name, false);
					MBTextManager.SetTextVariable("REG_TOTAL_AMOUNT_PL", numOfMercs * troopRecruitmentCost, false);
					args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
					return true;
				}
			}
			return false;
		}

		private void BuyRegMercenariesViaGameMenuToPartyLimit()
		{
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			if (MobileParty.MainParty.CurrentSettlement != null && MobileParty.MainParty.CurrentSettlement.IsTown && MobileParty.MainParty.CurrentSettlement.Town.MercenaryData != null && MobileParty.MainParty.CurrentSettlement.Town.MercenaryData.Number > 0 && numOfTroopSlotsOpen > 0)
			{
				int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(MobileParty.MainParty.CurrentSettlement.Town.MercenaryData.TroopType, null, false);
				if (Hero.MainHero.Gold >= troopRecruitmentCost)
				{
					int numOfMercs = Math.Min(MobileParty.MainParty.CurrentSettlement.Town.MercenaryData.Number, Hero.MainHero.Gold / troopRecruitmentCost);
					numOfMercs = Math.Min(numOfTroopSlotsOpen, numOfMercs);
					MobileParty.MainParty.MemberRoster.AddToCounts(MobileParty.MainParty.CurrentSettlement.Town.MercenaryData.TroopType, numOfMercs, false, 0, 0, true, -1);
					GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, -(numOfMercs * troopRecruitmentCost), false);
					MobileParty.MainParty.CurrentSettlement.Town.MercenaryData.ChangeMercenaryCount(-numOfMercs);
					GameMenu.SwitchToMenu("town_backstreet");
				}
			}
		}
	}
}

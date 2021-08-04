using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.CampaignSystem.GameMenus;
using System.Collections.Generic;

namespace Bannerlord.CustomTavernMercs
{
	internal class CustomTavernMercsBehaviors : CampaignBehaviorBase
    {
		public override void SyncData(IDataStore dataStore) { }

		public CustomMercDataHolder custom_merc_data_holder = null;

		public override void RegisterEvents()
		{
			CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnAfterNewGameCreated));
			CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.OnSettlementEntered));
			CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, new Action<MobileParty, Settlement>(this.OnSettlementExit));
			CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnGameLoaded));
			CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
			if (Settings.Settings.Instance.UpdateTiming.SelectedValue == "Weekly")
			{
				CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, new Action(this.WeeklyTickTown));
			}
			else
			{
				CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, new Action<Town>(this.DailyTickTown));
			}
		}

		// Only triggers on loaded games
		private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
		{
			if (custom_merc_data_holder == null)
			{
				CustomMercDataHolder customMercDataHolder = new CustomMercDataHolder();
				custom_merc_data_holder = customMercDataHolder;
				foreach (Town town in Town.AllTowns)
				{
					UpdateCurrentMercenaryTroopAndCount(town);
				}
			}
			// Add Character if inside of town
			if (Settlement.CurrentSettlement != null)
			{
				AddCustomMercenaryCharacterToTavern(Settlement.CurrentSettlement);
			}
		}

		// Only triggers on new campaigns created
		private void OnAfterNewGameCreated(CampaignGameStarter starter)
		{
			if (custom_merc_data_holder == null)
			{
				CustomMercDataHolder customMercDataHolder = new CustomMercDataHolder();
				custom_merc_data_holder = customMercDataHolder;
				foreach (Town town in Town.AllTowns)
				{
					UpdateCurrentMercenaryTroopAndCount(town);
				}
			}
		}
		public void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
		{
			if (mobileParty != MobileParty.MainParty) return;
			AddCustomMercenaryCharacterToTavern(settlement);
		}

		public void OnSettlementExit(MobileParty mobileParty, Settlement settlement)
        {
			if (mobileParty != MobileParty.MainParty) return;
			RemoveMercenaryCharacterFromTavern(settlement);
		}

		//Remove Character from the Tavern
		private void RemoveMercenaryCharacterFromTavern(Settlement settlement)
        {
			if (settlement.IsTown && settlement.LocationComplex != null)
            {
				LocationCharacter locationCharToRemove = custom_merc_data_holder.dictionaryOfMercAtTownData[settlement.Town].LocationChar;
				Location locationWithId = settlement.LocationComplex.GetLocationWithId("tavern");
				if (locationWithId != null && locationWithId.ContainsCharacter(locationCharToRemove))
                {
					settlement.LocationComplex.GetLocationWithId("tavern").RemoveLocationCharacter(locationCharToRemove);
				}
			}
        }

		// Adding Character to the Tavern
		private void AddCustomMercenaryCharacterToTavern(Settlement settlement)
		{
			if (!Hero.MainHero.IsPrisoner && settlement.IsTown && !settlement.IsUnderSiege && settlement.LocationComplex != null && custom_merc_data_holder.dictionaryOfMercAtTownData[settlement.Town].HasAvailableMercenary())
			{
				Location locationWithId = settlement.LocationComplex.GetLocationWithId("tavern");
				if (locationWithId != null && !locationWithId.ContainsCharacter(custom_merc_data_holder.dictionaryOfMercAtTownData[settlement.Town].LocationChar))
				{
					locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(custom_merc_data_holder.dictionaryOfMercAtTownData[settlement.Town].UpdateLocationChar), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
				}
			}
		}

        // Update customMerc troops
        private void DailyTickTown(Town town)
		{
			UpdateCurrentMercenaryTroopAndCount(town);
		}

		private void WeeklyTickTown()
		{
			foreach (Town town in Town.AllTowns)
			{
				UpdateCurrentMercenaryTroopAndCount(town);
			}
		}

		private static int FindNumberOfMercenariesToAdd()
		{
			float troopMultipler = Settings.Settings.Instance.TroopMultiplier;
			int minNumberOfTroops = Settings.Settings.Instance.MinNumberOfTroops;
			int maxNumberOfTroops = Settings.Settings.Instance.MaxNumberOfTroops + 1; // if set at 15 will never get 15 need this + 1
			float numOfMercs = MBRandom.RandomInt(minNumberOfTroops, maxNumberOfTroops);
			numOfMercs *= troopMultipler;
			return MBRandom.RoundRandomized(numOfMercs);
		}

		private void UpdateCurrentMercenaryTroopAndCount(Town town)
		{
			List<TroopInfo> possibleMercTroops = custom_merc_data_holder.dictionaryOfMercAtTownData[town].PossibleMercTroopInfo;
			if (possibleMercTroops.Count == 0) return;
			int r = MBRandom.Random.Next(possibleMercTroops.Count);
			int numbOfUnits = FindNumberOfMercenariesToAdd();
			if (MBRandom.RandomFloat > Settings.Settings.Instance.PossibilityOfSpawn)
			{
				numbOfUnits = 0;
			}
			TroopInfo newTroopInfo = possibleMercTroops[r];
			custom_merc_data_holder.dictionaryOfMercAtTownData[town].ChangeMercenaryType(newTroopInfo, numbOfUnits);

			// Since we don't have access to MercenaryNUmberChangedInTown or MercenaryTroopChangedInTown
			// need way to trigger spawn of hire guy in tavern when inside of town on a daily update
			// instead of PlayerEncounter.LocationEncounter != null just using currentSettlement to determine if inside of settlement
			// locationChar is null until one is set in AddCustomMercenaryCharacterToTavern as we only want this to trigger on this function after the first load.
			if (custom_merc_data_holder.dictionaryOfMercAtTownData[town].LocationChar != null && town.Settlement == Settlement.CurrentSettlement && MobileParty.MainParty.CurrentSettlement != null && MobileParty.MainParty.CurrentSettlement.IsTown && MobileParty.MainParty.CurrentSettlement.Town == town)
			{
				RemoveMercenaryCharacterFromTavern(town.Settlement);
				AddCustomMercenaryCharacterToTavern(town.Settlement);
			}
		}

		// start of the dialog and game Menu code flows
		public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
		{
			AddDialogs(campaignGameStarter);
			AddGameMenus(campaignGameStarter);
		}

		private CustomMercData GetCustomMercDataOfPlayerEncounter()
		{
			return custom_merc_data_holder.dictionaryOfMercAtTownData[MobileParty.MainParty.CurrentSettlement.Town];
		}

		private int troopRecruitmentCost(CustomMercData customMercData)
		{
			float recruitCostMultiplier = Settings.Settings.Instance.RecruitCostMultiplier;
			int baseCost = customMercData.GetRecruitmentCost();
			return MBRandom.RoundRandomized(baseCost * recruitCostMultiplier);
		}

		// TAVERN CODE
		protected void AddDialogs(CampaignGameStarter campaignGameStarter)
		{
			campaignGameStarter.AddDialogLine("custom_merc_talk_start_plural", "start", "custom_merc_tavern_talk", "Do you have a need for fighters, {?PLAYER.GENDER}madam{?}sir{\\?}? Me and {?CMERCS_PLURAL}{CMERCS_MERCENARY_COUNT} of my mates{?}one of my mates{\\?} looking for a master. You might call us mercenaries, like. We'll join you for {CMERCS_GOLD_AMOUNT}{GOLD_ICON}",
				new ConversationSentence.OnConditionDelegate(custom_mercenary_plural_start_condition), null, 150, null);
			campaignGameStarter.AddDialogLine("custom_merc_talk_start_singlular", "start", "custom_merc_tavern_talk", "Do you have a need for fighters, {?PLAYER.GENDER}madam{?}sir{\\?}? I am looking for a master. I'll join you for {CMERCS_GOLD_AMOUNT}{GOLD_ICON}",
				new ConversationSentence.OnConditionDelegate(custom_mercenary_single_start_condition), null, 150, null);
			campaignGameStarter.AddPlayerLine("custom_merc_talk_hire_one", "custom_merc_tavern_talk", "custom_merc_tavern_talk_hire_one", "All right. I would only like to hire one of you. Here is {CMERCS_GOLD_AMOUNT_FOR_ONE}{GOLD_ICON}",
				new ConversationSentence.OnConditionDelegate(custom_mercenary_hire_one_condition), delegate ()
				{
					HireCustomMercenariesInTavern(true);
				}, 110, null, null);
			campaignGameStarter.AddDialogLine("custom_merc_talk_hire_one_response", "custom_merc_tavern_talk_hire_one", "custom_merc_tavern_talk", "Deal, One of us will report to your party outside the gates after gathering their gear. Need anything else?", null, null, 100, null);
			campaignGameStarter.AddPlayerLine("custom_merc_talk_hire_all", "custom_merc_tavern_talk", "custom_merc_tavern_talk_hire", "All right. I will hire {?CMERCS_PLURAL}all of you{?}you{\\?}. Here is {CMERCS_GOLD_AMOUNT_ALL}{GOLD_ICON}",
				new ConversationSentence.OnConditionDelegate(custom_mercenary_hire_all_condition), delegate ()
				{
					HireCustomMercenariesInTavern(false, false);
				}, 100, null, null);
			campaignGameStarter.AddPlayerLine("custom_merc_talk_hire_all_past_limit", "custom_merc_tavern_talk", "custom_merc_tavern_talk_hire", "All right. I will hire {?CMERCS_PLURAL}all of you{?}you{\\?}. Here is {CMERCS_GOLD_AMOUNT_ALL}{GOLD_ICON} (Hires Past Party Limit)",
				new ConversationSentence.OnConditionDelegate(custom_mercenary_hire_all_past_limit_condition), delegate ()
				{
					HireCustomMercenariesInTavern(false, true);
				}, 110, null, null);
			campaignGameStarter.AddPlayerLine("custom_merc_talk_hire_some_past_limit", "custom_merc_tavern_talk", "custom_merc_tavern_talk_hire", "All right. But I can only hire {CMERCS_MERCENARY_COUNT_SOME_AFFORD} of you. Here is {CMERCS_GOLD_AMOUNT_SOME_AFFORD}{GOLD_ICON} (Hires Past Party Limit)",
				new ConversationSentence.OnConditionDelegate(custom_mercenary_afford_hire_some_past_limit_condition), delegate ()
				{
					HireCustomMercenariesInTavern(false, true);
				}, 110, null, null);
			campaignGameStarter.AddPlayerLine("custom_merc_talk_hire_some", "custom_merc_tavern_talk", "custom_merc_tavern_talk_hire", "All right. But I can only hire {CMERCS_MERCENARY_COUNT_SOME} of you. Here is {CMERCS_GOLD_AMOUNT_SOME}{GOLD_ICON}",
				new ConversationSentence.OnConditionDelegate(custom_mercenary_hire_some_on_condition), delegate ()
				{
					HireCustomMercenariesInTavern(false, false);
				}, 100, null, null);
			campaignGameStarter.AddPlayerLine("custom_merc_talk_reject_no_gold", "custom_merc_tavern_talk", "close_window", "That sounds good. But I can't hire any more men right now.",
				new ConversationSentence.OnConditionDelegate(custom_mercenary_reject_gold_or_party_size_condition), null, 100, null, null);
			campaignGameStarter.AddPlayerLine("custom_merc_talk_reject_party_full", "custom_merc_tavern_talk", "close_window", "Sorry. I don't need any other men right now.",
				new ConversationSentence.OnConditionDelegate(custom_mercenary_dont_need_men_condition), null, 100, null, null);
			campaignGameStarter.AddDialogLine("custom_merc_talk_hired_end", "custom_merc_tavern_talk_hire", "close_window", "{RANDOM_HIRE_SENTENCE}",
				new ConversationSentence.OnConditionDelegate(custom_mercenary_end_condition), null, 100, null);
			campaignGameStarter.AddDialogLine("custom_merc_talk_start_post_hire", "start", "close_window", "Don't worry, I'll be ready. Just having a last drink for the road.",
				new ConversationSentence.OnConditionDelegate(custom_mercenary_post_hire_start_condition), null, 150, null);
		}

		private bool CustomMercIsInTavern(CustomMercData customMercData)
		{
			if (CampaignMission.Current == null || CampaignMission.Current.Location == null || customMercData.TroopInfo == null || customMercData.TroopInfoCharObject() == null)
			{
				return false;
			}
			return CampaignMission.Current.Location.StringId == "tavern" && customMercData.TroopInfoCharObject().Name == CharacterObject.OneToOneConversationCharacter.Name;
		}

		// Conditions for starting line dialog
		private bool custom_mercenary_plural_start_condition()
		{
			if(MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return false;
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			bool flag = customMercData.Number > 1 && CustomMercIsInTavern(customMercData);
			if (flag)
			{
				int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
				MBTextManager.SetTextVariable("CMERCS_PLURAL", (customMercData.Number > 1) ? 1 : 0);
				MBTextManager.SetTextVariable("CMERCS_MERCENARY_COUNT", customMercData.Number - 1);
				MBTextManager.SetTextVariable("CMERCS_GOLD_AMOUNT", troopRecruitmentCost * customMercData.Number);
			}
			return flag;
		}

		private bool custom_mercenary_single_start_condition()
		{
			if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return false;
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			bool flag = customMercData.Number == 1 && CustomMercIsInTavern(customMercData);
			if (flag)
			{
				int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
				MBTextManager.SetTextVariable("CMERCS_GOLD_AMOUNT", customMercData.Number * troopRecruitmentCost);
			}
			return flag;
		}

		// Conditions for Hiring options and functions that follow
		private bool custom_mercenary_hire_one_condition()
		{
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
			int numOfTroopPlayerCanBuy = Hero.MainHero.Gold / troopRecruitmentCost;
			MBTextManager.SetTextVariable("CMERCS_GOLD_AMOUNT_FOR_ONE", troopRecruitmentCost);
			return 1 < customMercData.Number && numOfTroopPlayerCanBuy > 1;
		}

		private bool custom_mercenary_hire_all_condition()
		{
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			MBTextManager.SetTextVariable("CMERCS_PLURAL", (customMercData.Number > 1) ? 1 : 0);
			MBTextManager.SetTextVariable("CMERCS_GOLD_AMOUNT_ALL", troopRecruitmentCost * customMercData.Number);
			return Hero.MainHero.Gold >= customMercData.Number * troopRecruitmentCost && numOfTroopSlotsOpen >= customMercData.Number;
		}

		private bool custom_mercenary_hire_all_past_limit_condition()
		{
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
			int numOfTroopPlayerCanBuy = (troopRecruitmentCost==0) ? customMercData.Number : Hero.MainHero.Gold / troopRecruitmentCost;
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			MBTextManager.SetTextVariable("CMERCS_PLURAL", (customMercData.Number > 1) ? 1 : 0);
			MBTextManager.SetTextVariable("CMERCS_GOLD_AMOUNT_ALL", troopRecruitmentCost * numOfTroopPlayerCanBuy);
			return numOfTroopSlotsOpen < customMercData.Number && numOfTroopPlayerCanBuy >= customMercData.Number;
		}

		private bool custom_mercenary_hire_some_on_condition()
		{
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			if (Hero.MainHero.Gold >= troopRecruitmentCost && numOfTroopSlotsOpen > 0 && (Hero.MainHero.Gold < customMercData.Number * troopRecruitmentCost || numOfTroopSlotsOpen < customMercData.Number))
			{
				int numberToHire = 0;
				while (Hero.MainHero.Gold > troopRecruitmentCost * (numberToHire + 1) && numOfTroopSlotsOpen > numberToHire)
				{
					numberToHire++;
				}
				MBTextManager.SetTextVariable("CMERCS_MERCENARY_COUNT_SOME", numberToHire);
				MBTextManager.SetTextVariable("CMERCS_GOLD_AMOUNT_SOME", troopRecruitmentCost * numberToHire);
				return true;
			}
			return false;
		}

		private bool custom_mercenary_afford_hire_some_past_limit_condition()
		{
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			if (Hero.MainHero.Gold >= troopRecruitmentCost && Hero.MainHero.Gold < customMercData.Number * troopRecruitmentCost)
			{
				int numberToHire = 0;
				while (Hero.MainHero.Gold > troopRecruitmentCost * (numberToHire + 1) && customMercData.Number > numberToHire)
				{
					numberToHire++;
				}
				if (numberToHire <= numOfTroopSlotsOpen) return false;
				MBTextManager.SetTextVariable("CMERCS_MERCENARY_COUNT_SOME_AFFORD", numberToHire);
				MBTextManager.SetTextVariable("CMERCS_GOLD_AMOUNT_SOME_AFFORD", troopRecruitmentCost * numberToHire);
				return true;
			}
			return false;
		}

		// Conditions close Conversation
		private bool custom_mercenary_post_hire_start_condition()
		{
			if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return false;
			CustomMercData mercData = GetCustomMercDataOfPlayerEncounter();
			return CustomMercIsInTavern(mercData);
		}

		private bool custom_mercenary_reject_gold_or_party_size_condition()
		{
			int troopRecruitmentCost = this.troopRecruitmentCost(GetCustomMercDataOfPlayerEncounter());
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			return Hero.MainHero.Gold < troopRecruitmentCost || numOfTroopSlotsOpen <= 0;
		}

		private bool custom_mercenary_dont_need_men_condition()
		{
			int troopRecruitmentCost = this.troopRecruitmentCost(GetCustomMercDataOfPlayerEncounter());
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			return Hero.MainHero.Gold >= troopRecruitmentCost && numOfTroopSlotsOpen > 0;
		}

		// Successful hire npc phrase
		public bool custom_mercenary_end_condition()
		{
			MBTextManager.SetTextVariable("RANDOM_HIRE_SENTENCE", GameTexts.FindText("str_mercenary_tavern_talk_hire", MBRandom.RandomInt(4).ToString()));
			return true;
		}

		// Actual Hiring from Tavern
		private void HireCustomMercenariesInTavern(bool buyOne, bool pastPartyLimit = false)
		{
			if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return;
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			if (customMercData == null) return;

			int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
			int numberOfMercsToHire = 0;
			if(buyOne)
			{
				numberOfMercsToHire = 1;
			} else
			{
				int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
				while (Hero.MainHero.Gold > troopRecruitmentCost * (numberOfMercsToHire + 1) && customMercData.Number > numberOfMercsToHire && (pastPartyLimit || numOfTroopSlotsOpen > numberOfMercsToHire))
				{
					numberOfMercsToHire++;
				}
			}
			customMercData.ChangeMercenaryCount(-numberOfMercsToHire);
			MobileParty.MainParty.AddElementToMemberRoster(customMercData.TroopInfoCharObject(), numberOfMercsToHire, false);
			int amount = numberOfMercsToHire * troopRecruitmentCost;
			GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, amount, false);
			CampaignEventDispatcher.Instance.OnUnitRecruited(customMercData.TroopInfoCharObject(), numberOfMercsToHire);
		}

		// GAME MENU CODE
		public void AddGameMenus(CampaignGameStarter campaignGameStarter)
		{
			// index is location in menu 0 being top, 1 next if other of same index exist this are placed on top of them
			campaignGameStarter.AddGameMenuOption("town_backstreet", "recruit_custom_mercenaries_all", "{=*}Recruit {C_MEN_COUNT} {C_MERCENARY_NAME} ({C_TOTAL_AMOUNT}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(HireCustomMercsViaMenuCondition), delegate (MenuCallbackArgs x)
			{
				HireCustomMecenariesViaGameMenu(false, false);
			}, false, 1, false);
			campaignGameStarter.AddGameMenuOption("town_backstreet", "recruit_custom_mercenaries_party_limit", "{=*}Recruit to Party Limit {C_MEN_COUNT_PL} {C_MERCENARY_NAME_PL} ({C_TOTAL_AMOUNT_PL}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(HireCustomMercsViaMenuConditionToPartyLimit), delegate (MenuCallbackArgs x)
			{
				HireCustomMecenariesViaGameMenu(false, true);
			}, false, 1, false);
			campaignGameStarter.AddGameMenuOption("town_backstreet", "recruit_custom_mercenaries_hire_one", "{=*}Recruit 1 {C_MERCENARY_NAME_ONLY_ONE} ({C_TOTAL_AMOUNT_ONLY_ONE}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(HireCustomMercsViaMenuConditionHireOne), delegate (MenuCallbackArgs x)
			{
				HireCustomMecenariesViaGameMenu(true, false);
			}, false, 1, false);
		}

		private bool HireCustomMercsViaMenuConditionHireOne(MenuCallbackArgs args)
		{
			if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return false;
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			if (customMercData != null && customMercData.Number > 1)
			{
				int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
				int numOfTroopPlayerCanBuy = Hero.MainHero.Gold / troopRecruitmentCost;
				if (numOfTroopPlayerCanBuy > 1)
				{
					MBTextManager.SetTextVariable("C_MERCENARY_NAME_ONLY_ONE", customMercData.TroopInfoCharObject().Name);
					MBTextManager.SetTextVariable("C_TOTAL_AMOUNT_ONLY_ONE", troopRecruitmentCost);
					args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
					return true;
				}
			}
			return false;
		}

		private bool HireCustomMercsViaMenuCondition(MenuCallbackArgs args)
		{
			if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return false;
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			if (customMercData != null && customMercData.Number > 0)
			{
				int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
				if (Hero.MainHero.Gold >= troopRecruitmentCost)
				{
					int numOfTroopPlayerCanBuy = (troopRecruitmentCost == 0) ? customMercData.Number : Hero.MainHero.Gold / troopRecruitmentCost;
					int num = Math.Min(customMercData.Number, numOfTroopPlayerCanBuy);
					MBTextManager.SetTextVariable("C_MEN_COUNT", num);
					MBTextManager.SetTextVariable("C_MERCENARY_NAME", customMercData.TroopInfoCharObject().Name);
					MBTextManager.SetTextVariable("C_TOTAL_AMOUNT", num * troopRecruitmentCost);
					args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
					return true;
				}
			}
			return false;
		}

		private bool HireCustomMercsViaMenuConditionToPartyLimit(MenuCallbackArgs args)
		{
			if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return false;
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			if (customMercData != null && customMercData.Number > 0)
			{
				int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
				int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
				int numOfTroopPlayerCanBuy = (troopRecruitmentCost == 0) ? customMercData.Number : Hero.MainHero.Gold / troopRecruitmentCost;
				if (numOfTroopSlotsOpen > 0 && Hero.MainHero.Gold >= troopRecruitmentCost && numOfTroopSlotsOpen < customMercData.Number && numOfTroopSlotsOpen < numOfTroopPlayerCanBuy)
				{
					int numOfMercs = Math.Min(customMercData.Number, numOfTroopPlayerCanBuy);
					numOfMercs = Math.Min(numOfTroopSlotsOpen, numOfMercs);
					MBTextManager.SetTextVariable("C_MEN_COUNT_PL", numOfMercs);
					MBTextManager.SetTextVariable("C_MERCENARY_NAME_PL", customMercData.TroopInfoCharObject().Name);
					MBTextManager.SetTextVariable("C_TOTAL_AMOUNT_PL", numOfMercs * troopRecruitmentCost);
					args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
					return true;
				}
			}
			return false;
		}

		private void HireCustomMecenariesViaGameMenu(bool buyingOne, bool toPartyLimit)
		{
			if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return;
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			if (customMercData == null) return;
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
			if (customMercData.Number > 0 && Hero.MainHero.Gold >= troopRecruitmentCost && (!toPartyLimit || numOfTroopSlotsOpen > 0))
			{
				int numOfMercs = 1;
				if (!buyingOne)
				{
					int numOfTroopPlayerCanBuy = (troopRecruitmentCost == 0) ? customMercData.Number : Hero.MainHero.Gold / troopRecruitmentCost;
					numOfMercs = Math.Min(customMercData.Number, numOfTroopPlayerCanBuy);
					if (toPartyLimit) numOfMercs = Math.Min(numOfTroopSlotsOpen, numOfMercs);
				}
				MobileParty.MainParty.MemberRoster.AddToCounts(customMercData.TroopInfoCharObject(), numOfMercs, false, 0, 0, true, -1);
				GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, -(numOfMercs * troopRecruitmentCost), false);
				customMercData.ChangeMercenaryCount(-numOfMercs);
				GameMenu.SwitchToMenu("town_backstreet");
			}
		}
	}
}

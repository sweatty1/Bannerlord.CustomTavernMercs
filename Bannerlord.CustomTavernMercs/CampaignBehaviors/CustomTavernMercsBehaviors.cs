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
			CampaignEvents.OnNewGameCreatedEvent8.AddNonSerializedListener(this, new Action(this.OnAfterNewGameCreated));
			CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.OnSettlementEntered));
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
			if (Settlement.CurrentSettlement != null && !Hero.MainHero.IsPrisoner)
			{
				AddCustomMercenaryCharacterToTavern(Settlement.CurrentSettlement);
			}
		}

		// Only triggers on new campaigns created
		public void OnAfterNewGameCreated()
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

		// Adding Character to the Tavern
		private void AddCustomMercenaryCharacterToTavern(Settlement settlement)
		{
			if (settlement.LocationComplex != null && settlement.IsTown && custom_merc_data_holder.dictionaryOfMercAtTownData[settlement.Town].HasAvailableMercenary(Occupation.NotAssigned))
			{
				Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern");
				if (locationWithId != null)
				{
					locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(this.CreateCustomMercenary), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
				}
			}
		}

		private LocationCharacter CreateCustomMercenary(CultureObject culture, LocationCharacter.CharacterRelations relation)
		{
			Settlement currentSettlement = MobileParty.MainParty.CurrentSettlement;
			return new LocationCharacter(new AgentData(new SimpleAgentOrigin(custom_merc_data_holder.dictionaryOfMercAtTownData[currentSettlement.Town].TroopInfoCharObject(), -1, null, default(UniqueTroopDescriptor))).Monster(Campaign.Current.HumanMonsterSettlement).NoHorses(true), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddOutdoorWandererBehaviors), "npc_common", true, relation, null, false, false, null, false, false, true);
		}

		private void DoesCustomMercenaryCharacterNeedRefresh(Settlement settlement, CharacterObject oldTroopType)
		{
			if (settlement.IsTown && settlement == Settlement.CurrentSettlement && PlayerEncounter.LocationEncounter != null && settlement.LocationComplex != null && (CampaignMission.Current == null || GameStateManager.Current.ActiveState != CampaignMission.Current.State))
			{
				Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern").RemoveAllCharacters((LocationCharacter x) => (x.Character.Occupation == oldTroopType.Occupation && x.Character.Name == oldTroopType.Name));
				AddCustomMercenaryCharacterToTavern(settlement);
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
			CharacterObject oldTroopType = custom_merc_data_holder.dictionaryOfMercAtTownData[town].TroopInfoCharObject();
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
			if (oldTroopType != null)
			{
				DoesCustomMercenaryCharacterNeedRefresh(town.Settlement, oldTroopType);
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
			campaignGameStarter.AddDialogLine("custom_merc_recruit_talk_start_plural", "start", "custom_merc_mercenary_tavern_talk", "Do you have a need for fighters, {?PLAYER.GENDER}madam{?}sir{\\?}? Me and {?CMERCS_PLURAL}{CMERCS_MERCENARY_COUNT} of my mates{?}one of my mates{\\?} looking for a master. You might call us mercenaries, like. We'll join you for {CMERCS_GOLD_AMOUNT}{GOLD_ICON}", new ConversationSentence.OnConditionDelegate(this.conversation_custom_mercenary_recruit_plural_start_on_condition), null, 150, null);
			campaignGameStarter.AddDialogLine("custom_merc_recruit_talk_start_singlular", "start", "custom_merc_mercenary_tavern_talk", "Do you have a need for fighters, {?PLAYER.GENDER}madam{?}sir{\\?}? I am looking for a master. I'll join you for {CMERCS_GOLD_AMOUNT}{GOLD_ICON}", new ConversationSentence.OnConditionDelegate(this.conversation_custom_mercenary_recruit_single_start_on_condition), null, 150, null);
			campaignGameStarter.AddPlayerLine("custom_merc_recruit_talk_hire_one", "custom_merc_mercenary_tavern_talk", "custom_merc_mercenary_tavern_talk_hire_one", "All right. I would only like to hire one of you. Here is {CMERCS_GOLD_AMOUNT_FOR_ONE}{GOLD_ICON}", new ConversationSentence.OnConditionDelegate(this.conversation_custom_mercenary_recruit_one), new ConversationSentence.OnConsequenceDelegate(this.conversation_custom_mercenary_recruit_one_on_consequence), 110, null, null);
			campaignGameStarter.AddDialogLine("custom_merc_recruit_talk_hire_one_response", "custom_merc_mercenary_tavern_talk_hire_one", "custom_merc_mercenary_tavern_talk", "Deal, One of us will report to your party outside the gates after gathering their gear. Need anything else?", null, null, 100, null);
			campaignGameStarter.AddPlayerLine("custom_merc_recruit_talk_hire_all", "custom_merc_mercenary_tavern_talk", "custom_merc_mercenary_tavern_talk_hire", "All right. I will hire {?CMERCS_PLURAL}all of you{?}you{\\?}. Here is {CMERCS_GOLD_AMOUNT_ALL}{GOLD_ICON}", new ConversationSentence.OnConditionDelegate(this.conversation_custom_mercenary_recruit_accept_all_on_condition), new ConversationSentence.OnConsequenceDelegate(this.conversation_custom_mercenary_recruit_accept_all_on_consequence), 100, null, null);
			campaignGameStarter.AddPlayerLine("custom_merc_recruit_talk_hire_all_past_limit", "custom_merc_mercenary_tavern_talk", "custom_merc_mercenary_tavern_talk_hire", "All right. I will hire {?CMERCS_PLURAL}all of you{?}you{\\?}. Here is {CMERCS_GOLD_AMOUNT_ALL}{GOLD_ICON} (Hires Past Party Limit)", new ConversationSentence.OnConditionDelegate(this.conversation_custom_mercenary_recruit_accept_all_on_condition_past_limit), new ConversationSentence.OnConsequenceDelegate(this.conversation_custom_mercenary_recruit_accept_all_on_consequence), 110, null, null);
			campaignGameStarter.AddPlayerLine("custom_merc_recruit_talk_hire_some_past_limit", "custom_merc_mercenary_tavern_talk", "custom_merc_mercenary_tavern_talk_hire", "All right. But I can only hire {CMERCS_MERCENARY_COUNT_SOME_AFFORD} of you. Here is {CMERCS_GOLD_AMOUNT_SOME_AFFORD}{GOLD_ICON} (Hires Past Party Limit)", new ConversationSentence.OnConditionDelegate(this.conversation_custom_mercenary_recruit_accept_some_on_condition_past_limit_afford), new ConversationSentence.OnConsequenceDelegate(this.conversation_custom_mercenary_recruit_accept_some_past_limit_on_consequence), 110, null, null);
			campaignGameStarter.AddPlayerLine("custom_merc_recruit_talk_hire_some", "custom_merc_mercenary_tavern_talk", "custom_merc_mercenary_tavern_talk_hire", "All right. But I can only hire {CMERCS_MERCENARY_COUNT_SOME} of you. Here is {CMERCS_GOLD_AMOUNT_SOME}{GOLD_ICON}", new ConversationSentence.OnConditionDelegate(this.conversation_custom_mercenary_recruit_accept_some_on_condition), new ConversationSentence.OnConsequenceDelegate(this.conversation_custom_mercenary_recruit_accept_some_on_consequence), 100, null, null);
			campaignGameStarter.AddPlayerLine("custom_merc_recruit_talk_reject_no_gold", "custom_merc_mercenary_tavern_talk", "close_window", "That sounds good. But I can't hire any more men right now.", new ConversationSentence.OnConditionDelegate(this.conversation_custom_mercenary_recruit_reject_gold_or_party_size_on_condition), null, 100, null, null);
			campaignGameStarter.AddPlayerLine("custom_merc_recruit_talk_reject_party_full", "custom_merc_mercenary_tavern_talk", "close_window", "Sorry. I don't need any other men right now.", new ConversationSentence.OnConditionDelegate(this.conversation_custom_mercenary_recruit_dont_need_men_on_condition), null, 100, null, null);
			campaignGameStarter.AddDialogLine("custom_merc_recruit_talk_hired_end", "custom_merc_mercenary_tavern_talk_hire", "close_window", "{RANDOM_HIRE_SENTENCE}", new ConversationSentence.OnConditionDelegate(this.conversation_custom_mercenary_recruit_end_on_condition), null, 100, null);
			campaignGameStarter.AddDialogLine("custom_merc_recruit_talk_start_post_hire", "start", "close_window", "Don't worry, I'll be ready. Just having a last drink for the road.", new ConversationSentence.OnConditionDelegate(this.conversation_custom_mercenary_recruited_on_condition), null, 150, null);
		}

		private bool CustomMercIsInTavern(CustomMercData customMercData)
		{
			if (CampaignMission.Current == null || CampaignMission.Current.Location == null || customMercData.TroopInfo == null || customMercData.TroopInfoCharObject() == null)
			{
				return false;
			}
			return CampaignMission.Current.Location.StringId == "tavern" && customMercData.TroopInfoCharObject().Name == CharacterObject.OneToOneConversationCharacter.Name && CharacterObject.OneToOneConversationCharacter.IsSoldier;
		}

		// Conditions for starting line dialog
		private bool conversation_custom_mercenary_recruit_plural_start_on_condition()
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

		private bool conversation_custom_mercenary_recruit_single_start_on_condition()
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

		private bool conversation_custom_mercenary_recruited_on_condition()
		{
			if (MobileParty.MainParty.CurrentSettlement == null || !MobileParty.MainParty.CurrentSettlement.IsTown) return false;
			CustomMercData mercData = GetCustomMercDataOfPlayerEncounter();
			return CustomMercIsInTavern(mercData);
		}

		// Conditions for Hiring options and functions that follow
		private bool conversation_custom_mercenary_recruit_one()
		{
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
			int numOfTroopPlayerCanBuy = Hero.MainHero.Gold / troopRecruitmentCost;
			MBTextManager.SetTextVariable("CMERCS_GOLD_AMOUNT_FOR_ONE", troopRecruitmentCost);
			return 1 < customMercData.Number && numOfTroopPlayerCanBuy > 1;
		}

		private void conversation_custom_mercenary_recruit_one_on_consequence()
		{
			BuyCustomMercenariesInTavern(1);
		}

		private bool conversation_custom_mercenary_recruit_accept_all_on_condition()
		{
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			MBTextManager.SetTextVariable("CMERCS_PLURAL", (customMercData.Number > 1) ? 1 : 0);
			MBTextManager.SetTextVariable("CMERCS_GOLD_AMOUNT_ALL", troopRecruitmentCost * customMercData.Number);
			return Hero.MainHero.Gold >= customMercData.Number * troopRecruitmentCost && numOfTroopSlotsOpen >= customMercData.Number;
		}

		private bool conversation_custom_mercenary_recruit_accept_all_on_condition_past_limit()
		{
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
			int numOfTroopPlayerCanBuy = (troopRecruitmentCost==0) ? customMercData.Number : Hero.MainHero.Gold / troopRecruitmentCost;
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			MBTextManager.SetTextVariable("CMERCS_PLURAL", (customMercData.Number > 1) ? 1 : 0);
			MBTextManager.SetTextVariable("CMERCS_GOLD_AMOUNT_ALL", troopRecruitmentCost * numOfTroopPlayerCanBuy);
			return numOfTroopSlotsOpen < customMercData.Number && numOfTroopPlayerCanBuy >= customMercData.Number;
		}

		private void conversation_custom_mercenary_recruit_accept_all_on_consequence()
		{
			BuyCustomMercenariesInTavern(GetCustomMercDataOfPlayerEncounter().Number);
		}

		private bool conversation_custom_mercenary_recruit_accept_some_on_condition()
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

		private void conversation_custom_mercenary_recruit_accept_some_on_consequence()
		{
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			int numberToHire = 0;
			while (Hero.MainHero.Gold > troopRecruitmentCost * (numberToHire + 1) && numOfTroopSlotsOpen > numberToHire)
			{
				numberToHire++;
			}
			BuyCustomMercenariesInTavern(numberToHire);
		}

		private bool conversation_custom_mercenary_recruit_accept_some_on_condition_past_limit_afford()
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
				if (numberToHire <= numOfTroopSlotsOpen)
				{
					return false;
				}
				MBTextManager.SetTextVariable("CMERCS_MERCENARY_COUNT_SOME_AFFORD", numberToHire);
				MBTextManager.SetTextVariable("CMERCS_GOLD_AMOUNT_SOME_AFFORD", troopRecruitmentCost * numberToHire);
				return true;
			}
			return false;
		}

		private void conversation_custom_mercenary_recruit_accept_some_past_limit_on_consequence()
		{
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
			int numberToHire = 0;
			while (Hero.MainHero.Gold > troopRecruitmentCost * (numberToHire + 1) && customMercData.Number > numberToHire)
			{
				numberToHire++;
			}
			BuyCustomMercenariesInTavern(numberToHire);
		}

		private void BuyCustomMercenariesInTavern(int numberOfMercsToHire)
		{
			CustomMercData customMercData = GetCustomMercDataOfPlayerEncounter();
			customMercData.ChangeMercenaryCount(-numberOfMercsToHire);
			int troopRecruitmentCost = this.troopRecruitmentCost(customMercData);
			MobileParty.MainParty.AddElementToMemberRoster(customMercData.TroopInfoCharObject(), numberOfMercsToHire, false);
			int amount = numberOfMercsToHire * troopRecruitmentCost;
			GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, amount, false);
			CampaignEventDispatcher.Instance.OnUnitRecruited(customMercData.TroopInfoCharObject(), numberOfMercsToHire);
		}

		// Conditions to trigger reject hiring options
		private bool conversation_custom_mercenary_recruit_reject_gold_or_party_size_on_condition()
		{
			int troopRecruitmentCost = this.troopRecruitmentCost(GetCustomMercDataOfPlayerEncounter());
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			return Hero.MainHero.Gold < troopRecruitmentCost || numOfTroopSlotsOpen <= 0;
		}

		private bool conversation_custom_mercenary_recruit_dont_need_men_on_condition()
		{
			int troopRecruitmentCost = this.troopRecruitmentCost(GetCustomMercDataOfPlayerEncounter());
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			return Hero.MainHero.Gold >= troopRecruitmentCost && numOfTroopSlotsOpen > 0;
		}

		// Successful hire npc phrase
		public bool conversation_custom_mercenary_recruit_end_on_condition()
		{
			MBTextManager.SetTextVariable("RANDOM_HIRE_SENTENCE", GameTexts.FindText("str_mercenary_tavern_talk_hire", MBRandom.RandomInt(4).ToString()));
			return true;
		}

		// GAME MENU CODE
		//Interaction of the Tavern from the Game Menu tested to work on 1.4.1
		// these variables have these names otherwise if say MEN_COUNT would override the 1.4.1 Game Menu for normal mercs
		public void AddGameMenus(CampaignGameStarter campaignGameStarter)
		{
			// index is location in menu 0 being top, 1 next if other of same index exist this are placed on top of them
			campaignGameStarter.AddGameMenuOption("town_backstreet", "recruit_custom_mercenaries_all", "{=*}Recruit {C_MEN_COUNT} {C_MERCENARY_NAME} ({C_TOTAL_AMOUNT}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(BuyCustomMercsViaMenuCondition), delegate (MenuCallbackArgs x)
			{
				BuyCustomMecenariesViaGameMenu(false, false);
			}, false, 1, false);
			campaignGameStarter.AddGameMenuOption("town_backstreet", "recruit_custom_mercenaries_party_limit", "{=*}Recruit to Party Limit {C_MEN_COUNT_PL} {C_MERCENARY_NAME_PL} ({C_TOTAL_AMOUNT_PL}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(BuyCustomMercsViaMenuConditionToPartyLimit), delegate (MenuCallbackArgs x)
			{
				BuyCustomMecenariesViaGameMenu(false, true);
			}, false, 1, false);
			campaignGameStarter.AddGameMenuOption("town_backstreet", "recruit_custom_mercenaries_hire_one", "{=*}Recruit 1 {C_MERCENARY_NAME_ONLY_ONE} ({C_TOTAL_AMOUNT_ONLY_ONE}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(BuyCustomMercsViaMenuConditionHireOne), delegate (MenuCallbackArgs x)
			{
				BuyCustomMecenariesViaGameMenu(true, false);
			}, false, 1, false);
		}

		private bool BuyCustomMercsViaMenuConditionHireOne(MenuCallbackArgs args)
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

		private bool BuyCustomMercsViaMenuCondition(MenuCallbackArgs args)
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

		private bool BuyCustomMercsViaMenuConditionToPartyLimit(MenuCallbackArgs args)
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

		private void BuyCustomMecenariesViaGameMenu(bool buyingOne, bool toPartyLimit)
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

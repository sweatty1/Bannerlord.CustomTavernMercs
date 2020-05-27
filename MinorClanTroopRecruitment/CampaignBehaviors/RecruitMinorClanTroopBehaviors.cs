using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.CampaignSystem.GameMenus;
using System.Collections.Generic;

namespace MinorClanTroopRecruitment
{
	internal class RecruitMinorClanTroopBehaviors : CampaignBehaviorBase
    {
		public override void SyncData(IDataStore dataStore) { }

		public MinorClanMercDataHolder mc_merc_data = null;

		public override void RegisterEvents()
		{
			CampaignEvents.OnNewGameCreatedEvent2.AddNonSerializedListener(this, new Action(this.OnAfterNewGameCreated));
			CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, new Action<Town>(this.DailyTickTown));
			CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.OnSettlementEntered));
			CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnGameLoaded));
			CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
		}

		// Only triggers on loaded games
		private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
		{
			if (mc_merc_data == null)
			{
				MinorClanMercDataHolder clanMercData = new MinorClanMercDataHolder();
				this.mc_merc_data = clanMercData;
				foreach (Town town in Town.AllTowns)
				{
					this.UpdateCurrentMercenaryTroopAndCount(town);
				}
			}
			// Add Character if inside of town
			if (Settlement.CurrentSettlement != null && !Hero.MainHero.IsPrisoner && LocationComplex.Current != null)
			{
				this.AddMinorClanMercenaryCharacterToTavern(Settlement.CurrentSettlement);
			}
		}

		// Only triggers on new campaigns created
		public void OnAfterNewGameCreated()
		{
			if (mc_merc_data == null)
			{
				MinorClanMercDataHolder clanMercData = new MinorClanMercDataHolder();
				this.mc_merc_data = clanMercData;
				foreach (Town town in Town.AllTowns)
				{
					this.UpdateCurrentMercenaryTroopAndCount(town);
				}
			}
		}

		public void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
		{
			if (mobileParty != MobileParty.MainParty)
			{
				return;
			}
			this.AddMinorClanMercenaryCharacterToTavern(settlement);
		}

		// Adding Character to the Tavern
		private void AddMinorClanMercenaryCharacterToTavern(Settlement settlement)
		{

			if (settlement.LocationComplex != null && settlement.IsTown && mc_merc_data.dictionaryOfMercAtTownData[settlement.Town].HasAvailableMercenary(Occupation.NotAssigned))
			{
				Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern");
				if (locationWithId != null)
				{
					locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(this.CreateMinorClanMercenary), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
				}
			}
		}

		private LocationCharacter CreateMinorClanMercenary(CultureObject culture, LocationCharacter.CharacterRelations relation)
		{
			return new LocationCharacter(new AgentData(new SimpleAgentOrigin(mc_merc_data.dictionaryOfMercAtTownData[PlayerEncounter.Settlement.Town].TroopType, -1, null, default(UniqueTroopDescriptor))).Monster(Campaign.Current.HumanMonsterSettlement).NoHorses(true), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddOutdoorWandererBehaviors), "spawnpoint_mercenary", true, relation, null, false, false, null, false, false, true);
		}

		private void CheckIfMinorClanMercenaryCharacterNeedsToRefresh(Settlement settlement, CharacterObject oldTroopType)
		{
			if (settlement.IsTown && settlement == Settlement.CurrentSettlement && PlayerEncounter.LocationEncounter != null && settlement.LocationComplex != null && (CampaignMission.Current == null || GameStateManager.Current.ActiveState != CampaignMission.Current.State))
			{
				Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern").RemoveAllCharacters((LocationCharacter x) => (x.Character.Occupation == oldTroopType.Occupation && x.Character.Name == oldTroopType.Name));
				this.AddMinorClanMercenaryCharacterToTavern(settlement);
			}
		}

		// Update minorMerc troops
		private void DailyTickTown(Town town)
		{
			this.UpdateCurrentMercenaryTroopAndCount(town);
		}
		

		private static int FindNumberOfMercenariesWillBeAdded(BasicCharacterObject character)
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
			CharacterObject oldTroopType = mc_merc_data.dictionaryOfMercAtTownData[town].TroopType;
			List<Clan> possibleClans = mc_merc_data.dictionaryOfMercAtTownData[town].PossibleClans;
			int r = MBRandom.Random.Next(possibleClans.Count);
			string basicTroopId = possibleClans[r].BasicTroop.StringId;
			CharacterObject basicTroopObject = Game.Current.ObjectManager.GetObject<CharacterObject>(basicTroopId.ToString());
			int numbOfUnits = FindNumberOfMercenariesWillBeAdded(basicTroopObject);
			if (MBRandom.RandomFloat > Settings.Settings.Instance.PossibilityOfSpawn)
			{
				numbOfUnits = 0;
			}
			mc_merc_data.dictionaryOfMercAtTownData[town].ChangeMercenaryType(basicTroopObject, numbOfUnits);

			// Since we don't have access to MercenaryNUmberChangedInTown or MercenaryTroopChangedInTown
			// need way to trigger spawn of hire guy in tavern when inside of town on a daily update
			if(oldTroopType != null)
			{
				CheckIfMinorClanMercenaryCharacterNeedsToRefresh(town.Settlement, oldTroopType);
			}
		}

		// start of the dialog and game Menu code flows
		public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
		{
			this.AddDialogs(campaignGameStarter);
			this.AddGameMenus(campaignGameStarter);
		}

		private MinorClanMercData getMinorMercDataOfPlayerEncounter()
		{
			return mc_merc_data.dictionaryOfMercAtTownData[PlayerEncounter.Settlement.Town];
		}

		private int troopRecruitmentCost(CharacterObject troopType)
		{
			float recruitCostMultiplier = Settings.Settings.Instance.RecruitCostMultiplier;
			int baseCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(troopType, Hero.MainHero, false);
			return MBRandom.RoundRandomized(baseCost * recruitCostMultiplier);
		}


		// TAVERN CODE
		protected void AddDialogs(CampaignGameStarter campaignGameStarter)
		{
			// priority = higher takes presidence if equal first one added if condition isn't met will pass to next priority
			// priority also makes it so that if two prompts are present on that the higher one is higher on the list
			// not that start is start token for all converstaions and it goes down the priority to see the first start that returns true for the ConversationSentence.OnConditionDelegate 
			campaignGameStarter.AddDialogLine("minor_clan_recruit_talk_start_plural", "start", "minor_clan_mercenary_tavern_talk", "Do you have a need for fighters, {?PLAYER.GENDER}madam{?}sir{\\?}? Me and {?PLURAL}{MERCENARY_COUNT} of my mates{?}one of my mates{\\?} looking for a master. You might call us mercenaries, like. We'll join you for {GOLD_AMOUNT}{GOLD_ICON}", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruit_plural_start_on_condition), null, 150, null);
			campaignGameStarter.AddDialogLine("minor_clan_recruit_talk_start_singlular", "start", "minor_clan_mercenary_tavern_talk", "Do you have a need for fighters, {?PLAYER.GENDER}madam{?}sir{\\?}? I am looking for a master. I'll join you for {GOLD_AMOUNT}{GOLD_ICON}", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruit_single_start_on_condition), null, 150, null);
			campaignGameStarter.AddPlayerLine("minor_clan_recruit_talk_hire_all", "minor_clan_mercenary_tavern_talk", "minor_clan_mercenary_tavern_talk_hire", "All right. I will hire {?PLURAL}all of you{?}you{\\?}. Here is {GOLD_AMOUNT}{GOLD_ICON}", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruit_accept_all_on_condition), new ConversationSentence.OnConsequenceDelegate(this.conversation_minor_clan_mercenary_recruit_accept_all_on_consequence), 100, null, null);
			campaignGameStarter.AddPlayerLine("minor_clan_recruit_talk_hire_all_past_limit", "minor_clan_mercenary_tavern_talk", "minor_clan_mercenary_tavern_talk_hire", "All right. I will hire {?PLURAL}all of you{?}you{\\?}. Here is {GOLD_AMOUNT}{GOLD_ICON} (Hires Past Party Limit)", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruit_accept_all_on_condition_past_limit), new ConversationSentence.OnConsequenceDelegate(this.conversation_minor_clan_mercenary_recruit_accept_all_on_consequence), 110, null, null);
			campaignGameStarter.AddPlayerLine("minor_clan_recruit_talk_hire_some_past_limit", "minor_clan_mercenary_tavern_talk", "minor_clan_mercenary_tavern_talk_hire", "All right. But I can only hire {MERCENARY_COUNT_SOME_AFFORD} of you. Here is {GOLD_AMOUNT_SOME_AFFORD}{GOLD_ICON} (Hires Past Party Limit)", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruit_accept_some_on_condition_past_limit_afford), new ConversationSentence.OnConsequenceDelegate(this.conversation_minor_clan_mercenary_recruit_accept_some_past_limit_on_consequence), 110, null, null);
			campaignGameStarter.AddPlayerLine("minor_clan_recruit_talk_hire_some", "minor_clan_mercenary_tavern_talk", "minor_clan_mercenary_tavern_talk_hire", "All right. But I can only hire {MERCENARY_COUNT_SOME} of you. Here is {GOLD_AMOUNT_SOME}{GOLD_ICON}", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruit_accept_some_on_condition), new ConversationSentence.OnConsequenceDelegate(this.conversation_minor_clan_mercenary_recruit_accept_some_on_consequence), 100, null, null);
			campaignGameStarter.AddPlayerLine("minor_clan_recruit_talk_reject_no_gold", "minor_clan_mercenary_tavern_talk", "close_window", "That sounds good. But I can't hire any more men right now.", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruit_reject_gold_or_party_size_on_condition), null, 100, null, null);
			campaignGameStarter.AddPlayerLine("minor_clan_recruit_talk_reject_party_full", "minor_clan_mercenary_tavern_talk", "close_window", "Sorry. I don't need any other men right now.", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruit_dont_need_men_on_condition), null, 100, null, null);
			campaignGameStarter.AddDialogLine("minor_clan_recruit_talk_hired_end", "minor_clan_mercenary_tavern_talk_hire", "close_window", "{RANDOM_HIRE_SENTENCE}", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruit_end_on_condition), null, 100, null);
			campaignGameStarter.AddDialogLine("minor_clan_recruit_talk_start_post_hire", "start", "close_window", "Don't worry, I'll be ready. Just having a last drink for the road.", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruited_on_condition), null, 150, null);
		}

		private bool minorClanMercGuardIsInTavern(MinorClanMercData minorMercData)
		{
			if (CampaignMission.Current == null || CampaignMission.Current.Location == null || minorMercData.TroopType == null)
			{
				return false;
			}
			return CampaignMission.Current.Location.StringId == "tavern" && minorMercData.TroopType.Name == CharacterObject.OneToOneConversationCharacter.Name && CharacterObject.OneToOneConversationCharacter.IsSoldier;
		}

		// Conditions for starting line dialog
		private bool conversation_minor_clan_mercenary_recruit_plural_start_on_condition()
		{
			if(PlayerEncounter.Settlement == null || !PlayerEncounter.Settlement.IsTown)
			{
				return false;
			}
			MinorClanMercData minorMercData = getMinorMercDataOfPlayerEncounter();
			bool flag = minorMercData.Number > 1 && minorClanMercGuardIsInTavern(minorMercData);
			if (flag)
			{
				int troopRecruitmentCost = this.troopRecruitmentCost(minorMercData.TroopType);
				MBTextManager.SetTextVariable("PLURAL", (minorMercData.Number > 1) ? 1 : 0, false);
				MBTextManager.SetTextVariable("MERCENARY_COUNT", minorMercData.Number - 1, false);
				MBTextManager.SetTextVariable("GOLD_AMOUNT", troopRecruitmentCost * minorMercData.Number, false);
			}
			return flag;
		}

		private bool conversation_minor_clan_mercenary_recruit_single_start_on_condition()
		{
			if (PlayerEncounter.Settlement == null || !PlayerEncounter.Settlement.IsTown)
			{
				return false;
			}
			MinorClanMercData minorMercData = getMinorMercDataOfPlayerEncounter();
			bool flag = minorMercData.Number == 1 && minorClanMercGuardIsInTavern(minorMercData);
			if (flag)
			{
				int troopRecruitmentCost = this.troopRecruitmentCost(minorMercData.TroopType);
				MBTextManager.SetTextVariable("GOLD_AMOUNT", minorMercData.Number * troopRecruitmentCost, false);
			}
			return flag;
		}

		private bool conversation_minor_clan_mercenary_recruited_on_condition()
		{
			if (PlayerEncounter.Settlement == null || !PlayerEncounter.Settlement.IsTown)
			{
				return false;
			}
			MinorClanMercData minorMercData = getMinorMercDataOfPlayerEncounter();
			return minorClanMercGuardIsInTavern(minorMercData);
		}

		// Conditions for Hiring options and functions that follow
		private bool conversation_minor_clan_mercenary_recruit_accept_all_on_condition()
		{
			MinorClanMercData minorMercData = getMinorMercDataOfPlayerEncounter();
			int troopRecruitmentCost = this.troopRecruitmentCost(minorMercData.TroopType);
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			MBTextManager.SetTextVariable("PLURAL", (minorMercData.Number > 1) ? 1 : 0, false);
			return Hero.MainHero.Gold >= minorMercData.Number * troopRecruitmentCost && numOfTroopSlotsOpen >= minorMercData.Number;
		}

		private bool conversation_minor_clan_mercenary_recruit_accept_all_on_condition_past_limit()
		{
			MinorClanMercData minorMercData = getMinorMercDataOfPlayerEncounter();
			int troopRecruitmentCost = this.troopRecruitmentCost(minorMercData.TroopType);
			int numOfTroopPlayerCanBuy = Hero.MainHero.Gold / troopRecruitmentCost;
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			MBTextManager.SetTextVariable("PLURAL", (minorMercData.Number > 1) ? 1 : 0, false);
			return numOfTroopSlotsOpen < minorMercData.Number && numOfTroopPlayerCanBuy >= minorMercData.Number;
		}

		private void conversation_minor_clan_mercenary_recruit_accept_all_on_consequence()
		{
			this.BuyMinorClanMercenariesInTavern(getMinorMercDataOfPlayerEncounter().Number);
		}

		private bool conversation_minor_clan_mercenary_recruit_accept_some_on_condition()
		{
			MinorClanMercData minorMercData = getMinorMercDataOfPlayerEncounter();
			int troopRecruitmentCost = this.troopRecruitmentCost(minorMercData.TroopType);
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			if (Hero.MainHero.Gold >= troopRecruitmentCost && numOfTroopSlotsOpen > 0 && (Hero.MainHero.Gold < minorMercData.Number * troopRecruitmentCost || numOfTroopSlotsOpen < minorMercData.Number))
			{
				int numberToHire = 0;
				while (Hero.MainHero.Gold > troopRecruitmentCost * (numberToHire + 1) && numOfTroopSlotsOpen > numberToHire)
				{
					numberToHire++;
				}
				MBTextManager.SetTextVariable("MERCENARY_COUNT_SOME", numberToHire, false);
				MBTextManager.SetTextVariable("GOLD_AMOUNT_SOME", troopRecruitmentCost * numberToHire, false);
				return true;
			}
			return false;
		}

		private void conversation_minor_clan_mercenary_recruit_accept_some_on_consequence()
		{
			MinorClanMercData minorMercData = getMinorMercDataOfPlayerEncounter();
			int troopRecruitmentCost = this.troopRecruitmentCost(minorMercData.TroopType);
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			int numberToHire = 0;
			while (Hero.MainHero.Gold > troopRecruitmentCost * (numberToHire + 1) && numOfTroopSlotsOpen > numberToHire)
			{
				numberToHire++;
			}
			this.BuyMinorClanMercenariesInTavern(numberToHire);
		}

		private bool conversation_minor_clan_mercenary_recruit_accept_some_on_condition_past_limit_afford()
		{
			MinorClanMercData minorMercData = getMinorMercDataOfPlayerEncounter();
			int troopRecruitmentCost = this.troopRecruitmentCost(minorMercData.TroopType);
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			if (Hero.MainHero.Gold >= troopRecruitmentCost && Hero.MainHero.Gold < minorMercData.Number * troopRecruitmentCost)
			{
				int numberToHire = 0;
				while (Hero.MainHero.Gold > troopRecruitmentCost * (numberToHire + 1) && minorMercData.Number > numberToHire)
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

		private void conversation_minor_clan_mercenary_recruit_accept_some_past_limit_on_consequence()
		{
			MinorClanMercData minorMercData = getMinorMercDataOfPlayerEncounter();
			int troopRecruitmentCost = this.troopRecruitmentCost(minorMercData.TroopType);
			int numberToHire = 0;
			while (Hero.MainHero.Gold > troopRecruitmentCost * (numberToHire + 1) && minorMercData.Number > numberToHire)
			{
				numberToHire++;
			}
			this.BuyMinorClanMercenariesInTavern(numberToHire);
		}

		private void BuyMinorClanMercenariesInTavern(int numberOfMercsToHire)
		{
			MinorClanMercData minorMercData = getMinorMercDataOfPlayerEncounter();
			minorMercData.ChangeMercenaryCount(-numberOfMercsToHire);
			int troopRecruitmentCost = this.troopRecruitmentCost(minorMercData.TroopType);
			MobileParty.MainParty.AddElementToMemberRoster(minorMercData.TroopType, numberOfMercsToHire, false);
			int amount = numberOfMercsToHire * troopRecruitmentCost;
			GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, amount, false);
			CampaignEventDispatcher.Instance.OnUnitRecruited(minorMercData.TroopType, numberOfMercsToHire);
		}

		// Conditions to trigger reject hiring options
		private bool conversation_minor_clan_mercenary_recruit_reject_gold_or_party_size_on_condition()
		{
			int troopRecruitmentCost = this.troopRecruitmentCost(getMinorMercDataOfPlayerEncounter().TroopType);
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			return Hero.MainHero.Gold < troopRecruitmentCost || numOfTroopSlotsOpen <= 0;
		}

		private bool conversation_minor_clan_mercenary_recruit_dont_need_men_on_condition()
		{
			int troopRecruitmentCost = this.troopRecruitmentCost(getMinorMercDataOfPlayerEncounter().TroopType);
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			return Hero.MainHero.Gold >= troopRecruitmentCost && numOfTroopSlotsOpen > 0;
		}

		// Successful hire npc phrase
		public bool conversation_minor_clan_mercenary_recruit_end_on_condition()
		{
			MBTextManager.SetTextVariable("RANDOM_HIRE_SENTENCE", GameTexts.FindText("str_mercenary_tavern_talk_hire", MBRandom.RandomInt(4).ToString()), false);
			return true;
		}

		// GAME MENU CODE
		//Interaction of the Tavern from the Game Menu tested to work on 1.4.1
		// these variables have these names otherwise if say MEN_COUNT would override the 1.4.1 Game Menu for normal mercs
		public void AddGameMenus(CampaignGameStarter campaignGameStarter)
		{
			// index is location in menu 0 being top, 1 next if other of same index exist this are placed on top of them
			campaignGameStarter.AddGameMenuOption("town_backstreet", "recruit_minor_clan_mercenaries_all", "{=*}Recruit {MC_MEN_COUNT} {MC_MERCENARY_NAME} ({MC_TOTAL_AMOUNT}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(this.BuyMinorClanMercsViaMenuCondition), delegate (MenuCallbackArgs x)
			{
				BuyMinorClanMercenariesViaGameMenu(mc_merc_data.dictionaryOfMercAtTownData[MobileParty.MainParty.CurrentSettlement.Town]);
			}, false, 1, false);
			campaignGameStarter.AddGameMenuOption("town_backstreet", "recruit_minor_clan_mercenaries_party_limit", "{=*}Recruit to Party Limit {MC_MEN_COUNT_PL} {MC_MERCENARY_NAME_PL} ({MC_TOTAL_AMOUNT_PL}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(this.BuyMinorClanMercsViaMenuConditionToPartyLimit), delegate (MenuCallbackArgs x)
			{
				BuyMinorClanMercenariesViaGameMenuToPartyLimit(mc_merc_data.dictionaryOfMercAtTownData[MobileParty.MainParty.CurrentSettlement.Town]);
			}, false, 1, false);
		}

		private bool BuyMinorClanMercsViaMenuCondition(MenuCallbackArgs args)
		{
			MinorClanMercData minorMercData = mc_merc_data.dictionaryOfMercAtTownData[MobileParty.MainParty.CurrentSettlement.Town];
			if (MobileParty.MainParty.CurrentSettlement != null && MobileParty.MainParty.CurrentSettlement.IsTown && minorMercData != null && minorMercData.Number > 0)
			{
				int troopRecruitmentCost = this.troopRecruitmentCost(minorMercData.TroopType);
				if (Hero.MainHero.Gold >= troopRecruitmentCost)
				{
					int num = Math.Min(minorMercData.Number, Hero.MainHero.Gold / troopRecruitmentCost);
					MBTextManager.SetTextVariable("MC_MEN_COUNT", num, false);
					MBTextManager.SetTextVariable("MC_MERCENARY_NAME", minorMercData.TroopType.Name, false);
					MBTextManager.SetTextVariable("MC_TOTAL_AMOUNT", num * troopRecruitmentCost, false);
					args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
					return true;
				}
			}
			return false;
		}

		private static void BuyMinorClanMercenariesViaGameMenu(MinorClanMercData minorMercData)
		{
			if (MobileParty.MainParty.CurrentSettlement != null && MobileParty.MainParty.CurrentSettlement.IsTown && minorMercData != null && minorMercData.Number > 0)
			{
				int troopRecruitmentCost = MBRandom.RoundRandomized(Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(minorMercData.TroopType, null, false) * Settings.Settings.Instance.RecruitCostMultiplier);
				if (Hero.MainHero.Gold >= troopRecruitmentCost)
				{
					int numOfMercs = Math.Min(minorMercData.Number, Hero.MainHero.Gold / troopRecruitmentCost);
					MobileParty.MainParty.MemberRoster.AddToCounts(minorMercData.TroopType, numOfMercs, false, 0, 0, true, -1);
					GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, -(numOfMercs * troopRecruitmentCost), false);
					minorMercData.ChangeMercenaryCount(-numOfMercs);
					GameMenu.SwitchToMenu("town_backstreet");
				}
			}
		}
		private bool BuyMinorClanMercsViaMenuConditionToPartyLimit(MenuCallbackArgs args)
		{
			MinorClanMercData minorMercData = mc_merc_data.dictionaryOfMercAtTownData[MobileParty.MainParty.CurrentSettlement.Town];
			if (MobileParty.MainParty.CurrentSettlement != null && MobileParty.MainParty.CurrentSettlement.IsTown && minorMercData != null && minorMercData.Number > 0)
			{
				int troopRecruitmentCost = this.troopRecruitmentCost(minorMercData.TroopType);
				int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
				int numOfTroopPlayerCanBuy = Hero.MainHero.Gold / troopRecruitmentCost;
				if (numOfTroopSlotsOpen > 0 && Hero.MainHero.Gold >= troopRecruitmentCost && numOfTroopSlotsOpen < minorMercData.Number && numOfTroopSlotsOpen < numOfTroopPlayerCanBuy)
				{
					int numOfMercs = Math.Min(minorMercData.Number, numOfTroopPlayerCanBuy);
					numOfMercs = Math.Min(numOfTroopSlotsOpen, numOfMercs);
					MBTextManager.SetTextVariable("MC_MEN_COUNT_PL", numOfMercs, false);
					MBTextManager.SetTextVariable("MC_MERCENARY_NAME_PL", minorMercData.TroopType.Name, false);
					MBTextManager.SetTextVariable("MC_TOTAL_AMOUNT_PL", numOfMercs * troopRecruitmentCost, false);
					args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
					return true;
				}
			}
			return false;
		}

		private static void BuyMinorClanMercenariesViaGameMenuToPartyLimit(MinorClanMercData minorMercData)
		{
			int numOfTroopSlotsOpen = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			if (MobileParty.MainParty.CurrentSettlement != null && MobileParty.MainParty.CurrentSettlement.IsTown && minorMercData != null && minorMercData.Number > 0 && numOfTroopSlotsOpen > 0)
			{
				int troopRecruitmentCost = MBRandom.RoundRandomized(Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(minorMercData.TroopType, null, false) * Settings.Settings.Instance.RecruitCostMultiplier);
				if (Hero.MainHero.Gold >= troopRecruitmentCost)
				{
					int numOfMercs = Math.Min(minorMercData.Number, Hero.MainHero.Gold / troopRecruitmentCost);
					numOfMercs = Math.Min(numOfTroopSlotsOpen, numOfMercs);
					MobileParty.MainParty.MemberRoster.AddToCounts(minorMercData.TroopType, numOfMercs, false, 0, 0, true, -1);
					GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, -(numOfMercs * troopRecruitmentCost), false);
					minorMercData.ChangeMercenaryCount(-numOfMercs);
					GameMenu.SwitchToMenu("town_backstreet");
				}
			}
		}
	}
}

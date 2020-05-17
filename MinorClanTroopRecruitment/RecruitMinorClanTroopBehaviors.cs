using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.CampaignSystem.GameMenus;

namespace MinorClanTroopRecruitment
{
    internal class RecruitMinorClanTroopBehaviors : CampaignBehaviorBase
    {
		public MinorClanMercDataHolder mc_merc_data = null;

		public override void RegisterEvents()
		{
			CampaignEvents.OnNewGameCreatedEvent2.AddNonSerializedListener(this, new Action(this.OnAfterNewGameCreated));
			CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, new Action<Town>(this.DailyTickTown));
			CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.OnSettlementEntered));
			CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnGameLoaded));
			CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
		}

		private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
		{
			if (mc_merc_data == null)
			{
				MinorClanMercDataHolder clanMercData = new MinorClanMercDataHolder();
				this.mc_merc_data = clanMercData;
			}
			if (Settlement.CurrentSettlement != null && !Hero.MainHero.IsPrisoner && LocationComplex.Current != null)
			{
				this.AddMinorClanMercenaryCharacterToTavern(Settlement.CurrentSettlement);
			}
		}

		public override void SyncData(IDataStore dataStore) { }

		public void OnAfterNewGameCreated()
		{
			if (mc_merc_data == null)
			{
				MinorClanMercDataHolder clanMercData = new MinorClanMercDataHolder();
				this.mc_merc_data = clanMercData;
			}
			foreach (Town town in Town.AllTowns)
			{
				this.UpdateCurrentMercenaryTroopAndCount(town, true);
			}
		}

		private void DailyTickTown(Town town)
		{
			this.UpdateCurrentMercenaryTroopAndCount(town, (int)CampaignTime.Now.ToDays % 2 == 0);
		}

		private static int FindNumberOfMercenariesWillBeAdded(BasicCharacterObject character, bool dailyUpdate = false)
		{
			int level = character.Level;
			int num = MBRandom.RoundRandomized((float)(100.0 / Math.Sqrt((double)level)));
			float randomFloat = MBRandom.RandomFloat;
			float randomFloat2 = MBRandom.RandomFloat;
			float num2 = randomFloat * randomFloat2 * (float)num;
			num2 = ((num2 > 15f) ? 15f : num2);
			num2 = ((num2 < 1f) ? 1f : num2);
			num2 *= (dailyUpdate ? 0.1f : 1f);
			return MBRandom.RoundRandomized(num2);
		}

		private void UpdateCurrentMercenaryTroopAndCount(Town town, bool forceUpdate = false)
		{
			// TODO Make it basied off town culture
			int r = MBRandom.Random.Next(mc_merc_data.minorClanList.Count);
			var basicTroopId = mc_merc_data.minorClanList[r].BasicTroop.StringId;
			CharacterObject basicTroopObject = Game.Current.ObjectManager.GetObject<CharacterObject>(basicTroopId.ToString());
			int numbOfUnits = FindNumberOfMercenariesWillBeAdded(basicTroopObject, false);
			mc_merc_data.dictionaryOfMercAtTownData[town].ChangeMercenaryType(basicTroopObject, numbOfUnits);
			// InformationManager.DisplayMessage(new InformationMessage($"Updated town: {town.Name} with troop {basicTroopId} count of {numbOfUnits} from clan: {mc_merc_data.minorClanList[r].Name}"));
		}

		public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
		{
			this.AddDialogs(campaignGameStarter);
			this.AddGameMenus(campaignGameStarter);
		}

		public void AddGameMenus(CampaignGameStarter campaignGameStarter)
		{
			campaignGameStarter.AddGameMenuOption("town_backstreet", "recruit_minor_clan_mercenaries", "{=*}Recruit {MC_MEN_COUNT} {MC_MERCENARY_NAME} ({MC_TOTAL_AMOUNT}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(this.BuyMinorClanMercsViaMenuCondition), delegate (MenuCallbackArgs x)
			{
				BuyMinorClanMercenariesViaGameMenu(mc_merc_data.dictionaryOfMercAtTownData[MobileParty.MainParty.CurrentSettlement.Town]);
			}, false, -1, false);
		}

		public void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
		{
			if (mobileParty != MobileParty.MainParty)
			{
				return;
			}
			this.AddMinorClanMercenaryCharacterToTavern(settlement);
		}

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

		protected void AddDialogs(CampaignGameStarter campaignGameStarter)
		{
			//guard is the what the soldiers are considered inside of the tavern so useing a are we in tavern start branch for converstation
			//campaignGameStarter.AddDialogLine("guard_start", "start", "close_window", "I DRINK BEER", new ConversationSentence.OnConditionDelegate(this.minormercTavernTalk), null, 150, null);
			// priority = higher takes presidence if equal first one added if condition isn't met will pass to next priority
			// priority also makes it so that if two prompts are present on that the higher one is higher on the list
			// campaignGameStarter.AddDialogLine("guard_start", "start", "minor_clan_mercenary_talk_start", "You want da bois", new ConversationSentence.OnConditionDelegate(this.minormercTavernTalk), null, 150, null);
			campaignGameStarter.AddDialogLine("guard_start", "start", "minor_clan_mercenary_tavern_talk", "Do you have a need for fighters, {?PLAYER.GENDER}madam{?}sir{\\?}? Me and {?PLURAL}{MERCENARY_COUNT} of my mates{?}one of my mates{\\?} looking for a master. You might call us mercenaries, like. We'll join you for {GOLD_AMOUNT}{GOLD_ICON}", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruit_plural_start_on_condition), null, 150, null);
			campaignGameStarter.AddDialogLine("guard_start", "start", "minor_clan_mercenary_tavern_talk", "Do you have a need for fighters, {?PLAYER.GENDER}madam{?}sir{\\?}? I am looking for a master. I'll join you for {GOLD_AMOUNT}{GOLD_ICON}", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruit_single_start_on_condition), null, 150, null);
			campaignGameStarter.AddPlayerLine("mercenary_recruit_accept_mod", "minor_clan_mercenary_tavern_talk", "minor_clan_mercenary_tavern_talk_hire", "All right. I will hire {?PLURAL}all of you{?}you{\\?}. Here is {GOLD_AMOUNT}{GOLD_ICON}", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruit_accept_on_condition), new ConversationSentence.OnConsequenceDelegate(this.conversation_minor_clan_mercenary_recruit_accept_on_consequence), 100, null, null);
			campaignGameStarter.AddPlayerLine("mercenary_recruit_accept_some_mod", "minor_clan_mercenary_tavern_talk", "minor_clan_mercenary_tavern_talk_hire", "All right. But I can only hire {MERCENARY_COUNT} of you. Here is {GOLD_AMOUNT}{GOLD_ICON}", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruit_accept_some_on_condition), new ConversationSentence.OnConsequenceDelegate(this.conversation_minor_clan_mercenary_recruit_accept_some_on_consequence), 100, null, null);
			campaignGameStarter.AddPlayerLine("mercenary_recruit_reject_gold_mod", "minor_clan_mercenary_tavern_talk", "close_window", "That sounds good. But I can't hire any more men right now.", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruit_reject_gold_or_party_size_on_condition), null, 100, null, null);
			campaignGameStarter.AddPlayerLine("mercenary_recruit_reject_mod", "minor_clan_mercenary_tavern_talk", "close_window", "Sorry. I don't need any other men right now.", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruit_dont_need_men_on_condition), null, 100, null, null);
			campaignGameStarter.AddDialogLine("mercenary_recruit_end_mod", "minor_clan_mercenary_tavern_talk_hire", "close_window", "{RANDOM_HIRE_SENTENCE}", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruit_end_on_condition), null, 100, null);
			campaignGameStarter.AddDialogLine("guard_start", "start", "close_window", "Don't worry, I'll be ready. Just having a last drink for the road.", new ConversationSentence.OnConditionDelegate(this.conversation_minor_clan_mercenary_recruited_on_condition), null, 150, null);
		}
		private bool minorClanMercGuardIsInTavern()
		{
			return CampaignMission.Current.Location.StringId == "tavern";
		}
		private MinorClanMercData getMinorMercDataOfPlayerEncounter()
		{
			return mc_merc_data.dictionaryOfMercAtTownData[PlayerEncounter.Settlement.Town];
		}

		private bool conversation_minor_clan_mercenary_recruit_plural_start_on_condition()
		{
			if(PlayerEncounter.Settlement == null || !PlayerEncounter.Settlement.IsTown)
			{
				return false;
			}
			MinorClanMercData minorMercData = getMinorMercDataOfPlayerEncounter();
			bool flag = minorMercData.Number > 1 && minorClanMercGuardIsInTavern() && CharacterObject.OneToOneConversationCharacter.IsSoldier;
			if (flag)
			{
				int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(minorMercData.TroopType, Hero.MainHero, false);
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
			bool flag = minorMercData.Number == 1 && minorClanMercGuardIsInTavern() && CharacterObject.OneToOneConversationCharacter.IsSoldier;
			if (flag)
			{
				int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(minorMercData.TroopType, Hero.MainHero, false);
				MBTextManager.SetTextVariable("GOLD_AMOUNT", minorMercData.Number * troopRecruitmentCost, false);
			}
			return flag;
		}

		private bool conversation_minor_clan_mercenary_recruit_accept_on_condition()
		{
			MinorClanMercData minorMercData = getMinorMercDataOfPlayerEncounter();
			int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(minorMercData.TroopType, Hero.MainHero, false);
			int num = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			MBTextManager.SetTextVariable("PLURAL", (minorMercData.Number > 1) ? 1 : 0, false);
			return Hero.MainHero.Gold >= minorMercData.Number * troopRecruitmentCost && num >= minorMercData.Number;
		}

		private bool conversation_minor_clan_mercenary_recruited_on_condition()
		{
			return CharacterObject.OneToOneConversationCharacter.IsSoldier && PlayerEncounter.Settlement != null && minorClanMercGuardIsInTavern();
		}

		private void BuyMinorClanMercenariesInTavern()
		{

			MinorClanMercData minorMercData = getMinorMercDataOfPlayerEncounter();
			minorMercData.ChangeMercenaryCount(-this._selectedMinorClanMercCount);
			int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(minorMercData.TroopType, Hero.MainHero, false);
			MobileParty.MainParty.AddElementToMemberRoster(minorMercData.TroopType, this._selectedMinorClanMercCount, false);
			int amount = this._selectedMinorClanMercCount * troopRecruitmentCost;
			GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, amount, false);
			CampaignEventDispatcher.Instance.OnUnitRecruited(minorMercData.TroopType, this._selectedMinorClanMercCount);
		}
		private void conversation_minor_clan_mercenary_recruit_accept_on_consequence()
		{
			this._selectedMinorClanMercCount = getMinorMercDataOfPlayerEncounter().Number;
			this.BuyMinorClanMercenariesInTavern();
		}

		private bool conversation_minor_clan_mercenary_recruit_accept_some_on_condition()
		{
			int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(getMinorMercDataOfPlayerEncounter().TroopType, Hero.MainHero, false);
			int num = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			if (Hero.MainHero.Gold >= troopRecruitmentCost && num > 0 && (Hero.MainHero.Gold < getMinorMercDataOfPlayerEncounter().Number * troopRecruitmentCost || num < getMinorMercDataOfPlayerEncounter().Number))
			{
				this._selectedMinorClanMercCount = 0;
				while (Hero.MainHero.Gold > troopRecruitmentCost * (this._selectedMinorClanMercCount + 1) && num > this._selectedMinorClanMercCount)
				{
					this._selectedMinorClanMercCount++;
				}
				MBTextManager.SetTextVariable("MERCENARY_COUNT", this._selectedMinorClanMercCount, false);
				MBTextManager.SetTextVariable("GOLD_AMOUNT", troopRecruitmentCost * this._selectedMinorClanMercCount, false);
				return true;
			}
			return false;
		}

		private void conversation_minor_clan_mercenary_recruit_accept_some_on_consequence()
		{
			this.BuyMinorClanMercenariesInTavern();
		}
		private bool conversation_minor_clan_mercenary_recruit_reject_gold_or_party_size_on_condition()
		{
			int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(getMinorMercDataOfPlayerEncounter().TroopType, Hero.MainHero, false);
			int num = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			return Hero.MainHero.Gold < troopRecruitmentCost || num <= 0;
		}

		private bool conversation_minor_clan_mercenary_recruit_dont_need_men_on_condition()
		{
			int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(getMinorMercDataOfPlayerEncounter().TroopType, Hero.MainHero, false);
			int num = PartyBase.MainParty.PartySizeLimit - PartyBase.MainParty.NumberOfAllMembers;
			return Hero.MainHero.Gold >= troopRecruitmentCost && num > 0;
		}

		public bool conversation_minor_clan_mercenary_recruit_end_on_condition()
		{
			MBTextManager.SetTextVariable("RANDOM_HIRE_SENTENCE", GameTexts.FindText("str_mercenary_tavern_talk_hire", MBRandom.RandomInt(4).ToString()), false);
			return true;
		}

		private int _selectedMinorClanMercCount;

		//Interaction of the Tavern from the Game Menu tested to work on 1.4.1
		// these variables have these names otherwise if say MEN_COUNT would override the 1.4.1 Game Menu for normal mercs
		private bool BuyMinorClanMercsViaMenuCondition(MenuCallbackArgs args)
		{
			MinorClanMercData minorMercData = mc_merc_data.dictionaryOfMercAtTownData[MobileParty.MainParty.CurrentSettlement.Town];
			if (MobileParty.MainParty.CurrentSettlement != null && MobileParty.MainParty.CurrentSettlement.IsTown && minorMercData != null && minorMercData.Number > 0)
			{
				int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(minorMercData.TroopType, null, false);
				if (Hero.MainHero.Gold >= troopRecruitmentCost)
				{
					int num = Math.Min(minorMercData.Number, Hero.MainHero.Gold / troopRecruitmentCost);
					MBTextManager.SetTextVariable("MC_MEN_COUNT", num, false);
					MBTextManager.SetTextVariable("MC_MERCENARY_NAME", minorMercData.TroopType.Name, false);
					MBTextManager.SetTextVariable("MC_TOTAL_AMOUNT", num * troopRecruitmentCost, false);
					args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
					return true;
				}
				int number = minorMercData.Number;
				MBTextManager.SetTextVariable("MC_MEN_COUNT", number, false);
				MBTextManager.SetTextVariable("MC_MERCENARY_NAME", minorMercData.TroopType.Name, false);
			}
			return false;
		}

		private static void BuyMinorClanMercenariesViaGameMenu(MinorClanMercData minorMercData)
		{
			if (MobileParty.MainParty.CurrentSettlement != null && MobileParty.MainParty.CurrentSettlement.IsTown && minorMercData != null && minorMercData.Number > 0)
			{
				int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(minorMercData.TroopType, null, false);
				if (Hero.MainHero.Gold >= troopRecruitmentCost)
				{
					int num = Math.Min(minorMercData.Number, Hero.MainHero.Gold / troopRecruitmentCost);
					MobileParty.MainParty.MemberRoster.AddToCounts(minorMercData.TroopType, num, false, 0, 0, true, -1);
					GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, -(num * troopRecruitmentCost), false);
					minorMercData.ChangeMercenaryCount(-num);
					GameMenu.SwitchToMenu("town_backstreet");
				}
			}
		}
	}
}

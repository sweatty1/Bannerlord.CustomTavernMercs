using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using StoryMode;

namespace MinorClanTroopRecruitment
{
    public class MinorClanTroopRecruitmentSubModule : MBSubModuleBase
    {
        public override void OnCampaignStart(Game game, object gameStarterObject)
        {
            base.OnCampaignStart(game, gameStarterObject);
            Campaign campaign = game.GameType as Campaign;
            if (game.GameType is Campaign)
            {
                CampaignGameStarter gameInitializer = (CampaignGameStarter)gameStarterObject;
                AddBehaviors(gameInitializer);
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if(game.GameType is Campaign && ((CampaignStoryMode)game.GameType).CampaignGameLoadingType != Campaign.GameLoadingType.NewCampaign)
            {
                Campaign campaign = game.GameType as Campaign;
                CampaignGameStarter gameInitializer = (CampaignGameStarter)gameStarterObject;
                AddBehaviors(gameInitializer);
            }
        }

        private void AddBehaviors(CampaignGameStarter gameInitializer)
        {
            gameInitializer.AddBehavior(new RecruitMinorClanTroopBehaviors());
        }
    }
}

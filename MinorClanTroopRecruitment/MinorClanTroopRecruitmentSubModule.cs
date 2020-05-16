using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MinorClanTroopRecruitment
{
    public class MinorClanTroopRecruitmentSubModule : MBSubModuleBase
    {
        protected override void OnGameStart(Game game, IGameStarter gameStartedObject)
        {
            Campaign campaign = game.GameType as Campaign;
            CampaignGameStarter gameInitializer = (CampaignGameStarter)gameStartedObject;
            AddBehaviors(gameInitializer);
        }
        private void AddBehaviors(CampaignGameStarter gameInitializer)
        {
            gameInitializer.AddBehavior(new RecruitMinorClanTroopBehaviors());
        }
    }
}

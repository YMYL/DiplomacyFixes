﻿using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;

namespace DiplomacyFixes.CampaignEventBehaviors
{
    public class WarExhaustionBehavior : CampaignBehaviorBase
    {
        private WarExhaustionManager _warExhaustionManager;

        public WarExhaustionBehavior()
        {
            _warExhaustionManager = new WarExhaustionManager();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.SiegeCompletedEvent.AddNonSerializedListener(this, SiegeCompleted);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, MapEventEnded);
        }

        private void MapEventEnded(MapEvent mapEvent)
        {
            if (!Settings.Instance.EnableWarExhaustion)
            {
                return;
            }

            Kingdom attackerSide = mapEvent.AttackerSide.LeaderParty.MapFaction as Kingdom;
            Kingdom defenderSide = mapEvent.DefenderSide.LeaderParty.MapFaction as Kingdom;

            if (attackerSide == null || defenderSide == null)
            {
                return;
            }

            _warExhaustionManager.AddCasualtyWarExhaustion(attackerSide, defenderSide, mapEvent.AttackerSide.Casualties);
            _warExhaustionManager.AddCasualtyWarExhaustion(defenderSide, attackerSide, mapEvent.DefenderSide.Casualties);
        }

        private void OnDailyTick()
        {
            if (!Settings.Instance.EnableWarExhaustion)
            {
                return;
            }

            _warExhaustionManager.UpdateDailyWarExhaustionForAllKingdoms();

            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (PlayerHelpers.IsPlayerLeaderOfFaction(kingdom))
                {
                    continue;
                }
                ConsiderPeaceActions(kingdom);
            }
        }

        private void SiegeCompleted(Settlement settlement, MobileParty mobileParty, bool isWin, bool isSiege)
        {
            if (!Settings.Instance.EnableWarExhaustion)
            {
                return;
            }

            if (!(isWin && isSiege))
            {
                return;
            }

            Kingdom previousOwner = settlement.OwnerClan.MapFaction as Kingdom;
            Kingdom newOwner = mobileParty.MapFaction as Kingdom;

            if (previousOwner != null && newOwner != null && previousOwner != newOwner)
            {
                _warExhaustionManager.AddSiegeWarExhaustion(previousOwner, newOwner);
            }
        }

        private void ConsiderPeaceActions(Kingdom kingdom)
        {
            foreach (Kingdom targetKingdom in FactionManager.GetEnemyKingdoms(kingdom))
            {
                if (_warExhaustionManager.HasMaxWarExhaustion(kingdom, targetKingdom) && IsValidQuestState(kingdom, targetKingdom))
                {
                    KingdomPeaceAction.ApplyPeaceDueToWarExhaustion(kingdom, targetKingdom);
                }
            }
        }

        private bool IsValidQuestState(Kingdom kingdom1, Kingdom kingdom2)
        {
            bool isValidQuestState = true;
            Kingdom opposingKingdom = PlayerHelpers.GetOpposingKingdomIfPlayerKingdomProvided(kingdom1, kingdom2);
            if (opposingKingdom != null)
            {
                ThirdPhase thirdPhase = StoryMode.StoryMode.Current.MainStoryLine.ThirdPhase;
                isValidQuestState = thirdPhase == null || !thirdPhase.OppositionKingdoms.Contains(opposingKingdom);
            }
            return isValidQuestState;
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_warExhaustionManager", ref _warExhaustionManager);
        }
    }
}

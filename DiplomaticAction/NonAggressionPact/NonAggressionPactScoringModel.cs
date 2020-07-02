﻿using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DiplomacyFixes.DiplomaticAction.NonAggressionPact
{
    class NonAggressionPactScoringModel
    {
        public static float FormNonAggressionPactThreshold { get; } = 100.0f;

        private static readonly TextObject _weakFaction = new TextObject("{=q5qphBwi}Weak Faction");
        private static readonly TextObject _relationship = new TextObject("{=sygtLRqA}Relationship");

        public static ExplainedNumber GetFormNonAggressionPactScore(Kingdom kingdom, Kingdom otherKingdom, StatExplainer explanation = null)
        {
            ExplainedNumber explainedNumber = new ExplainedNumber((float)NonAggressionScore.Base, explanation, null);
            float medianStrength = GetMedianStrength();

            // weak faction bonus
            if (kingdom.TotalStrength <= medianStrength)
            {
                explainedNumber.Add((float)NonAggressionScore.BelowMedianStrength, _weakFaction);
            }

            // common enemies
            IEnumerable<Kingdom> commonEnemies = FactionManager.GetEnemyKingdoms(kingdom).Intersect(FactionManager.GetEnemyKingdoms(otherKingdom));
            foreach (Kingdom commonEnemy in commonEnemies)
            {
                TextObject textObject = null;
                if (explanation != null)
                {
                    textObject = new TextObject("{=RqQ4oqvl}War with {ENEMY_KINGDOM}");
                    textObject.SetTextVariable("ENEMY_KINGDOM", commonEnemy.Name);
                }
                explainedNumber.Add((int)NonAggressionScore.HasCommonEnemy, textObject);
            }

            IEnumerable<Kingdom> alliedEnemies = Kingdom.All.Except(new[] { kingdom, otherKingdom }).Where(curKingdom => FactionManager.IsAlliedWithFaction(otherKingdom, curKingdom) && FactionManager.IsAtWarAgainstFaction(kingdom, curKingdom));
            IEnumerable<Kingdom> alliedNeutrals = Kingdom.All.Except(new[] { kingdom, otherKingdom }).Where(curKingdom => FactionManager.IsAlliedWithFaction(otherKingdom, curKingdom) && !FactionManager.IsAtWarAgainstFaction(kingdom, curKingdom));
            foreach (Kingdom alliedEnemy in alliedEnemies)
            {
                TextObject textObject = null;
                if (explanation != null)
                {
                    textObject = new TextObject("{=cmOSpfyW}Alliance with {ALLIED_KINGDOM}");
                    textObject.SetTextVariable("ALLIED_KINGDOM", alliedEnemy.Name);
                }
                explainedNumber.Add((int)NonAggressionScore.ExistingAllianceWithEnemy, textObject);
            }
            foreach (Kingdom alliedNeutral in alliedNeutrals)
            {
                TextObject textObject = null;
                if (explanation != null)
                {
                    textObject = new TextObject("{=cmOSpfyW}Alliance with {ALLIED_KINGDOM}");
                    textObject.SetTextVariable("ALLIED_KINGDOM", alliedNeutral.Name);
                }
                explainedNumber.Add((int)NonAggressionScore.ExistingAllianceWithNeutral, textObject);
            }

            // relation modifier
            float relationModifier = MBMath.ClampFloat((float)Math.Log((kingdom.Leader.GetRelation(otherKingdom.Leader) + 100f) / 100f, 1.5), -1, 1);
            explainedNumber.Add((int)NonAggressionScore.Relationship * relationModifier, _relationship);

            return explainedNumber;
        }

        public static bool ShouldFormNonAggressionPact(Kingdom kingdom, Kingdom otherKingdom)
        {
            return GetFormNonAggressionPactScore(kingdom, otherKingdom).ResultNumber >= FormNonAggressionPactThreshold && GetFormNonAggressionPactScore(otherKingdom, kingdom).ResultNumber >= FormNonAggressionPactThreshold;
        }

        private static float GetMedianStrength()
        {
            float medianStrength;
            float[] kingdomStrengths = Kingdom.All.Select(curKingdom => curKingdom.TotalStrength).OrderBy(a => a).ToArray();

            int halfIndex = kingdomStrengths.Count() / 2;

            if (kingdomStrengths.Length % 2 == 0)
            {
                medianStrength = (kingdomStrengths.ElementAt(halfIndex) + kingdomStrengths.ElementAt(halfIndex - 1)) / 2;
            }
            else
            {
                medianStrength = kingdomStrengths.ElementAt(halfIndex);
            }
            return medianStrength;
        }

        private enum NonAggressionScore : int
        {
            Base = 50,
            BelowMedianStrength = 50,
            HasCommonEnemy = 50,
            ExistingAllianceWithNeutral = -50,
            ExistingAllianceWithEnemy = -1000,
            Relationship = 50
        }
    }
}
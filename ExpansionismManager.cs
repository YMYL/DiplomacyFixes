﻿
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace DiplomacyFixes
{
    [SaveableClass(8)]
    class ExpansionismManager
    {
        [SaveableField(1)]
        private Dictionary<IFaction, float> _expansionism;

        public static ExpansionismManager Instance { get; private set; }
        public float SiegeExpansionism { get { return 20f; } }
        public float ExpansionismDecayPerDay { get { return 1f; } }

        public ExpansionismManager()
        {
            this._expansionism = new Dictionary<IFaction, float>();
            Instance = this;
        }

        public float GetExpansionism(IFaction faction)
        {
            return this._expansionism.TryGetValue(faction, out float result) ? result : 0f;
        }

        public void AddSiegeScore(IFaction faction)
        {
            this._expansionism.TryGetValue(faction, out float value);
            this._expansionism[faction] = value + SiegeExpansionism;
        }

        public void ApplyExpansionismDecay(IFaction faction)
        {
            if(this._expansionism.TryGetValue(faction, out float value))
            {
                this._expansionism[faction] = value - ExpansionismDecayPerDay;
            }
        }

        internal void Sync()
        {
            Instance = this;
        }
    }
}
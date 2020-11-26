using System;
using FFXIVAPP.Plugin.TeastParse.Models;

namespace FFXIVAPP.Plugin.TeastParse.Actors.Potency
{
    /// <summary>
    /// Base class for keeping track on potency
    /// </summary>
    internal abstract class PotencyBase
    {
        private ulong DamagePerPotency;
        private ulong DamagePerPotencyCount;
        private ulong DamageCritPerPotency;
        private ulong DamageCritPerPotencyCount;
        private ulong DamageDirectPerPotency;
        private ulong DamageDirectPerPotencyCount;
        private ulong DamageDirectCritPerPotency;
        private ulong DamageDirectCritPerPotencyCount;

        private ulong NrOfHits;
        private ulong NrOfCrits;
        private ulong NrOfDirect;
        private ulong NrOfDirectAndCrits;

        public virtual void StoreDamage(DamageModel model)
        {
            NrOfHits++;
            Action<ulong> Potency;
            Action IncCount;

            if (model.Critical && model.DirectHit)
            {
                NrOfCrits++;
                NrOfDirect++;
                NrOfDirectAndCrits++;
                Potency = val => DamageDirectCritPerPotency += val;
                IncCount = () => DamageDirectCritPerPotencyCount++;
            }
            else if (model.Critical)
            {
                NrOfCrits++;
                Potency = val => DamageCritPerPotency += val;
                IncCount = () => DamageCritPerPotencyCount++;
            }
            else if (model.DirectHit)
            {
                NrOfDirect++;
                Potency = val => DamageDirectPerPotency += val;
                IncCount = () => DamageDirectPerPotencyCount++;
            }
            else
            {
                Potency = val => DamagePerPotency += val;
                IncCount = () => DamagePerPotencyCount++;
            }

            Potency(model.Damage / (ulong)model.Potency);
            IncCount();
        }

        /// <summary>
        /// Damage per potency (no critical nor direct hit included)
        /// </summary>
        public double DPP {
            get {
                var count = Math.Max(DamagePerPotencyCount, 1);
                return (double)DamagePerPotency / count;
            }
        }

        /// <summary>
        /// Get critical change percent
        /// </summary>
        public double CriticalChance => ((double)NrOfCrits / Math.Max(NrOfHits, 1)) * 100;

        public string Debug()
        {
            var dpp = (double)DamagePerPotency / DamagePerPotencyCount;
            var dcpp = (double)DamageCritPerPotency / DamageCritPerPotencyCount;
            var dcdpp = (double)DamageDirectCritPerPotency / DamageDirectCritPerPotencyCount;
            var ddpp = (double)DamageDirectPerPotency / DamageDirectPerPotencyCount;
            return $"DPP: {dpp}, DCPP: {dcpp}, DCDPP: {dcdpp}, DDPP: {ddpp}, hits: {NrOfHits}, crits: {NrOfCrits}, direct: {NrOfDirect}, crits+direct: {NrOfDirectAndCrits}";
        }
    }
}
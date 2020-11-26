using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVAPP.Common.Utilities;
using FFXIVAPP.Plugin.TeastParse.Models;
using NLog;

namespace FFXIVAPP.Plugin.TeastParse.Actors.Potency
{
    internal class ActorPotencyFacade
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Random _rnd = new Random();
        private readonly PotencyAttack _attack;
        private readonly PotencyAutoAttack _autoAttack;
        private readonly PotencyDetrimental _detrimental;

        private readonly Dictionary<PotencyRequest, PotencyRequestValue> _cache;
        private double _oldAADPP = 0;
        private double _oldADPP = 0;
        private double _oldCritical;

        public ActorPotencyFacade()
        {
            _attack = new PotencyAttack();
            _autoAttack = new PotencyAutoAttack();
            _detrimental = new PotencyDetrimental();
            _cache = new Dictionary<PotencyRequest, PotencyRequestValue>();
        }

        public void StoreDamageDetails(DamageModel model)
        {
            if (model.IsDetrimental)
                _detrimental.StoreDamage(model);
            else if (string.IsNullOrEmpty(model.ActionName))
                _autoAttack.StoreDamage(model);
            else
                _attack.StoreDamage(model);
        }

        public string Debug()
        {
            return $"Auto-Attack: {_autoAttack.Debug()}, Attack: {_attack.Debug()}";
        }

        internal bool HasAutoAttack => !double.IsNaN(_autoAttack.DPP) && _autoAttack.DPP > 0.0;

        internal int GetDetrimentalPotency()
        {
            var aadpp = _autoAttack.DPP;
            var adpp = _attack.DPP;
            if (double.IsNaN(aadpp) || aadpp == 0.0)
                return (int)(adpp);
            if (double.IsNaN(adpp) || adpp == 0.0)
                return (int)(aadpp);

            return (int)((aadpp + adpp) / 2);
        }

        internal bool IsCritical()
        {
            var chance = _attack.CriticalChance;
            var nr = _rnd.Next(0, 100);

            return (nr <= chance);
        }

        internal double DetrimentalDamage(int ticks, int potency)
        {
            //#if DEBUG
            //var dstr = "[";
            //#endif

            var pr = new PotencyRequest(ticks, potency);
            var hasCache = _cache.TryGetValue(pr, out var cacheVal);
            if (hasCache && cacheVal.Requests < 5)
            {
                Logging.Log(Logger, $"Using pre-cache version for {ticks},{potency} == {cacheVal.Value} ({cacheVal.Requests})");
                cacheVal.Requests++;
                _cache[pr] = cacheVal;
                return cacheVal.Value;
            }

            // Cache so we do not do calculations all the time if we dont have to...
            if (hasCache && _attack.CriticalChance == _oldCritical && _autoAttack.DPP == _oldAADPP && _attack.DPP == _oldADPP)
            {
                cacheVal.Requests = 0;
                _cache[pr] = cacheVal;
                Logging.Log(Logger, $"Using cache version for {ticks},{potency} == {cacheVal.Value}");
                return cacheVal.Value;
            }

            _oldCritical = _attack.CriticalChance;
            _oldAADPP = _autoAttack.DPP;
            _oldADPP = _attack.DPP;

            var dpp = GetDetrimentalPotency();
            var damages = new double[ticks];

            var total = damages.Select(t =>
            {
                var chance = IsCritical();
                var crit = chance ? 1.4 : 1.0;

                // TODO: Just a number I camed up with to get up the dot damage...
                var modif = (double)1 + ((double)_rnd.Next(10, 28) / 100);

                if (!HasAutoAttack)
                    modif = 1.0;

                var vall = potency * dpp * crit * modif;
                //#if DEBUG
                //dstr += $"{vall}({crit}|{modif})|";
                //#endif
                return vall;
            }).Sum();

            //#if DEBUG
            //Logging.Log(Logger, $"DetrimentalDamage: ({dpp}) {dstr}]");
            //#endif

            if (!hasCache)
                cacheVal = new PotencyRequestValue(total);
            else
                cacheVal.Value = total;
                
            cacheVal.Requests = 0;
            _cache[pr] = cacheVal;
            Logging.Log(Logger, $"Using no-cache version for {ticks},{potency} == {total}");
            return total;
        }

        private struct PotencyRequestValue
        {
            public int Requests { get; set; }
            public double Value { get; set; }

            public PotencyRequestValue(double value)
            {
                Value = value;
                Requests = 0;
            }
        }

        private struct PotencyRequest
        {
            public int Ticks { get; }
            public int Potency { get; }

            public PotencyRequest(int ticks, int potency)
            {
                Ticks = ticks;
                Potency = potency;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                if (!typeof(PotencyRequest).IsInstanceOfType(obj))
                    return false;
                var b = (PotencyRequest)obj;
                return (b.Ticks == Ticks && b.Potency == Potency);
            }

            public override int GetHashCode()
            {
                var hash = 7879;
                var modifier = 271;
                hash += Ticks.GetHashCode() * modifier;
                hash += Potency.GetHashCode() * modifier;
                return hash;
            }
        }
    }
}
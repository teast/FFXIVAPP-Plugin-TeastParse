using System;
using FFXIVAPP.Plugin.TeastParse.Models;

namespace FFXIVAPP.Plugin.TeastParse.Actors.Potency
{
    internal class ActorPotencyFacade
    {
        private Random _rnd = new Random();
        private readonly PotencyAttack _attack;
        private readonly PotencyAutoAttack _autoAttack;
        private readonly PotencyDetrimental _detrimental;

        public ActorPotencyFacade()
        {
            _attack = new PotencyAttack();
            _autoAttack = new PotencyAutoAttack();
            _detrimental = new PotencyDetrimental();
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
    }
}
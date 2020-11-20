namespace FFXIVAPP.Plugin.TeastParse.Models
{
    public class ActionModel
    {
        public string Name { get; }
        public int Potency { get; }
        public bool IsCombo { get; }
        public ActionCategory Category { get; }
        public ActionModel(string name, ActionCategory category, int potency = 0, bool isCombo = false)
        {
            Name = name;
            Potency = potency;
            IsCombo = isCombo;
            Category = category;
        }
    }

    public enum ActionCategory
    {
        Event,
        System,
        Item,
        AutoAttack,
        Weaponskill,
        Ability,
        Spell,
        LimitBreak,
        DoLAbility,
        DoHAbility,
        Artillery,
        Special,
        Mount,
        AdrenalineRush
    }
}
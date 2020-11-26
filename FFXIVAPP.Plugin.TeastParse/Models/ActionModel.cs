namespace FFXIVAPP.Plugin.TeastParse.Models
{
    public class ActionModel
    {
        public string Name { get; }
        /// <summary>
        /// EnglishName is needed to match combos
        /// </summary>
        public string EnglishName { get; }
        public int Potency { get; }
        public bool IsCombo { get; }
        public ActionCategory Category { get; }
        public int Duration { get; }
        public ActionModel(string name, string englishName, ActionCategory category, int potency = 0, bool isCombo = false, int duration = 0)
        {
            Name = name;
            EnglishName = englishName;
            Potency = potency;
            IsCombo = isCombo;
            Category = category;
            Duration = duration;
        }

        public override string ToString()
        {
            return $"[Name: {Name}, EnglishName: {EnglishName}, Potency: {Potency}, IsCombo: {IsCombo}, Category: {Category}]";
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
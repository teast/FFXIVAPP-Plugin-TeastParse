using System;
using FFXIVAPP.Plugin.TeastParse;
using TechTalk.SpecFlow;

namespace Tests.Steps
{
    [Binding]
    public sealed class BattleSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly World _world;

        public BattleSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            if (!_scenarioContext.ContainsKey("World"))
            {
                _scenarioContext["World"] = new World();
            }
        }

        [AfterScenario]
        public void AfterScenario()
        {
            ((World)_scenarioContext["World"]).AfterScenario();
        }

        [Given("Player with name (.*)")]
        public void GivenPlayerWithName(string name)
        {
            ((World)_scenarioContext["World"]).CreatePlayer(name);
        }

        [Given("(.*) is you")]
        public void PlayerIsYou(string name)
        {
            ((World)_scenarioContext["World"]).PlayerIsYou(name);
        }

        [Given("(.*) is he")]
        public void PlayerIsHe(string name)
        {
            ((World)_scenarioContext["World"]).PlayerIsHe(name);
        }

        [Given("(.*) is she")]
        public void PlayerIsShe(string name)
        {
            ((World)_scenarioContext["World"]).PlayerIsHe(name);
        }

        [Given("Monster with name (.*)")]
        public void GivenMonsterWithName(string name)
        {
            ((World)_scenarioContext["World"]).CreateMonster(name);
        }

        [Given("(.*) chat")]
        public void GivenChatLanguage(GameLanguageEnum language)
        {
            ((World)_scenarioContext["World"]).SetLanguage(language);
        }

        [When("chat with code: (.*) and line: (.*)")]
        public void WhenAutoAttack(string code, string line)
        {
            ((World)_scenarioContext["World"]).RaiseChatLog(code, line);
        }

        [When("multiple chat lines")]
        public void WhenMultipleLines(Table table)
        {
            for (int i = 0; i < table.RowCount; i++)
            {
                var line = table.Rows[i]["line"].Trim('"');
                ((World)_scenarioContext["World"]).RaiseChatLog(table.Rows[i]["code"], line);
            }
        }

        [Then("Action (.*) with damage (.*), critical hit: (.*), blocked: (.*), parry: (.*), direct hit: (.*), modifier: (.*), should have been stored for player (.*) against (.*)")]
        public void ThenActionUsed(string action, ulong damage, bool crit, bool blocked, bool parry, bool direct, string modifier, string source, string target)
        {
            ((World)_scenarioContext["World"]).ContainSingleDamage(m =>
                DateTime.Parse(m.OccurredUtc) >= DateTime.UtcNow.AddMinutes(-5) && DateTime.Parse(m.OccurredUtc) <= DateTime.UtcNow &&
                m.Damage == damage &&
                m.Source == source &&
                m.Target == target &&
                m.Critical == crit && m.Blocked == blocked && m.Parried == parry && m.DirectHit == direct &&
                (string.IsNullOrEmpty(modifier) || m.Modifier == modifier) &&
                m.Action.Name == action);
        }

        [Then("Damage of (.*) with critical hit: (.*), blocked: (.*), parry: (.*), direct hit: (.*), modifier: (.*), should be stored for player (.*) against (.*)")]
        public void ThenCheckStoredDamage(ulong damage, bool crit, bool blocked, bool parry, bool direct, string modifier, string source, string target)
        {
            ((World)_scenarioContext["World"]).ContainSingleDamage(m =>
                    DateTime.Parse(m.OccurredUtc) >= DateTime.UtcNow.AddMinutes(-5) && DateTime.Parse(m.OccurredUtc) <= DateTime.UtcNow &&
                    m.Damage == damage &&
                    m.Source == source &&
                    m.Critical == crit && m.Blocked == blocked && m.Parried == parry && m.DirectHit == direct &&
                    (string.IsNullOrEmpty(modifier) || m.Modifier == modifier) &&
                    m.Target == target);
        }

        [Then("No damage made by (.*).")]
        public void ThenCheckMonsterDamage(string source)
        {
            if (source == "[none]") source = "";
            ((World)_scenarioContext["World"]).NotContainDamage(m => m.Source == source);
        }

        [Then("Damage of (.*) should be stored for (.*) against (.*).")]
        public void ThenCheckMonsterDamage(ulong damage, string source, string target)
        {
            if (source == "[none]") source = "";
            ((World)_scenarioContext["World"]).ContainSingleDamage(m =>
                        DateTime.Parse(m.OccurredUtc) >= DateTime.UtcNow.AddMinutes(-5) && DateTime.Parse(m.OccurredUtc) <= DateTime.UtcNow &&
                        m.Damage == damage &&
                        m.Source == source &&
                        m.Target == target);
        }

        [Then("Combo action (.*) with damage (.*) should be stored for (.*) against (.*).")]
        public void ThenCheckComboDamage(string action, ulong damage, string source, string target)
        {
            if (source == "[none]") source = "";
            ((World)_scenarioContext["World"]).ContainSingleDamage(m =>
                        DateTime.Parse(m.OccurredUtc) >= DateTime.UtcNow.AddMinutes(-5) && DateTime.Parse(m.OccurredUtc) <= DateTime.UtcNow &&
                        m.IsCombo == true &&
                        m.Action.Name == action &&
                        m.Damage == damage &&
                        m.Source == source &&
                        m.Target == target);
        }

        [Then("Cure of (.*) should be stored for (.*) on (.*).")]
        public void ThenCheckPlayerCure(ulong cure, string source, string target)
        {

            ((World)_scenarioContext["World"]).ContainSingleCure(m =>
                        DateTime.Parse(m.OccurredUtc) >= DateTime.UtcNow.AddMinutes(-5) && DateTime.Parse(m.OccurredUtc) <= DateTime.UtcNow &&
                        m.Cure == cure &&
                        m.Source == source &&
                        m.Target == target);
        }
    }
}
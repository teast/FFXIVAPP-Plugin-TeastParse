using FFXIVAPP.Plugin.TeastParse.Actors;
using TechTalk.SpecFlow;

namespace Tests.Steps
{
    [Binding]
    public sealed class ActorSteps
    {
        private readonly ScenarioContext _scenarioContext;

        public ActorSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            if (!_scenarioContext.ContainsKey("World"))
            {
                _scenarioContext["World"] = new World();
            }
        }

        [Then("actor with name (.*) that is you exists in database")]
        public void ActorIsYou(string name)
        {
            ((World)_scenarioContext["World"]).ContainActor(actor => 
                actor.ActorType == ActorType.Player &&
                actor.Name == name &&
                actor.IsYou == true &&
                actor.IsFromMemory);
        }

        [Then("actor with name (.*) that is party exists in database")]
        public void ActorIsParty(string name)
        {
            ((World)_scenarioContext["World"]).ContainActor(actor => 
                actor.ActorType == ActorType.Player &&
                actor.Name == name &&
                actor.IsParty == true &&
                actor.IsFromMemory);
        }

        [Then("actor with name (.*) that is alliance exists in database")]
        public void ActorIsAlliance(string name)
        {
            ((World)_scenarioContext["World"]).ContainActor(actor => 
                actor.Name == name &&
                actor.IsAlliance == true &&
                actor.IsFromMemory);
        }

        [Then("actor with name (.*) that is monster exists in database")]
        public void ActorIsMonster(string name)
        {
            ((World)_scenarioContext["World"]).ContainActor(actor =>
                actor.ActorType == ActorType.Monster &&
                actor.Name == name &&
                actor.IsAlliance == false &&
                actor.IsYou == false &&
                actor.IsParty == false &&
                actor.IsFromMemory);
        }

        [Then("actor with name (.*) that is not from memory and is party exists in database")]
        public void ActorIsPartyNotMemory(string name)
        {
            ((World)_scenarioContext["World"]).ContainActor(actor => 
                    actor.ActorType == ActorType.Player &&
                    actor.Name == name &&
                    actor.IsParty == true &&
                    actor.IsFromMemory == false);
        }

        [Given("time is \"(.*)\" UTC")]
        public void MoveTimeForward(string time)
        {
            ((World)_scenarioContext["World"]).SetTimeUtc(time);
        }

        [When("move time forward (.*) seconds")]
        public void MoveTimeForward(int seconds)
        {
            ((World)_scenarioContext["World"]).MoveTimeForward(seconds);
        }

        [Then("player with name (.*) should have detrimental (.*) with start \"(.*)\" and end \"(.*)\"")]
        public void PlayerDetrimental(string playerName, string detrimentalName, string start, string end)
        {
            ((World)_scenarioContext["World"]).PlayerDetrimental(playerName, detrimentalName, start, end);
        }

        [Then("monster with name (.*) should have detrimental (.*) with start \"(.*)\" and end \"(.*)\"")]
        public void MonsterDetrimental(string playerName, string detrimentalName, string start, string end)
        {
            ((World)_scenarioContext["World"]).MonsterDetrimental(playerName, detrimentalName, start, end);
        }
    }
}

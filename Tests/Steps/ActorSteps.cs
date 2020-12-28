using FluentAssertions;
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
            ((World)_scenarioContext["World"]).ContainActor(actor => actor.Name == name && actor.IsYou == true && actor.IsFromMemory);
        }

        [Then("actor with name (.*) that is party exists in database")]
        public void ActorIsParty(string name)
        {
            ((World)_scenarioContext["World"]).ContainActor(actor => actor.Name == name && actor.IsParty == true && actor.IsFromMemory);
        }

        [Then("actor with name (.*) that is alliance exists in database")]
        public void ActorIsAlliance(string name)
        {
            ((World)_scenarioContext["World"]).ContainActor(actor => actor.Name == name && actor.IsAlliance == true && actor.IsFromMemory);
        }

        [Then("actor with name (.*) that is monster exists in database")]
        public void ActorIsMonster(string name)
        {
            ((World)_scenarioContext["World"]).ContainActor(actor => actor.Name == name && actor.IsAlliance == false && actor.IsYou == false && actor.IsParty == false && actor.IsFromMemory);
        }

        [Then("actor with name (.*) that is not from memory and is party exists in database")]
        public void ActorIsPartyNotMemory(string name)
        {
            ((World)_scenarioContext["World"]).ContainActor(actor => actor.Name == name && actor.IsParty == true && actor.IsFromMemory == false);
        }
    }
}

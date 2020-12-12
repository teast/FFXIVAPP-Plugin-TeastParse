using FluentAssertions;
using TechTalk.SpecFlow;

namespace Tests.Steps
{
    [Binding]
    public sealed class UiSteps
    {
        private readonly ScenarioContext _scenarioContext;

        public UiSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            if (!_scenarioContext.ContainsKey("World"))
            {
                _scenarioContext["World"] = new World();
            }
        }

        [Given("(.*) view\\.")]
        public void GivenView(string view)
        {
        }

        [When("user loads parse data `(.*)`\\.")]
        public void WhenLoadParseData(string parseName)
        {
            var model = ((World)_scenarioContext["World"]).MainViewModel;
            model.LoadParse(parseName);
        }

        [Then("loaded parse should be current\\.")]
        public void ThenLoadedParse()
        {
            var model = ((World)_scenarioContext["World"]).MainViewModel;
            model.Active.IsCurrent.Should().BeTrue();
        }

        [Then("loaded parse should be `(.*)`\\.")]
        public void ThenLoadedParse(string parseName)
        {
            var model = ((World)_scenarioContext["World"]).MainViewModel;
            model.Active.Name.Should().Be(parseName);
        }
    }
}
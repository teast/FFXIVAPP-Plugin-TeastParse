Feature: UI requirements
    This file contains tests regarding user interface.

Scenario: User views overview view at first time
Given Overview view.
Then loaded parse should be current.

Scenario: User loads another parse
Given Overview view.
When user loads parse data `parses/parser20201127174532.db`.
Then loaded parse should be `parser20201127174532`.

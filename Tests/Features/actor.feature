Feature: Test that we handles actor information correctly

Scenario: CurrentPlayer do an action. Then register it in database
    Given Player with name Gudrun Arneson
    And Gudrun Arneson is you
    When multiple chat lines
        | code | line                                   |
        | 082B | "You use Devilment."                   |
        | 08AE | " ⇒ You gain the effect of Devilment." |
    Then actor with name Gudrun Arneson that is you exists in database

Scenario: Party player do an action. Then register it in database 
    Given Player with name Gudrun Arneson
    When multiple chat lines
        | code | line                                              |
        | 102B | "Gudrun Arneson use Devilment."                   |
        | 102E | " ⇒ Gudrun Arneson gain the effect of Devilment." |
    Then actor with name Gudrun Arneson that is party exists in database

Scenario: Alliance player do an action. Then register it in database 
    Given Player with name Gudrun Arneson
    When multiple chat lines
        | code | line                                              |
        | 182B | "Gudrun Arneson use Devilment."                   |
        | 182E | " ⇒ Gudrun Arneson gain the effect of Devilment." |
    Then actor with name Gudrun Arneson that is alliance exists in database

Scenario: Unengaged do an action. Then register it in database 
    Given Monster with name Shadowkeeper
    And Player with name Gudrun Arneson
    When multiple chat lines
        | code | line                                        |
        | 302B | "The Shadowkeeper uses Backward Implosion." |
        | 3129 | " ⇒ Gudrun Arneson takes 85237 damage."     |
    Then actor with name Shadowkeeper that is monster exists in database

Scenario: Engaged do an action. Then register it in database 
    Given Monster with name Shadowkeeper
    And Player with name Gudrun Arneson
    When multiple chat lines
        | code | line                                        |
        | 282B | "The Shadowkeeper uses Backward Implosion." |
        | 2929 | " ⇒ Gudrun Arneson takes 85237 damage."     |
    Then actor with name Shadowkeeper that is monster exists in database

Scenario: Party player that do not exist in memory list of players. Should still be registered in database but with "not in memory" set
    Given empty actor memory list
    When multiple chat lines
        | code | line                                              |
        | 102B | "Gudrun Arneson use Devilment."                   |
        | 102E | " ⇒ Gudrun Arneson gain the effect of Devilment." |
    Then actor with name Gudrun Arneson that is not from memory and is party exists in database

Scenario: Player that starts to exist in memory list should have their "not in memory" flag updated in database
    Given empty actor memory list
    When multiple chat lines
        | code | line                                              |
        | 102B | "Gudrun Arneson use Devilment."                   |
        | 102E | " ⇒ Gudrun Arneson gain the effect of Devilment." |
    Then actor with name Gudrun Arneson that is not from memory and is party exists in database
    When Player with name Gudrun Arneson
    And multiple chat lines
        | code | line                                              |
        | 102B | "Gudrun Arneson use Devilment."                   |
        | 102E | " ⇒ Gudrun Arneson gain the effect of Devilment." |
    Then actor with name Gudrun Arneson that is party exists in database

Scenario: CurrentPlayer attacks an monster. Then monster should not be marked as party member.
    Given Monster with name Cliffkite
    And Player with name Gudrun Arneson
    And Gudrun Arneson is you
    When chat with code "0B29" and line "Critical! You hit the cliffkite for 5851 damage."
    Then actor with name Cliffkite that is monster exists in database

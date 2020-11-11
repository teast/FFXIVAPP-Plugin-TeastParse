Feature: Misc events that can end up as damage

Scenario: Player fells from high ground and takes damage
    Given Player with name Player One
    And Player One is you
    When multiple chat lines
        | code | line                       |
        | 08AB | "You ready Teleport."      |
        | 082B | "You use Teleport."        |
        | 08A9 | " ⇒ You take 137 damage." |
    Then No damage made by You.
    And Damage of 137 should be stored for [none] against You.
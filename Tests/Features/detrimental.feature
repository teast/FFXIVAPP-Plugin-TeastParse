Feature: Detrimental attacks

Scenario: Alliance player uses Bioblaster.
    Given Player with name Gun Master
    And Monster with name Silkmoth
    When multiple chat lines
        | code | line                                                   |
        | 182B | "Gun Master uses Bioblaster."                          |
        | 1AA9 | " ⇒ Direct hit! The silkmoth takes 3279 damage."       |
        | 1AAF | " ⇒ The silkmoth suffers the effect of Bioblaster."    |
    Then Action Bioblaster with damage 3279, critical hit: False, blocked: False, parry: False, direct hit: True, modifier: , should have been stored for player Gun Master against Silkmoth
# TODO: Implement damage check for detrimental attacks
    And Action Bioblaster with damage 0, critical hit: False, blocked: False, parry: False, direct hit: False, modifier: , should have been stored for player Gun Master against Silkmoth

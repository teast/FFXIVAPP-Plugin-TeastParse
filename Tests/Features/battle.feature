Feature: Battle parse for single user
    Make sure the parser is handling all
    scenario correct when parsing

@mytag
Scenario: One Auto-attack single monster
    Given Player with name <player>
    And Monster with name <monster>
    And <language> chat
    When chat with code: <code> and line: <line>
    Then Damage of <damage> with critical hit: <crit>, blocked: <blocked>, parry: <parry>, direct hit: <direct>, modifier: <modifier>, should be stored for player <player> against <monster>

Examples:
 | language | player     | monster | damage | crit  | blocked | parry | direct | modifier | code | line                                                         |
 | English  | Player One | Azure | 120    | False | False   | False | False  |          | 12A9 | Player One hits Azure for 120 damage.                        |
 | English  | Player One | Azure | 94231  | True  | False   | False | False  |          | 12A9 | Critical! Player One hits Azure for 94231 damage.            |
 | English  | Player One | Azure | 96231  | True  | False   | False | True   |          | 12A9 | Critical direct hit! Player One hits Azure for 96231 damage. |
 | English  | Player One | Azure | 96231  | False | False   | False | True   |          | 12A9 | Direct hit! Player One hits Azure for 96231 damage.          |
 | English  | Player One | Azure | 96231  | False | True    | False | False  | -20      | 12A9 | Blocked! Player One hits Azure for 96231 (-20%) damage.      |
 | English  | Player One | Azure | 96231  | False | False   | True  | False  | -25      | 12A9 | Parried! Player One hits Azure for 96231 (-25%) damage.      |

Scenario: One Fire III on single monster [English]
    Given Player with name Player One
    And Monster with name Azure
    And English chat
    When multiple chat lines
        | code | line                           |
        | 102B | "Player One cast Fire III."    |
        | 12A9 | " ⇒ Azure takes 1024 damage." |
    Then Action Fire III with damage 1024, critical hit: False, blocked: False, parry: False, direct hit: False, modifier: , should have been stored for player Player One against Azure

Scenario: One Fire III with criticial hit on single monster [English]
    Given Player with name Player One
    And Monster with name Azure
    And English chat
    When multiple chat lines
        | code | line                                     |
        | 102B | "Player One cast Fire III."              |
        | 12A9 | " ⇒ Critical! Azure takes 1024 damage." |
    Then Action Fire III with damage 1024, critical hit: True, blocked: False, parry: False, direct hit: False, modifier: , should have been stored for player Player One against Azure

Scenario: One Fire III with direct hit on single monster [English]
    Given Player with name Player One
    And Monster with name Azure
    And English chat
    When multiple chat lines
        | code | line                                       |
        | 102B | "Player One cast Fire III."                |
        | 12A9 | " ⇒ Direct hit! Azure takes 1024 damage." |
    Then Action Fire III with damage 1024, critical hit: False, blocked: False, parry: False, direct hit: True, modifier: , should have been stored for player Player One against Azure

Scenario: One Fire III with criticial direct hit on single monster [English]
    Given Player with name Player One
    And Monster with name Azure
    And English chat
    When multiple chat lines
        | code | line                                                |
        | 102B | "Player One cast Fire III."                         |
        | 12A9 | " ⇒ Critical direct hit! Azure takes 1024 damage." |
    Then Action Fire III with damage 1024, critical hit: True, blocked: False, parry: False, direct hit: True, modifier: , should have been stored for player Player One against Azure

Scenario: One Fire III with modifier on single monster [English]
    Given Player with name Player One
    And Monster with name Azure
    And English chat
    When multiple chat lines
        | code | line                                  |
        | 102B | "Player One cast Fire III."           |
        | 12A9 | " ⇒ Azure takes 1024 (+68%) damage." |
    Then Action Fire III with damage 1024, critical hit: False, blocked: False, parry: False, direct hit: False, modifier: +68, should have been stored for player Player One against Azure

Scenario: One Bootshine with parry and negative modifier on single monster [English]
    Given Player with name Player One
    And Monster with name Azure
    And English chat
    When multiple chat lines
        | code | line                                           |
        | 102B | "Player One uses Bootshine."                   |
        | 12A9 | " ⇒ Parried! Azure takes 1024 (-68%) damage." |
    Then Action Bootshine with damage 1024, critical hit: False, blocked: False, parry: True, direct hit: False, modifier: -68, should have been stored for player Player One against Azure

Scenario: One Bootshine with block and negative modifier on single monster [English]
    Given Player with name Player One
    And Monster with name Azure
    And English chat
    When multiple chat lines
        | code | line                                           |
        | 102B | "Player One uses Bootshine."                   |
        | 12A9 | " ⇒ Blocked! Azure takes 1024 (-68%) damage." |
    Then Action Bootshine with damage 1024, critical hit: False, blocked: True, parry: False, direct hit: False, modifier: -68, should have been stored for player Player One against Azure

Scenario: One Bootshine and one True Strike on single monster [English]
    Given Player with name Player One
    And Monster with name Azure
    And English chat
    When multiple chat lines
        | code | line                           |
        | 102B | "Player One uses Bootshine."   |
        | 12A9 | " ⇒ Azure takes 1024 damage."  |
        | 102B | "Player One uses True Strike." |
        | 12A9 | " ⇒ Azure takes 1030 damage."  |
    Then Action Bootshine with damage 1024, critical hit: False, blocked: False, parry: False, direct hit: False, modifier: , should have been stored for player Player One against Azure
    And  Action True Strike with damage 1030, critical hit: False, blocked: False, parry: False, direct hit: False, modifier: , should have been stored for player Player One against Azure

Scenario: Two party members doing an action each on single monster [English]
    Given Player with name Player One
    And Player with name Player Two
    And Monster with name Azure
    And English chat
    When multiple chat lines
        | code | line                            |
        | 102B | "Player One uses Fist of Fire." |
        | 102B | "Player Two uses Bootshine."    |
        | 12A9 | " ⇒ Azure takes 1024 damage."  |
        | 102B | "Player One uses True Strike."  |
        | 12A9 | " ⇒ Azure takes 1030 damage."  |
    Then Action Bootshine with damage 1024, critical hit: False, blocked: False, parry: False, direct hit: False, modifier: , should have been stored for player Player Two against Azure
    And  Action True Strike with damage 1030, critical hit: False, blocked: False, parry: False, direct hit: False, modifier: , should have been stored for player Player One against Azure

Scenario: Monster has an "The" in the chat log. Make sure to handle it correctly
    Given Monster with name Zonure
    When chat with code: 28A9 and line: The zonure hits you for 1241 damage.
    Then Damage of 1241 should be stored for Zonure against You.

Scenario: Monster do an delayed action attack. Make sure to bind the damage to that action
    Given Monster with name Ifrit
    And Player with name Hessa Adn
    And Player with name Ryu Yun
    And Player with name Dylune Eclipse
    And Player with name Mark Kero
    When multiple chat lines
        | code | line                                           |
        | 282B | "Ifrit uses Eruption."                         |
        | 102B | "Hessa AdnOmega casts Malefic."                |
        | 12A9 | " ⇒ Ifrit takes 588 damage."                   |
        | 12A9 | "Ryu YunRagnarok hits Ifrit for 173 damage."   |
        | 102B | "Dylune EclipseMoogle begins casting Verfire." |
        | 3129 | " ⇒ Mark KeroRagnarok takes 1537 damage."      |
    Then Damage of 1537 should be stored for Ifrit against Mark KeroRagnarok.

Scenario: Player absorbs hp with Energy Drain.
    Given Player with name Player One
    And Monster with name 4th Legion signifer
    When multiple chat lines
        | code | line                                                       |
        | 102B | "Player One uses Energy Drain."                            |
        | 12A9 | " ⇒ Critical! The 4th Legion signifer takes 7701 damage."  |
        | 112D | " ⇒ Player One absorbs 2609 HP."                           |
    Then Cure of 2609 should be stored for Player One on Player One.

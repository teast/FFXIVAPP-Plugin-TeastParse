Feature: Various chatlogs that can occure regarding healing

Scenario: You recover hp with Souleater.
    Given Player with name Player One
    And Player One is you
    And Monster with name Spirulina
    When multiple chat lines
        | code | line                                           |
        | 082B | "You use Souleater."                           |
        | 0AA9 | " ⇒ The spirulina takes 8175 (+75%) damage."   |
        | 08AD | " ⇒ You recover 5405 HP."                      |
    Then Cure of 5405 should be stored for You on You.

Scenario: White mage uses Plenary Indulgence to recover hp on party.
    Given Player with name Player Whm1
    And Player Whm1 is you
    And Player with name Player Brd1
    And Player Brd1 is he
    And Player with name Player Brd2
    And Player Brd2 is she
    When multiple chat lines
        | code | line                                                           |
        | 082B | "You uses Plenary Indulgence."                         |
        | 08AE | " ⇒ You gains the effect of Confession."               |
        | 092E | " ⇒ Player Brd1 gains the effect of Confession."               |
        | 092E | " ⇒ Player Brd2 gains the effect of Confession."               |
        | 19AD | "Your Plenary Indulgence restores 11400 of your HP."           |
        | 19AD | "Player Brd1's Plenary Indulgence restores 11422 of his HP."   |
        | 19AD | "Player Brd2's Plenary Indulgence restores 11433 of her HP."   |
    Then Cure of 11400 should be stored for You on You.
    And Cure of 11422 should be stored for You on Player Brd1.
    And Cure of 11433 should be stored for You on Player Brd2.

Scenario: Scholar uses Horoscope to recover hp on party.
    Given Player with name Player Sch1
    And Player Sch1 is you
    And Player with name Player Brd1
    And Player Brd1 is he
    And Player with name Player Brd2
    And Player Brd2 is she
    When multiple chat lines
        | code | line                                                  |
        | 082B | "You uses Horoscope."                         |
        | 08AE | " ⇒ You gains the effect of Horoscope."       |
        | 092E | " ⇒ Player Brd1 gains the effect of Horoscope."       |
        | 092E | " ⇒ Player Brd2 gains the effect of Horoscope."       |
        | 19AD | "Your Horoscope restores 11400 of your HP."           |
        | 19AD | "Player Brd1's Horoscope restores 11422 of his HP."   |
        | 19AD | "Player Brd2's Horoscope restores 11433 of her HP."   |
    Then Cure of 11400 should be stored for You on You.
    And Cure of 11422 should be stored for You on Player Brd1.
    And Cure of 11433 should be stored for You on Player Brd2.
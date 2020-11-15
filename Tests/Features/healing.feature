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

# TODO: Add support for this kind of ability
#Scenario: White mage uses Plenary Indulgence to recover hp on party.
#    Given Player with name Player WHM1
#    And Player WHM1 is you
#    And Player with name Player BRD1
#    And Player BRD1 is he
#    And Player with name Player BRD2
#    And Player BRD2 is she
#    When multiple chat lines
#        | code | line                                                           |
#        | 182B | "Player WHM1 uses Plenary Indulgence."                         |
#        | 19AE | " ⇒ Player WHM1 gains the effect of Confession."               |
#        | 19AE | " ⇒ Player BRD1 gains the effect of Confession."               |
#        | 19AE | " ⇒ Player BRD2 gains the effect of Confession."               |
#        | 19AD | "Your Plenary Indulgence restores 11400 of your HP."           |
#        | 19AD | "Player BRD1's Plenary Indulgence restores 11422 of his HP."   |
#        | 19AD | "Player BRD2's Plenary Indulgence restores 11433 of her HP."   |
#    Then Cure of 11400 should be stored for You on You.
#    And Cure of 11422 should be stored for You on Player BRD1.
#    And Cure of 11433 should be stored for You on Player BRD2.

# TODO: Add support for this kind of ability
#Scenario: Scholar uses Horoscope to recover hp on party.
#    Given Player with name Player SCH1
#    And Player SCH1 is you
#    And Player with name Player BRD1
#    And Player BRD1 is he
#    And Player with name Player BRD2
#    And Player BRD2 is she
#    When multiple chat lines
#        | code | line                                                  |
#        | 182B | "Player SCH1 uses Horoscope."                         |
#        | 19AE | " ⇒ Player SCH1 gains the effect of Horoscope."       |
#        | 19AE | " ⇒ Player BRD1 gains the effect of Horoscope."       |
#        | 19AE | " ⇒ Player BRD2 gains the effect of Horoscope."       |
#        | 19AD | "Your Horoscope restores 11400 of your HP."           |
#        | 19AD | "Player BRD1's Horoscope restores 11422 of his HP."   |
#        | 19AD | "Player BRD2's Horoscope restores 11433 of her HP."   |
#    Then Cure of 11400 should be stored for You on You.
#    And Cure of 11422 should be stored for You on Player BRD1.
#    And Cure of 11433 should be stored for You on Player BRD2.
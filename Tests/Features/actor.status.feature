Feature: Test detrimental and beneficial status effects on actors

Scenario: Detrimental should be registered and have correct start/end time.
    Given Player with name Gudrun Anderson
    And Monster with name Unknown
    And time is "2020-12-31 20:10:00" UTC
    When chat with code "102B" and line "Gudrun Anderson casts Dia."
    And chat with code "12AF" and line " ⇒ Unknown suffers the effect of Dia."
    And move time forward 10 seconds
    And chat with code "2AB1" and line "Unknown recovers from the effect of Dia."
    Then monster with name Unknown should have detrimental Dia with start "2020-12-31 20:10:00" and end "2020-12-31 20:10:10"


Feature: File Validation
    As a user of the DPAT tool
    I want to ensure only valid files are accepted
    So that the system processes accurate data

Scenario: Valid Settings.json file should pass validation
    Given I have a valid Settings.json file
    When I validate the Settings.json file
    Then the validation should pass

Scenario: Invalid Settings.json file should fail validation
    Given I have an invalid Settings.json file
    When I validate the Settings.json file
    Then the validation should fail

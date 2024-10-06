Feature: Credit card transaction testing


@tag1
Scenario: Successful credit card transaction with valid credentials
	Given I navigate to the payment web page
	Then I fill-in my valid credentials
	When I click the pay button
	Then I get a successful payment message

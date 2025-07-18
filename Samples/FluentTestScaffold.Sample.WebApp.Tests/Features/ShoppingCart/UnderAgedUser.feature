Feature: UnderAgedUser
	As as shop owner
	I do not want to sell aged restricted items to under aged customers
	So I can be compliant with the law

Background:
	Given the shop has the following items to purchase
		| Title      | Description              | Price  | Age Restriction |
		| Cigarettes | A packet of smokes       | $20.00 | 18              |

Scenario: Should not allow under 18 years old customers to purchase age restricted products
	Given the logged in user is under 18
	And the user has selected the item "Cigarettes"
	When the item is added to the shopping cart
	Then the request should fail with message "You must be over 18 to add this item"
	And the item is not in the user's shopping cart

Scenario: Should allow 18 years old customers to purchase age restricted products
	Given the logged in user is 18
	And the user has selected the item "Cigarettes"
	When the item is added to the shopping cart
	Then request should be successful
	And the item is added to the user's shopping cart

Scenario: Should allow over 18 years old customers to purchase age restricted products
	Given the logged in user is over 18
	And the user has selected the item "Cigarettes"
	When the item is added to the shopping cart
	Then request should be successful
	And the item is added to the user's shopping cart

Feature: Shopping Cart
	As a customer
	I want to add items to my shopping cart
	so I can purchase the items


Background: 
	Given the following user has been registered into the system
		| name          | value                  |
		| Name          | Peter Parker           |
		| Email         | peter.parker@email.com |
		| Password      | Password123!           |
		| Date of Birth | 2001-08-10             |
	And the user has logged in
	And the shop has the following items to purchase
		| Title      | Description              | Price  | Age Restriction |
		| Cigarettes | A packet of smokes       | $20.00 | 18              |
		| Lollipop   | A ball candy on a stick  | $2.00  |                 |
		| Chips      | A bag of crispy potatoes | $4.00  |                 |
	
Scenario: A registered user can add items to his shopping cart
	Given the user has selected the item "Lollipop"
	When the item is added to the shopping cart
	Then request should be successful
	And the item is added to the user's shopping cart
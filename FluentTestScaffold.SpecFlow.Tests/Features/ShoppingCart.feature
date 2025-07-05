Feature: Shopping Cart
As a User
I want to be able to add an Items to my shopping cart
So that i can purchase them all at once

    @shopping-cart
    Scenario: Add item to Shopping Cart
        Given I am Authenticated as the User
          | Field       | Value        |
          | Name        | Bob          |
          | Email       | bob@test.com |
          | Password    | supperS3cret |
          | DateOfBirth | 1990-03-01   |
        When I Add Dead Pool to my Shopping Cart
        Then I should see the item Dead Pool in my Shopping Cart

    @WIP
    Scenario: Under aged User is Unable to Purchase Age Restricted Items
        Given the Items
        | Title     | AgeRestriction |
        | Adult Movie | 18             |
        And I am Authenticated as the User
          | Field       | Value        |
          | Name        | Bob          |
          | Email       | bob@test.com |
          | Password    | supperS3cret |
          | DateOfBirth | 2023-03-01   |
        When I Add Adult Movie to my Shopping Cart
        Then I should get the error "You must be over 18 to add this item" from my Shopping Cart
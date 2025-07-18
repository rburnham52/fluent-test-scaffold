Feature: UserFeatures
	As a user
	I want to know when I'm logged into the system
	So I can correct interact with the system

Background: 
	Given the following user has been registered into the system
		| name          | value                  |
		| Name          | Peter Parker           |
		| Email         | peter.parker@email.com |
		| Password      | Password123!           |
		| Date of Birth | 2001-08-10             |

Scenario: When the user is not logged in, then the system should not return any information about the user
	When the user's details are requested
	Then the system responds with unauthorized

Scenario: When the user has logged in, then the system should respond with the logged in user's details
	Given the user has logged in
	When the user's details are requested
	Then the system responds with the matching user's details

Scenario: When an invalid password is used, then the system should not return any information about the user
	Given the user attempts to sign in with an incorrect password
	When the user's details are requested
	Then the system responds with unauthorized
Scenario: When the user has logged out, then the system should not return any information about the user
	Given the user has logged in
	And the user logs out
	When the user's details are requested
	Then the system responds with unauthorized

# Our Main Goal
We want to train a model using game data from the past 5-10 years,
with about 70-80% of it being training data, and the rest being test data. How we split this up will become important to us very soon, and so we need to start thinking about it

So we need to pass our model a list of games (or events from the espn api), with a certian number of predictors.

## We will likely need 3 separate models:
1. One for the money-line prediction
- This will take our game data and predict a team winner. This will have to be a qualitative response as we're predicting one team or the other.
2. One for the Spread Prediction
- This will take our game data and produce a numeric output for the spread, either positive or negative. This is a quantitative response.
3. One for the total points prediction
- This will take our game data and produce a numeric output, prediciting the total number of points that will be scored in the game. Again, quantitative.

We have a plethora of (undocumented) endpoint located here: [Github Repo](https://github.com/aaronweldy/espn-openapi/blob/main/spec-sports-core-api.yaml)

# Our basic Models

### Season DTO:
*** Endpoint - http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{seasonYear}?lang=en&region=us ***

- We will need to call this for the past X number of years to get seasons (need to include 2025 data as well)
- This is where we will be able to get all of our data from. Each season contains what we need to get the games and stats
- This contains pre/regular/post season results, but we only need the regular season for now.
- The season contains relevant fields
1. Id
2. Type (regular)
3. year
4. start date and end date
5. weeks

- Also contains conferences, which we may want?

### Weeks Response DTO:
*** Endpoint - http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{seasonYear}/types/2/weeks?lang=en&region=us ***

- Just gives us a list of weeks in a season
- We only need this since we will be updating this as the 2025 season progresses, and weeks will be added accordingly.
- Contains relevant fields
1. Count
2. Items: A list of the weeks in the season

### Week DTO:
*** Endpoint - http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{seasonYear}/types/2/weeks/{weekNumber}?lang=en&region=us ***

- This is where A LOT of data lives
- We will need to get all the data from every week for each season we include in the model. That is a lot of endpoint calls
- We may need to limit this as we only get so many for free (I think?)
- Contains relevant fields:
1. Number 
2. Start Date/End Date
3. Events
4. Possibly QBR (this provides stats for each qb at that given time)

### Event Response DTO:
*** Endpoint - http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{seasonYear}/types/2/weeks/{weekNumber}/events?lang=en&region=us ***

- Lists all the events in a given week, these events are the games
- Lists the events as refs, which is annoying
- Includes relevant fields:
1. Count
2. Items: List of the events

### Event DTO:
*** Endpoint - http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/events/{eventID}?lang=en&region=us ***

- This is used to get the game information
- Includes relevant fields:
1. Id
2. Date
3. Name
4. TimeValid
5. Competitions: Contains some relevant info


### Competitions DTO
*** Endpoint - http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/events/{eventId}?lang=en&region=us ***

- Easily the largest and most complicated model we will have
- Includes relevant fields
1. id
2. date
3. timeValid
4. dateValid
5. nuetral site
6. div comp
7. conf comp 
8. Competitors DTO (done)
9. Odds Response DTO (done)
10. Predictor DTO (done)

### Competitors DTO 
*** Endpoint - http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/events/{eventId}/competitions/{event/compId}/competitors/{teamId}?lang=en&region=us ***

- Gets us a lot of information on the teams themselves in a given game
- Include relevant fields:
1. Id
2. homeAway
3. Winner
4. Team DTO
5. Score DTO
6. Statistics DTO
7. Record DTO


### Odds Response DTO:
*** Endpoint - http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/events/{eventId}/competitions/{event/compId}/odds?lang=en&region=us ***

- Gets us the odds we want to predict 
- A lot of useless information here, could confuse the model
- Inlcudes relevant fields:
1. Count
2. Items: List of odds 

### Odds DTO:
*** Endpoint - http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/events/401772936/competitions/401772936/odds/58?lang=en&region=us ***

- Gets us the actual odds in a game for different books
- Gets home team and away team odds
- Inlcudes relevant fields:
1. detials
2. overUnder
3. spread
4. overOdds
5. underOdds
6. TeamOdds DTO
7. moneyLine winner
8. spread winner


### Predictor DTO:
*** Endpoint - http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/events/401772936/competitions/401772936/predictor?lang=en&region=us ***

- Includes prediction statistics for a game
- Includes relevant fields
1. Name
2. lastModified
3. HomeTeam - Statistics
4. AwayTeam - Statistics


### What I need to do Sunday:
1. Complete the Season Model
2. Complete the Week Model
3. Fix the flow of data, from Season -> Week -> Event
4. Test to see if we can actually get the data
5

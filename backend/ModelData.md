### Deciding The Data
- We need to decide on what data is relevant and helpful for each of out models (spread, ML, Totals)
- Much of this will be the same, but some of it may not. 
- We may want to extract out a common ModelData object that contains data relevant to all models, then just add the other stuff

### Caching The Data:
- This might be a pain, and maybe even unecessary (idk if espn limits our endpoint calls), but if they do, we're going to have to cache a lot of 
- data when training the model if we can.

### The Data We Have
- I will be listing the data (or some of it since there is a lot in some cases)
- We can decide what is useful, and how to put it in our models

## The Season is our large model, it is the initial call we make to get all of our data
*** Season ***
- Year
- StartDate
- EndDate
- SeasonType
    - Id
    - Type (pre, regular, post)
    - Week Model (current week we're in) We should use this property to get the     data for the games we want to model
    - List of Weeks (all weeks for the year)


*** Week ***
- WeekNumber
- StartDate
- EndDate
- List of Events

*** Event ***
- Id
- Date
- Name
- TimeValid 
- List of Competitions (this is almost always just a singular item, Idk why its a list)
- SeasonNumber
- WeekNumber


*** Competition ***
- Id
- Date
- TimeValid
- DateValid
- NeutralSite
- DivisionComp
- ConferenceComp
- List of Competitors
    - Id
    - HomeAway
    - Winner
    - Team (Where much of our data will come from)
        - Id
        - Location
        - Name
        - DisplayName
        - Record
        - OddsRecord
        - Statistics
        - injuries
    - Score
        - Value
        - Display Value
        - Winner
- Odds
    - Details
    - AverageOverUnder
    - AverageSpread
    - MoneyLineWinner
    - SpreadWinner
- Predictors
    - Name
    - Short Name
    - LastModified
    - TeamPredictors
        - List of statistics for each team

*** Record ***
- Wins
- Losses
- AveragePointsAgainst;
- AveragePointsFor;
- PointDifferential;
- DivisionWinPercent;
- LeagueWinPercent;
- Streak;
- Ties;
- WinPercent;
- DivisionLosses;
- DivisionWins;
- HomeWins;
- HomeLosses;
- AwayWins;
- AwayLosses;
- ConferenceWins;
- ConferenceLosses;

## Important for model. It will want the wins and losses against the spread, moneyline, total
*** Odds Record ***
- List of OddsStats
    - OddsRecord
    - Wins
    - Losses


# This is where we can get A LOT of data for each team. This covers like every stat in football. This will likely be a shared model that we can pass to all 3 of our models
*** Statistics ***
- Name
- DisplayName
- List of category Stats
    - Name
    - Display Name
    - Value
    - Rank


#### What We need to do Now?
- We need to investigate all the data we get and determine what is useful.
- The statistics call in a Team model has thousands of lines, and some are not important. But most are...
- We will likely have to either include all categories, which will be an insane amount of data, or pick the most important ones.
- Or we can get all the value data from it and create some total stat score, but that may not be great for the model
- The record data has a lot of important info, wins, losses, division/conference play
- Odds Records is important to know how the teams are against the bets placed on them
- Total points is just team1.score.value + team2.score.value
- Spread is weird, we need to think about how to calculate spread
- Money line is just team1.score.winner = true/false

1. Remove Uneccessary fields 
    - 
    - spread/moneyline winner
    - HomeTeamMlWins, HomeTeamMlLosses, HomeTeamSpreadWins, HomeTeamSpreadLosses + away versions
    - Fill in NAN for AveragePredictedTotal, Average Predicted Spread
    - Drop moneyline winner, spreadwinner, predictedaway/homeoppStrength, home/away yards allowed
    

2. Fix conference/division games always being 0

3. Fix Model Training:
    - Heavily Favors Home
    - Every spread is terrbile
    - Totals aren't bad

4. Fix Week Prediction Excel Format
    - Only include predicted spread, total, winner
    - Include vegas predicted winner 


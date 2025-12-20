from fastapi import FastAPI
import pandas as pd
import numpy as np
from model_training import train_model
from prediction_data import PredictionData
from pathlib import Path

app = FastAPI()

def compute_95CI(pred, residuals):
    """Compute 95% confidence interval for regression prediction based on residuals."""
    sigma = np.std(residuals)
    lower = pred - 1.96 * sigma
    upper = pred + 1.96 * sigma
    return [lower, upper]

@app.post("/predict")
def make_prediction(predictionData: PredictionData, model_dict=None):
    print("Predictions started")
    print(predictionData)

    # Convert PredictionData to DataFrame
    data_dict = predictionData.model_dump()
    prediction_data_df = pd.DataFrame.from_dict(data_dict, orient="index", columns=["Value"])
    prediction_data_df = prediction_data_df.reset_index().rename(columns={"index": "Property"})
    
    BASE_DIR = Path(__file__).parent.parent  
    TEAM_STATS_FILE = BASE_DIR / "Data" / "TrainingData" / "game_team_data_2025.csv"

    team_stats_df = pd.read_csv(TEAM_STATS_FILE)
    team_stats_df = team_stats_df.sort_values(["season", "week"])

    # Extract most recent home/away stats
    home_team_name = predictionData.HomeTeamName
    away_team_name = predictionData.AwayTeamName

    home_stats = team_stats_df[team_stats_df["HomeTeamName"] == home_team_name].iloc[-1].filter(like="_home")
    away_stats = team_stats_df[team_stats_df["AwayTeamName"] == away_team_name].iloc[-1].filter(like="_away")
    home_stats['season'] = team_stats_df['season']
    home_stats['week'] = team_stats_df['week']

    # Merge all features
    features_df = pd.DataFrame({**prediction_data_df.set_index("Property")["Value"].to_dict(),
                                **home_stats.to_dict(),
                                **away_stats.to_dict()}, index=[0])
    
    # Load or train models
    if model_dict is None:
        total_model, spread_model, win_model, X_train, y_total_train, y_spread_train = train_model()
    else:
        total_model = model_dict["total_model"]
        spread_model = model_dict["spread_model"]
        win_model = model_dict["win_model"]
        X_train = model_dict["X_train"]
        y_total_train = model_dict["y_total_train"]
        y_spread_train = model_dict["y_spread_train"]

    # Align feature columns
    common_cols = X_train.columns.intersection(features_df.columns)
    X_features_total = features_df[common_cols]

    # Make predictions
    total_pred = float(total_model.predict(X_features_total)[0])
    spread_pred = float(spread_model.predict(X_features_total)[0])
    win_prob = float(win_model.predict_proba(X_features_total)[0,1])

    # Compute residuals from training set
    residuals_total = y_total_train.values - total_model.predict(X_train)
    residuals_spread = y_spread_train.values - spread_model.predict(X_train)

    # Compute 95% confidence intervals
    total_conf_range = compute_95CI(total_pred, residuals_total)
    spread_conf_range = compute_95CI(spread_pred, residuals_spread)

    # Winner prediction
    winner_prediction = home_team_name if win_prob >= 0.5 else away_team_name
    winner_confidence = max(win_prob, 1 - win_prob)

    game_predictions = {
        "total_prediction": total_pred,
        "total_confidence_range": total_conf_range,
        "total_confidence_score": 0.95,
        "spread_prediction": spread_pred,
        "spread_confidence_range": spread_conf_range,
        "spread_confidence_score": 0.95,
        "winner_prediction": winner_prediction,
        "winner_confidence": winner_confidence,
        "home_win_probability": win_prob,
        "away_win_probability": 1 - win_prob
    }

    print(game_predictions)

    return game_predictions


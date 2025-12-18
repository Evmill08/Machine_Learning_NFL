import xgboost as xg
from sklearn.metrics import accuracy_score, mean_absolute_error
from xgboost import XGBRegressor
import pandas as pd

# WE NEED:
# 2 regression models for total and spread
# One classifier for the winner

def drop_non_numeric(df: pd.DataFrame) -> pd.DataFrame:
    return df.select_dtypes(include=["number", "bool"])

def train_model():
    training_data = pd.read_csv("Data\\\\TrainingData\\\\game_team_data_2025.csv")
    testing_data = pd.read_csv("Data\\\\TrainingData\\\\game_team_data_2018_2024.csv")

    target_cols = ["TotalPoints", "Spread", "HomeWin"]

    base_drop_cols = [
        "game_id",
        "team_x",
        "season_type_x",
        "opponent_team_x",
        "fg_made_list_home",
        "fg_missed_list_home",
        "fg_blocked_list_home",
        "team_name_x",
        "opponent_name_x",
        "HomeTeamName_x",
        "AwayTeamName_x",
        "team_y",
        "season_type_y",
        "opponent_team_y",
        "fg_made_list_away",
        "fg_missed_list_away",
        "fg_blocked_list_away",
        "team_name_y",
        "opponent_name_y",
        "HomeTeamName_y",
        "AwayTeamName_y",
        "HomeTeamName",
        "AwayTeamName"
    ]

    X_train = training_data.drop(
        columns=[c for c in base_drop_cols + target_cols if c in training_data.columns]
    )

    X_train = drop_non_numeric(X_train)

    X_test = testing_data.drop(
        columns=[c for c in base_drop_cols + target_cols if c in testing_data.columns]
    )

    X_test = drop_non_numeric(X_test)

    print(X_train.columns.to_list())
    print("\n\n")
    print(X_test.columns.to_list())
    

    y_total_train = training_data["TotalPoints"]
    y_spread_train = training_data["Spread"]
    y_win_train = training_data["HomeWin"]

    y_total_test = testing_data["TotalPoints"]
    y_spread_test = testing_data["Spread"]
    y_win_test = testing_data["HomeWin"]

    total_model = XGBRegressor()
    spread_model = XGBRegressor()
    win_model = xg.XGBClassifier(objective="binary:logistic")

    total_model.fit(X_train, y_total_train)
    spread_model.fit(X_train, y_spread_train)
    win_model.fit(X_train, y_win_train)


    y_pred_total  = total_model.predict(X_test)
    y_pred_spread = spread_model.predict(X_test)
    y_pred_win    = win_model.predict(X_test)

    print(f"Total MAE:  {mean_absolute_error(y_total_test, y_pred_total):.2f}")
    print(f"Spread MAE: {mean_absolute_error(y_spread_test, y_pred_spread):.2f}")
    print(f"Win Accuracy: {accuracy_score(y_win_test, y_pred_win) * 100:.2f}%")

train_model()
import xgboost as xg
from sklearn.metrics import mean_absolute_error, log_loss, roc_auc_score
from xgboost import XGBRegressor
import pandas as pd
from pathlib import Path


def drop_non_numeric(df: pd.DataFrame) -> pd.DataFrame:
    return df.select_dtypes(include=["number", "bool"])

def train_model():
    BASE_DIR = Path(__file__).parent.parent  
    TEAM_STATS_FILE_TRAIN = BASE_DIR / "Data" / "TrainingData" / "game_team_data_2018_2024.csv"
    TEAM_STATS_FILE_TEST = BASE_DIR / "Data" / "TrainingData" / "game_team_data_2025.csv"

    training_data = pd.read_csv(TEAM_STATS_FILE_TRAIN)
    testing_data = pd.read_csv(TEAM_STATS_FILE_TEST)

    target_cols = ["TotalPoints", "Spread", "HomeWin"]

    base_drop_cols = [
        "Spread",
        "TotalPoints",
        "HomeWin",
        "game_id",
        "HomeTeamName",
        "AwayTeamName",
        "HomeTeamId",
        "AwayTeamId",
        "fg_made_list",
        "fg_missed_list",
        "fg_blovked_list",
        "team_name",
        "opponent_team",
        "EventId",
    ]

    X_train = training_data.drop(
        columns=[c for c in base_drop_cols + target_cols if c in training_data.columns]
    )

    X_train = X_train.select_dtypes(include=["number"])

    X_test = testing_data.drop(
        columns=[c for c in base_drop_cols + target_cols if c in testing_data.columns]
    )

    X_test  = X_test.select_dtypes(include=["number"])

    y_total_train = training_data["TotalPoints"]
    y_spread_train = training_data["Spread"]
    y_win_train = training_data["HomeWin"]

    y_total_test = testing_data["TotalPoints"]
    y_spread_test = testing_data["Spread"]
    y_win_test = testing_data["HomeWin"]

    common_cols = X_train.columns.intersection(X_test.columns)
    X_train = X_train[common_cols]
    X_test  = X_test[common_cols]

    assert list(X_train.columns) == list(X_test.columns)

    total_model = XGBRegressor(n_estimators=1000, learning_rate=.03, max_depth=4, subsample=.8, colsample_bytree=.08, eval_metric="mae", early_stopping_rounds=50)
    spread_model = XGBRegressor(n_estimators=1000, learning_rate=.03, max_depth=4, subsample=.8, colsample_bytree=.08, eval_metric="mae", early_stopping_rounds=50)
    win_model = xg.XGBClassifier(objective="binary:logistic", n_estimators=1000, learning_rate=.03, max_depth=4, subsample=.8, colsample_bytree=.08, eval_metric="logloss", early_stopping_rounds=50)

    total_model.fit(X_train, y_total_train, eval_set=[(X_test, y_total_test)], verbose=False)
    spread_model.fit(X_train, y_spread_train, eval_set=[(X_test, y_spread_test)], verbose=False)
    win_model.fit(X_train, y_win_train, eval_set=[(X_test, y_win_test)], verbose=False)

    y_pred_total  = total_model.predict(X_test)
    y_pred_spread = spread_model.predict(X_test)
    y_pred_win = win_model.predict_proba(X_test)[: ,1]

    print(f"Total MAE:  {mean_absolute_error(y_total_test, y_pred_total):.2f}")
    print(f"Spread MAE: {mean_absolute_error(y_spread_test, y_pred_spread):.2f}")
    print(f"Win LogLoss: {log_loss(y_win_test, y_pred_win):.4f}")
    print(f"Win ROC-AUC: {roc_auc_score(y_win_test, y_pred_win):.4f}")

    return (total_model, spread_model, win_model, X_train, y_total_train, y_spread_train)
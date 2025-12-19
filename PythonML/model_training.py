import xgboost as xg
from sklearn.metrics import accuracy_score, mean_absolute_error, log_loss, roc_auc_score
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
        "Spread_x",
        "Spread_y",
        "TotalPoints_x",
        "TotalPoints_y",
        "game_id",
        "EventId,"
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
        "AwayTeamName",
        "fg_blocked_list_x",
        "fg_blocked_list_y"
    ]

    X_train = training_data.drop(
        columns=[c for c in base_drop_cols + target_cols if c in training_data.columns]
    )

    X_train = X_train.select_dtypes(include=["number"])

    X_test = testing_data.drop(
        columns=[c for c in base_drop_cols + target_cols if c in testing_data.columns]
    )

    X_test  = X_test.select_dtypes(include=["number"])

    LEAKY_WIN_COLS = [
        c for c in X_train.columns
        if (
            "spread" in c.lower()
            or "differential" in c.lower()
            or "line" in c.lower()
            or "predicted" in c.lower()
        )
    ]

    X_train_win = X_train.drop(columns=LEAKY_WIN_COLS)
    X_test_win  = X_test.drop(columns=LEAKY_WIN_COLS)

    print([c for c in X_train.columns if "point" in c.lower()])
    print([c for c in X_train.columns if "score" in c.lower()])
    print([c for c in X_train.columns if "spread" in c.lower()])

    for col in target_cols:
        assert col not in X_train.columns, f"LEAKAGE: {col} in X_train"
        assert col not in X_test.columns, f"LEAKAGE: {col} in X_test"

    y_total_train = training_data["TotalPoints"]
    y_spread_train = training_data["Spread"]
    y_win_train = training_data["HomeWin"]

    y_total_test = testing_data["TotalPoints"]
    y_spread_test = testing_data["Spread"]
    y_win_test = testing_data["HomeWin"]

    common_cols = X_train.columns.intersection(X_test.columns)
    X_train = X_train[common_cols]
    X_test  = X_test[common_cols]

    common_cols_win = X_train_win.columns.intersection(X_test_win.columns)
    X_train_win = X_train_win[common_cols_win]
    X_test_win  = X_test_win[common_cols_win]

    assert list(X_train.columns) == list(X_test.columns)
    assert list(X_train_win.columns) == list(X_test_win.columns)

    total_model = XGBRegressor(n_estimators=1000, learning_rate=.03, max_depth=4, subsample=.8, colsample_bytree=.08, eval_metric="mae", early_stopping_rounds=50)
    spread_model = XGBRegressor(n_estimators=1000, learning_rate=.03, max_depth=4, subsample=.8, colsample_bytree=.08, eval_metric="mae", early_stopping_rounds=50)
    win_model = xg.XGBClassifier(objective="binary:logistic", n_estimators=1000, learning_rate=.03, max_depth=4, subsample=.8, colsample_bytree=.08, eval_metric="logloss", early_stopping_rounds=50)

    total_model.fit(X_train, y_total_train, eval_set=[(X_test, y_total_test)], verbose=False)
    spread_model.fit(X_train, y_spread_train, eval_set=[(X_test, y_spread_test)], verbose=False)
    win_model.fit(X_train_win, y_win_train, eval_set=[(X_test_win, y_win_test)], verbose=False)

    pd.concat([
        X_train_win,
        y_win_train
    ], axis=1).corr()["HomeWin"].sort_values(ascending=False).head(10)

    y_pred_total  = total_model.predict(X_test)
    y_pred_spread = spread_model.predict(X_test)
    y_pred_win = win_model.predict_proba(X_test_win)[: ,1]

    print(f"Total MAE:  {mean_absolute_error(y_total_test, y_pred_total):.2f}")
    print(f"Spread MAE: {mean_absolute_error(y_spread_test, y_pred_spread):.2f}")
    print(f"Win LogLoss: {log_loss(y_win_test, y_pred_win):.4f}")
    print(f"Win ROC-AUC: {roc_auc_score(y_win_test, y_pred_win):.4f}")

    return (total_model, spread_model, win_model, X_train, y_total_train, y_spread_train)
import pandas as pd
import requests

def create_dataset(df_team_stats_file: str, df_game_stats_file: str, df_game_stats_sheet: str, map_file: str, output_file: str, current: bool = False):
    # Should reach out to our backend, update the current year, then create out train and test datasets
    # If we are getting current data, update it, else use the historical data that will not change
    if (current):
        try:
            nfl_predictions_url = "http://localhost:5145/data/year"
            response = requests.get(nfl_predictions_url)

            if not response.status_code < 300:
                print("Error fetching 2025 data")
                return
        except:
            print("Error updating 2025 data sheet")
            return

    df_team_stats = pd.read_csv(df_team_stats_file)
    df_team_stats = df_team_stats.sort_values(["season", "week", "team"])
                                              
    df_game_stats = pd.read_excel(df_game_stats_file, sheet_name=df_game_stats_sheet)
    df_map = pd.read_excel(map_file, sheet_name='Map')
    team_map = df_map.set_index('Abbreviations')['Team Name'].to_dict()

    # Map the abbreviations in the df itself
    df_team_stats['team_name'] = df_team_stats['team'].map(team_map)
    df_team_stats['opponent_name'] = df_team_stats['opponent_team'].map(team_map)

    # Create game ids for each game so that BAL vs BUF == BUF vs BAL
    df_team_stats['game_id'] = (
        df_team_stats[['team_name', 'opponent_name']]
        .fillna('UNKNOWN')
        .astype(str)
        .apply(lambda x: '_'.join(sorted(x)), axis=1)
    )

    # Dropping these from the team data, so when we merge with game data, these wil; not be here
    NON_FEATURE_COLS_TEAM = ["season", "week", "team", "season_type", "opponent_team", "team_name", "opponent_name", "game_id", "fg_made_list", "fg_missed_list", "fg_blocked_list"]
    STAT_COLS = (
        df_team_stats
        .drop(columns=NON_FEATURE_COLS_TEAM)
        .select_dtypes(include="number")
        .columns
        .tolist()
    )

    # Weighted average for the data points, last 5 games have more weight
    SPAN = 5
    weighted_features = (
        df_team_stats
        .groupby(["season", "team"], group_keys=False)[STAT_COLS]
        .apply(lambda x: x.shift(1).ewm(span=SPAN, adjust=False).mean()))
    
    for col in STAT_COLS:
        df_team_stats[col] = weighted_features[col]

    # Dropping the first week weighted average data
    df_team_stats = df_team_stats.dropna(subset=[f"{STAT_COLS[0]}"])

    # Reverse the spread, get rid of leaked data, rename some of the columns for ease of merge
    df_game_stats['Spread'] = -df_game_stats['Spread']
    df_game_stats = df_game_stats.drop(columns={'AwayWinner', "HomeWinner"}, axis=1)
    df_game_stats = df_game_stats.rename(columns={"SeasonYear": "season", "WeekNumber": "week"})

    df_game_stats['game_id'] = (
        df_game_stats[['HomeTeamName', 'AwayTeamName']]
        .fillna('UNKNOWN')
        .astype(str)
        .apply(lambda x: '_'.join(sorted(x)), axis=1)
    )

    home_stats = df_game_stats.merge(
        df_team_stats,
        left_on=['season', 'week', 'game_id', 'HomeTeamName'],
        right_on=['season', 'week', 'game_id', 'team_name'],
        how='left',
    )

    home_stats = home_stats = home_stats.rename(columns={c: f"{c}_home" for c in STAT_COLS})

    away_stats = df_game_stats.merge(
        df_team_stats,
        left_on=['season', 'week', 'game_id', 'AwayTeamName'],
        right_on=['season', 'week', 'game_id', 'team_name'],
        how='left'
    )

    away_stats = away_stats = away_stats.rename(columns={c: f"{c}_away" for c in STAT_COLS})

    final_df = home_stats.merge(
        away_stats[['season', 'week', 'game_id'] + [f"{c}_away" for c in STAT_COLS]],
        on=['season', 'week', 'game_id'],
        how='left',
    )

    merged_message = "2018-2024" if not current else "2025"
    final_df.to_csv(f"Data////TrainingData////{output_file}", index=False)
    print(f"Saved merged {merged_message} stats as {output_file}")

# Create the historical data set
create_dataset(
    df_team_stats_file="Data\\\\Old Data\\\\team_stats_2018_2024.csv",
    df_game_stats_file="Data\\\\Old Data\\\\NFL_Predictions.xlsx",
    df_game_stats_sheet="Range_2018_2024",
    map_file="Data\\\\Old Data\\\\NFL_Predictions.xlsx",
    output_file="game_team_data_2018_2024.csv"
)

# Create the current data set
create_dataset(
    df_team_stats_file="Data\\\\Old Data\\\\team_stats_2025.csv",
    df_game_stats_file="Data\\\\Old Data\\\\NFL_Predictions.xlsx",
    df_game_stats_sheet="Year_2025",
    map_file="Data\\\\Old Data\\\\NFL_Predictions.xlsx",
    output_file="game_team_data_2025.csv",
    current=True
)

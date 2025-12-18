import pandas as pd
import requests

def create_dataset(df_a_file: str, df_b_file: str, df_b_sheet: str, map_file: str, output_file: str, current: bool = False):
    # Should reach out to our backend, update the current year, then create out train and test datasets
    # If we are getting current data, update it, else use the historical data that will not change
    if (current):
        try:
            nfl_predictions_url = "http://localhost:5145/data/year"
            response = requests.get(nfl_predictions_url)
        except:
            return "Error updating 2025 data sheet"

    df_a = pd.read_csv(df_a_file)
    df_b = pd.read_excel(df_b_file, sheet_name=df_b_sheet)
    df_map = pd.read_excel(map_file, sheet_name='Map')

    team_map = df_map.set_index('Abbreviation')['Team Name'].to_dict()

    # Map the abbreviations in the df itself
    df_a['team_name'] = df_a['team'].map(team_map)
    df_a['opponent_name'] = df_a['opponent_team'].map(team_map)

    # Create game ids for each game so that BAL vs BUF == BUF vs BAL
    df_a['game_id'] = (
        df_a[['team_name', 'opponent_name']]
        .fillna('UNKNOWN')
        .astype(str)
        .apply(lambda x: '_'.join(sorted(x)), axis=1)
    )

    df_b['Spread'] = -df_b['Spread']
    df_b = df_b.drop('AwayWinner', axis=1)
    df_b = df_b.rename(columns={"SeasonYear": "season", "WeekNumber": "week"})

    df_b['game_id'] = (
        df_b[['HomeTeamName', 'AwayTeamName']]
        .fillna('UNKNOWN')
        .astype(str)
        .apply(lambda x: '_'.join(sorted(x)), axis=1)
    )

    # Merge the home stats 
    home_stats = df_a.merge(
        df_b,
        left_on=['season', 'week', 'game_id', 'team_name'],
        right_on=['season', 'week', 'game_id', 'HomeTeamName'],
        how='inner'
    )

    # Merge the away stats
    away_stats = df_a.merge(
        df_b,
        left_on=['season', 'week', 'game_id', 'opponent_name'],
        right_on=['season', 'week', 'game_id', 'AwayTeamName'],
        how='inner'
    )

    # Get the stat columns
    stat_cols = [
        c for c in df_a.columns
        if c not in ['season', 'week', 'team', 'opponent_team',
                    'team_name', 'opponent_name', 'season_type', 'game_id']
    ]

    # Create properly named stats for the game to discern home and away
    home_stats = home_stats.rename(columns={c: f"{c}_home" for c in stat_cols})
    away_stats = away_stats.rename(columns={c: f"{c}_away" for c in stat_cols})

    # Merge the home and away stats into one column
    team_game_stats = home_stats.merge(
        away_stats,
        on=['season', 'week', 'game_id'],
        how='inner'
    )

    # Merge the total game stats into the game data 
    final_df = df_b.merge(
        team_game_stats,
        on=['season', 'week', 'game_id'],
        how='left'
    )

    # ========================
    # 7. Save as CSV
    # ========================
    merged_message = "2018-2024" if not current else "2025"
    final_df.to_csv(f"Data////TrainingData////{output_file}", index=False)
    print(f"Saved merged {merged_message} stats as {output_file}")

# Create the historical data set
create_dataset(
    df_a_file="Data\\\\Old Data\\\\team_stats_2018_2024.csv",
    df_b_file="Data\\\\Old Data\\\\NFL_Predictions.xlsx",
    df_b_sheet="Range_2018_2024",
    map_file="Data\\\\Old Data\\\\NFL_Predictions.xlsx",
    output_file="game_team_data_2018_2024.csv"
)

# Create the current data set
create_dataset(
    df_a_file="Data\\\\Old Data\\\\team_stats_2025.csv",
    df_b_file="Data\\\\Old Data\\\\NFL_Predictions.xlsx",
    df_b_sheet="Year_2025",
    map_file="Data\\\\Old Data\\\\NFL_Predictions.xlsx",
    output_file="game_team_data_2025.csv",
    current=True
)
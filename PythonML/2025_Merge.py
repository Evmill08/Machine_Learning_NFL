import pandas as pd

# TODO: Make this a function that uses the specific data frames as params, don't need two files here 
# Load files
df_a = pd.read_csv("Old Data\\\\team_stats_2025.csv")
df_b = pd.read_excel('Old Data\\\\NFL_Predictions.xlsx', sheet_name='Year_2025')
df_map = pd.read_excel('Old Data\\\\NFL_Predictions.xlsx', sheet_name='Map')

# Map from abbr (eg BAL, used in csv) to team name (eg Ravens, used in xlsx)
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

final_df.to_csv('game_team_data_2025.csv', index=False)
print("Saved merged 2018-2024 stats as merged_team_stats_2025.csv")

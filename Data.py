import pandas as pd

# Load files
df_a = pd.read_csv('team_stats_2018_2024.csv')
df_b = pd.read_excel('NFL_Predictions.xlsx', sheet_name='Range_2018_2024')

# ========== 1. Normal merge (home perspective) ==========
forward = df_a.merge(
    df_b[['season', 'week', 'team', 'opponent_team',
          'TotalPoints', 'Spread', 'HomeWin']],
    on=['season', 'week', 'team', 'opponent_team'],
    how='left'
)

# ========== 2. Create REVERSED version of File B ==========
df_b_rev = df_b.copy()
df_b_rev = df_b_rev.rename(columns={
    'team': 'opponent_team',
    'opponent_team': 'team'
})

# Reverse home/away results:
# Away team wins = 1 - HomeWin
df_b_rev['HomeWin'] = 1 - df_b_rev['HomeWin']
df_b_rev['Spread'] = -df_b_rev['Spread']  # spread flips sign

# ========== 3. Merge reversed version (away perspective) ==========
reverse = df_a.merge(
    df_b_rev[['season', 'week', 'team', 'opponent_team',
              'TotalPoints', 'Spread', 'HomeWin']],
    on=['season', 'week', 'team', 'opponent_team'],
    how='left'
)

# ========== 4. Fill missing forward values with reverse values ==========
for col in ['TotalPoints', 'Spread', 'HomeWin']:
    forward[col] = forward[col].fillna(reverse[col])

merged_df = forward

# Move prediction columns to left
new_cols = ['TotalPoints', 'Spread', 'HomeWin']
other_cols = [c for c in merged_df.columns if c not in new_cols]
merged_df = merged_df[new_cols + other_cols]

# Save
merged_df.to_csv('merged_team_stats.csv', index=False)

print("Merged both home & away rows. Saved merged_team_stats.csv")

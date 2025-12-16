import pandas as pd

# ========================
# 1. Load A & B
# ========================
df_a = pd.read_csv('team_stats_2018_2024.csv')
df_b = pd.read_excel('NFL_Predictions.xlsx', sheet_name='Range_2018_2024')

# ========================
# 2. Normal merge (home perspective)
# ========================
forward = df_a.merge(
    df_b[['season', 'week', 'team', 'opponent_team',
          'TotalPoints', 'Spread', 'HomeWin']],
    on=['season', 'week', 'team', 'opponent_team'],
    how='left'
)

# ========================
# 3. Create reversed version of File B
# ========================
df_b_rev = df_b.copy()
df_b_rev = df_b_rev.rename(columns={
    'team': 'opponent_team',
    'opponent_team': 'team'
})

# reverse the outcome perspective:
df_b_rev['HomeWin'] = 1 - df_b_rev['HomeWin']     # away team wins
df_b_rev['Spread'] = -df_b_rev['Spread']          # flip spread
# TotalPoints stays the same

# ========================
# 4. Merge reversed version (away perspective)
# ========================
reverse = df_a.merge(
    df_b_rev[['season', 'week', 'team', 'opponent_team',
              'TotalPoints', 'Spread', 'HomeWin']],
    on=['season', 'week', 'team', 'opponent_team'],
    how='left'
)

# ========================
# 5. Fill missing home rows with away rows
# ========================
for col in ['TotalPoints', 'Spread', 'HomeWin']:
    forward[col] = forward[col].fillna(reverse[col])

merged_df = forward

# ========================
# 6. Reorder columns → stats first
# ========================
new_cols = ['TotalPoints', 'Spread', 'HomeWin']
other_cols = [c for c in merged_df.columns if c not in new_cols]
merged_df = merged_df[new_cols + other_cols]

# ========================
# 7. Save as CSV
# ========================
merged_df.to_csv('merged_team_stats_2018_2024.csv', index=False)
print("Saved merged 2018-2024 stats as merged_team_stats_2018_2024.csv")

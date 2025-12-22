import { GetGameData, GetCurrentWeekNumber } from "./Games.api";

export const gamesQueryKeys = {
    all: ["games"] as const,
    week: (week: number) =>  [...gamesQueryKeys.all, week] as const,
};

export const gamesQueryOptions = (week: number) => ({
    queryKey: gamesQueryKeys.week(week),
    queryFn: () => GetGameData(week),
    staleTime: 1000 * 60 * 60 * 24 * 6, // Game data is considered fresh for 6 days
    cacheTime: 1000 * 60 * 60 * 24 * 14 // Game data is stored in memory for 14
});
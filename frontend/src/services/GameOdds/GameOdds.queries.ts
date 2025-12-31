import { GetGameOdds } from "./GameOdds.api";

export const oddsQueryKeys = {
    all: ["odds"] as const,
    eventId: (eventId: string) =>  [...oddsQueryKeys.all, eventId] as const,
};

export const gameOddsQueryOptions = (eventId: string) => ({
    queryKey: oddsQueryKeys.eventId(eventId),
    queryFn: () => GetGameOdds(eventId),
    staleTime: 1000 * 60 * 60,  // odds data is considered fresh for 1 hour
    cacheTime: 1000 * 60 * 60 * 2 // odds data is stored in memory for 2 hours
});
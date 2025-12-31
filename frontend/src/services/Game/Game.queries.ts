import { GetGameData } from "./Game.api";

export const gameQueryKeys = {
    all: ["game"] as const,
    eventId: (eventId: string) =>  [...gameQueryKeys.all, eventId] as const,
};

export const gameQueryOptions = (eventId: string) => ({
    queryKey: gameQueryKeys.eventId(eventId),
    queryFn: () => GetGameData(eventId),
    staleTime: 1000 * 60 * 60 * 24 * 6,  // game data is considered fresh for 6 days
    cacheTime: 1000 * 60 * 60 * 24 * 14 // game data is stored in memory for 14 days
});
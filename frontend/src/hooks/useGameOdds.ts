import { useQuery } from "@tanstack/react-query";
import { gameOddsQueryOptions } from "../services/GameOdds/GameOdds.queries";

export function useGameOdds(eventId: string) {
    return useQuery({
        ...gameOddsQueryOptions(eventId || "1"),
        enabled: eventId !== null,
    })
}
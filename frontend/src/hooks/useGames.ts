import { useQuery } from "@tanstack/react-query";
import { gamesQueryOptions } from "../services/Games/Games.queries";

export function useGames(week: number | null) {
    return useQuery({
        ...gamesQueryOptions(week ?? 1),
        enabled: week !== null,
    })
}
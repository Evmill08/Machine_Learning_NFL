import { useQuery } from "@tanstack/react-query";
import { gamesQueryOptions } from "../services/Games/Games.queries";

export function useGames(week: number) {
    console.log("hitting useGames method");
    return useQuery({
        ...gamesQueryOptions(week ?? 1),
        enabled: week !== null,
    })
}
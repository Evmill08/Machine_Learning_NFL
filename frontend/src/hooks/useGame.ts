import { useQuery } from "@tanstack/react-query";
import { gameQueryOptions } from "../services/Game/Game.queries";

export function useGame(eventId: string | undefined) {
    return useQuery({
        ...gameQueryOptions(eventId ?? "1"),
        enabled: eventId !== null,
    })
}
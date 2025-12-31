import { useQuery } from "@tanstack/react-query";
import { predictionQueryOptions } from "../services/Predictions/Predictions.queries.api";

export function usePredictions(eventId: string){
    return useQuery({
        ...predictionQueryOptions(eventId || "1"),
        enabled: eventId !== null,
    })
}
import { useQuery } from "@tanstack/react-query";
import { predictionQueryOptions } from "../services/Predictions/Predictions.queries.api";

export function usePredictions(eventId: string | undefined){
    console.log("Starting predictions");
    return useQuery({
        ...predictionQueryOptions(eventId || "1"),
        enabled: eventId !== null,
    })
}
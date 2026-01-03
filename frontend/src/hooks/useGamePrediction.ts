import { useQuery } from "@tanstack/react-query";
import { predictionQueryOptions } from "../services/Prediction/Prediction.queries.api";

export function usePrediction(eventId: string | undefined){
    console.log("Starting predictions");
    return useQuery({
        ...predictionQueryOptions(eventId || "1"),
        enabled: eventId !== null,
    })
}
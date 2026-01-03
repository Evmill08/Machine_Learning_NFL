//TODO: Think about abstracting this for game odds too
// Have to think a little more about if we really want to cache them for the same amount of time

import { GetGamePrediction } from "./Prediction.api";

export const predictionQueryKeys = {
    all: ["predictions"] as const,
    eventId: (eventId: string) =>  [...predictionQueryKeys.all, eventId] as const,
}

export const predictionQueryOptions = (eventId: string) => ({
    queryKey: predictionQueryKeys.eventId(eventId),
    queryFn: () => GetGamePrediction(eventId), 
    staleTime: 1000 * 60 * 60,  // prediction data is considered fresh for 1 hour
    cacheTime: 1000 * 60 * 60 * 2 // prediction data is stored in memory for 2 hours 
})
// TODO: Cache and run on launch
// This is our most expensive function, need to think about how we want to
// handle loadin this as fast as possible.

import { PredictionData } from "../../models/PredictionData";

export async function GetGamePrediction(eventId: string){
    const url = `http://localhost:5145/prediction/gamePrediction/${eventId}`;
    console.log("Hitting backend prediction endpoint");

    try {
        const predictionResponse = await fetch(url);

        if (!predictionResponse.ok){
            throw new Error(`HTTP Error. Status ${predictionResponse.status}`);
        }

        const predictionData: PredictionData = await predictionResponse.json();
        console.log("Prediction data: ", predictionData);
        return predictionData;

    } catch (error){
        console.error("Error fetching users: ", error);
        return undefined;
    }
}
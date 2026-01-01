import { usePredictions } from "../hooks/useGamePredictions";
import { GameDetailsComponentProps } from "./gameOddsTabComponent";
import {PredictionData } from "../models/PredictionData";

export function GamePredictionTabComponent({eventId}: GameDetailsComponentProps) {
    const {data, isLoading, error} = usePredictions(eventId);

    if (isLoading && !data){
        return <div>Loading Prediction</div>
    }

    const gamePrediction = data as PredictionData;
    console.log("GamePrediction data: ", gamePrediction);
    return (
        <div>
            <h1>Predicted Winner: {gamePrediction.gamePrediction.winnerPrediction}</h1>
            <h1>Predicted Spread: {gamePrediction.gamePrediction.spreadPrediction}</h1>
            <h1>Predicted Total: {gamePrediction.gamePrediction.totalPrediction}</h1>
        </div>
    )

}

export default GamePredictionTabComponent;
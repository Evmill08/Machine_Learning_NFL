import { usePrediction } from "../hooks/useGamePrediction";
import { GameDetailsComponentProps } from "./gameOddsTabComponent";

export function GamePredictionComponent ({eventId}: GameDetailsComponentProps) {

    const {data: gamePrediction, isLoading, error} = usePrediction(eventId);

    return (
        <div>
            <h1>Predicted Winner: {gamePrediction?.gamePrediction.winnerPrediction}</h1>
            <h1>Predicted Spread: {gamePrediction?.gamePrediction.spreadPrediction.toFixed(1)}</h1>
            <h1>Predicted Total: {gamePrediction?.gamePrediction.totalPrediction.toFixed(0)}</h1>
        </div>
    )
}

export default GamePredictionComponent;
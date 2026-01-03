import { usePrediction } from "../hooks/useGamePrediction";
import { GameDetailsComponentProps } from "./gameOddsTabComponent";
import {PredictionData } from "../models/PredictionData";
import GamePredictionComponent from "./gamePredictionComponent";

export function GamePredictionTabComponent({eventId}: GameDetailsComponentProps) {

    return (
        <div>
            <GamePredictionComponent eventId={eventId}/>
        </div>
    )

}

export default GamePredictionTabComponent;
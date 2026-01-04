import { usePrediction } from "../hooks/useGamePrediction";
import { GameDetailsComponentProps } from "./gameOddsTabComponent";
import {PredictionData } from "../models/PredictionData";
import GamePredictionComponent from "./gamePredictionComponent";
import { GamePredictionComponentProps } from "./gamePredictionComponent";

export function GamePredictionTabComponent({eventId, homeTeamId, awayTeamId}: GamePredictionComponentProps) {

    return (
        <div>
            <GamePredictionComponent eventId={eventId} homeTeamId={homeTeamId} awayTeamId={awayTeamId}/>
        </div>
    )

}

export default GamePredictionTabComponent;
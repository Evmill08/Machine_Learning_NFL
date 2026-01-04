import { usePrediction } from "../hooks/useGamePrediction";
import { GameDetailsComponentProps } from "./gameOddsTabComponent";
import styles from "../css/gamePredictionComponent.module.css";
import { TeamLogos } from "../models/TeamLogos";

export interface GamePredictionComponentProps {
    eventId: string | undefined,
    homeTeamId: string | undefined,
    awayTeamId: string | undefined
}

export function GamePredictionComponent ({eventId, homeTeamId, awayTeamId}: GamePredictionComponentProps) {

    const {data: gamePrediction, isLoading, error} = usePrediction(eventId);
    console.log("Prediction Component for event: ", eventId);

    return (
        <div className={styles.gamePredictionContainer}>
            <div className={styles.predictionDetailsContainer}>
                <div className={styles.awayTeamContainer}>
                    <img src={TeamLogos[awayTeamId!]} alt={gamePrediction?.awayTeamName}/> 
                    <h1>{gamePrediction?.awayTeamName}</h1>
                </div>
                <h1>Vs</h1>
                <div className={styles.homeTeamContainer}>
                    <img src={TeamLogos[homeTeamId!]} alt={gamePrediction?.homeTeamName}/>
                    <h1>{gamePrediction?.homeTeamName}</h1>
                </div>
            </div>

            <div className={styles.predictionContainer}>
                <h1>Winner: {gamePrediction?.gamePrediction.winnerPrediction}</h1>
                <h1>Spread: {gamePrediction?.gamePrediction.spreadPrediction.toFixed(1)}</h1>
                
                <h1>Total: {gamePrediction?.gamePrediction.totalPrediction.toFixed(0)}</h1>
                <h1>Home Win Prob: {gamePrediction?.gamePrediction.homeWinProbability.toFixed(2)}</h1>
                
                <h1>Away Win Prob: {gamePrediction?.gamePrediction.awayWinProbability.toFixed(2)}</h1>
                <h1>Range for Total: {gamePrediction?.gamePrediction.totalRange[0].toFixed(0)}, {gamePrediction?.gamePrediction.totalRange[1].toFixed(0)}</h1>
                
                <h1>Range for Spread: {gamePrediction?.gamePrediction.spreadRange[0].toFixed(0)}, {gamePrediction?.gamePrediction.spreadRange[1].toFixed(0)}</h1>
                <h1>Vegas Lowest Spread: {gamePrediction?.vegasLowestSpread.oddsValue.toFixed(1)}: {gamePrediction?.vegasLowestSpread.sportsBook}</h1>
                
                <h1>Vegas Lowest Total: {gamePrediction?.vegasLowestTotal.oddsValue.toFixed(0)}: {gamePrediction?.vegasLowestTotal.sportsBook}</h1>
                <h1>Vegas Winner: {gamePrediction?.vegasWinner == "Home" ? gamePrediction.homeTeamName : gamePrediction?.awayTeamName }</h1>
            </div>
        </div>
    )
}

export default GamePredictionComponent;
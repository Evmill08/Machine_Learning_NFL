import { useState } from "react";
import styles from "../css/gameDetails.module.css";
import { useGame } from "../hooks/useGame";
import { TeamLogos } from "../models/TeamLogos";
import {useParams, useNavigate } from "react-router-dom";
import GamePredictionTabComponent from "../components/gamePredictionTabComponent";
import GameOddsTabComponent from "../components/gameOddsTabComponent";
import { usePrediction } from "../hooks/useGamePrediction";

// Essentially 2 tabs on this page
export function GameDetails() {
    const {eventId} = useParams();
    const navigate = useNavigate();
    const [oddsTabSelected, setOddsTabSelected] = useState(true);

    const {data: gameData, isLoading: isGameLoading, error: gameError} = useGame(eventId);
    const {data: gamePrediction, isLoading: isPredictionLoading, error: predictionError} = usePrediction(eventId);

    // Again need to think about loading and errors more
    if (isGameLoading) return <div>Loading games...</div>;
    if (gameError) return <div>Error loading games</div>;

    return (
        <div className={styles.gameDetailsContainer}>
            <div className={styles.header}>
                <div className={styles.awayTeamContainer}>
                    <img src={TeamLogos[gameData.awayTeamId]}/> 
                    <h1>{gameData.awayTeamName}</h1>
                </div>
                <h1>Vs</h1>
                <div className={styles.homeTeamContainer}>
                    <img src={TeamLogos[gameData.homeTeamId]}/>
                    <h1>{gameData.homeTeamName}</h1>
                </div>
            </div>

            <div className={styles.contentContainer}>
                {/* Sectction for the tabs */}
                <div className={styles.tabContainer}>
                    <div className={oddsTabSelected ? styles.selectedTab : styles.unselectedTab} onClick={() => setOddsTabSelected(true)}>
                        <h1 className={styles.tabHeader}>Odds</h1>
                    </div>

                    <div className={oddsTabSelected ? styles.unselectedTab : styles.selectedTab} onClick={() => setOddsTabSelected(false)}>
                        <h1 className={styles.tabHeader}>Predictions</h1>
                    </div>
                </div>

                {/* The actual content which is changed based on what is pressed  */}
                {/* We need to IMMEDIATLY try and load the predictions since this call takes a long time */}
                <div className={styles.shownContent}>
                    {oddsTabSelected ? (
                        <GameOddsTabComponent eventId={eventId}/>
                    ) : (
                        <GamePredictionTabComponent eventId={eventId}/>
                    )}
                </div>
            </div>

  

            <button className={styles.returnHomeButton} onClick={() => navigate("/")}>Back</button>
        </div>
    );
}   


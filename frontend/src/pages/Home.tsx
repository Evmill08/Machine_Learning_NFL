import { useState, useEffect } from 'react';
import {GetCurrentWeekNumber} from "../services/Games/Games.api";
import { useGames } from '../hooks/useGames';
import { GameData } from '../models/GameData';
import GameDataComponent from '../components/gameDataComponent';
import styles from "../css/home.module.css"
import { useNavigate } from 'react-router-dom';
import { usePrediction } from '../hooks/useGamePrediction';
import { useQueryClient } from '@tanstack/react-query';
import { predictionQueryOptions } from '../services/Prediction/Prediction.queries.api';

export function HomePage() {
    const [weekNumber, setWeekNumber] = useState<number | null>(null);
    const queryClient = useQueryClient();

    useEffect(() => {
        const loadWeekNumber = async () => {
            try {
                const weekNumber = await GetCurrentWeekNumber();
                if (weekNumber){
                    setWeekNumber(weekNumber);
                }
            } catch (error){
                console.log(error);
            }
        };
        loadWeekNumber();
    }, []);

    const navigate = useNavigate();

    const {data: weeklyGames, isLoading, error} = useGames(weekNumber!);
    console.log("We hit use games");

    if (weeklyGames){
        (weeklyGames as GameData[])
        .sort((a, b) => {
            const time_a = new Date(a.date);
            const time_b = new Date(b.date);
            return time_a.getHours() - time_b.getHours();
        })
    }

    useEffect(() => {
        if (!weeklyGames) return;

        weeklyGames.forEach((game: GameData) => {
            queryClient.prefetchQuery(
                predictionQueryOptions(game.eventId)
            );
        });
    }, [weeklyGames, queryClient]);

    if (isLoading) return <div>Loading games...</div>;
    if (error) return <div>Error loading games</div>;

    return (
        <div className={styles.homeContainer}>
            <div className={styles.homeHeader}>
                <img src="\NFL_logos\National_Football_League_logo.png"/>
                <h1 className={styles.homeHeaderText}>Weekly Games</h1>
            </div>
            <div className={styles.scrollingContainer}>
                <div className={styles.scrollingText}>We are in NO way affiliated with the NFL. This is an independent project meant for education and personal use only.
                    This site is also in no way affiliated with any of the sports betting sites whose information is used to display certain odds. Please use the information in
                    this website wisely. We are not betting professionals, nor are we telling you what to do with your money.
                </div>
            </div>

            {/*Add loading wheel or something cool */}
            {isLoading ?? (
                <div>Loading games...</div>
            )}
            
            <ul>
                <div className={styles.gameGrid} >
                    {weeklyGames?.map((g: GameData) => (
                        <GameDataComponent gameData={g}
                        key={g.eventId}/>
                    ))}
                </div>
            </ul>

            <div className={styles.predictionButtonContainer}>
                <button className={styles.predictionButton} onClick={() => navigate(`/predictions/${weekNumber}`)}>View All Predictions</button>
                <h1 className={styles.predictionDisclaimer}>Disclaimer: Viewing all predictions may take up to 2 minutes. Please be patient while we load the predictions!</h1>
            </div>
        </div>
    );
}

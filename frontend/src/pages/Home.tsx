import { useState, useEffect } from 'react';
import {GetCurrentWeekNumber} from "../services/Games/Games.api";
import { useGames } from '../hooks/useGames';
import { GameData } from '../models/GameData';
import GameDataComponent from '../components/gameDataComponent';
import styles from "../css/home.module.css"

export function HomePage() {
    const [weekNumber, setWeekNumber] = useState<number | null>(null);

    // TODO: Fix this, cache the week number
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

    // TODO: When the home page is loaded, we should IMMEDIATLY be getting predictions for 2-4 games at a time in batches
    // Need to figure out how to do this. 
    const {data: weeklyGames, isLoading, error} = useGames(weekNumber);

    if (weeklyGames){
        (weeklyGames as GameData[])
        .sort((a, b) => {
            const time_a = new Date(a.date);
            const time_b = new Date(b.date);
            return time_a.getHours() - time_b.getHours();
        })
    }

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

            {/*Have to fix this, but we also want to handle these errors more gracefully anyway */}
            {/*{error ?? (
                <div>Error loading games</div>
            )}*/}
            

            <ul>
                <div className={styles.gameGrid} >
                    {weeklyGames?.map((g: GameData) => (
                        <GameDataComponent gameData={g}
                        key={g.eventId}/>
                    ))}
                </div>
            </ul>
        </div>
    );

}
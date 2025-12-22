import { useState, useEffect } from 'react';
import {GameData} from "./models/GameData";
import {GameOdds} from "./models/GameOdds";
import {PredictionData} from "./models/PredictionData";
import {GetGameData, GetCurrentWeekNumber} from "./services/Games/Games.api";
import {GetGameOdds} from "./services/GameOdds/GameOdds.api";
import {GetGamePrediction} from "./services/Predictions/Predictions.api"; 
import { useGames } from './hooks/useGames';

export function MainPageExample() {
    const [weekNumber, setWeekNumber] = useState<number | null>(null);

    // TODO: Getting a weird error in the network from this
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

    const {data: weeklyGames, isLoading, error} = useGames(weekNumber);

    if (isLoading) return <div>Loading games...</div>;
    if (error) return <div>Error loading games</div>;

    return (
        <div className="p-4">
            <h1 className="text-xl font-bold mb-4">Weekly Games</h1>

            <ul className="space-y-4">
                {weeklyGames?.map((g) => (
                    <li key={g.eventId} className="p-4 rounded-xl shadow bg-white">
                        
                        {/* Header */}
                        <div className="text-lg font-semibold">
                            {g.homeTeamName} vs {g.awayTeamName}
                        </div>
                        <div className="text-sm text-gray-500 mb-3">
                            {new Date(g.date).toLocaleString()}
                        </div>
        
                    </li>
                ))}
            </ul>
        </div>
    );

}
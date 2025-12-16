import { useState, useEffect } from 'react';
import {GameData} from "./models/GameData";
import {GameOdds} from "./models/GameOdds";
import {PredictionData} from "./models/PredictionData";
import {GetGameData} from "../src/services/GameDataService";
import {GetGameOdds} from "../src/services/GameOddsService";
import {GetGamePrediction} from "../src/services/PredictionService";

export function MainPageExample() {
    const [weeklyGames, setWeeklyGames] = useState<GameData[]>([]);
    const [gameOdds, setGameOdds] = useState<Record<string, GameOdds[]>>({});
    const [gamePredictions, setGamePredictions] = useState<Record<string, PredictionData>>({});


    // TODO: Getting a weird error in the network from this
    useEffect(() => {
        const loadGames = async () => {
            try {
                const data = await GetGameData();
                if (data){
                    setWeeklyGames(data);
                }
            } catch (error){
                console.log(error);
            }
        }

        loadGames();
    }, []);

    useEffect(() => {
        if (weeklyGames.length == 0){
            return;
        }

        const loadOdds = async () => {
            try{
                const results = await Promise.all(
                    weeklyGames.map(async (g) => {
                        const odds = await GetGameOdds(g.eventId);
                        return [g.eventId, odds] as const;
                    })
                );

                const record: Record<string, GameOdds[]> = {};
                results.forEach(([id, odds]) => {
                    if (odds){
                        record[id] = odds;
                    }
                });

                setGameOdds(record);
            } catch (error){
                console.error(error);
            }
        }

        loadOdds();
    }, [weeklyGames]);

    useEffect(() => {
        const loadPredictions = async () => {
            try{
                const results = await Promise.all(
                    weeklyGames.map(async (g) => {
                        const predictions = await GetGamePrediction(g.eventId);
                        return [g.eventId, predictions]as const;
                    })
                );

                const record: Record<string, PredictionData> = {};
                results.forEach(([id, p]) => {
                    if (p){
                        record[id] = p;
                    }
                });

                setGamePredictions(record);
            } catch(error){
                console.error(error);
            }
        }

        loadPredictions();

    }, [weeklyGames]);

    return (
        <div className="p-4">
            <h1 className="text-xl font-bold mb-4">Weekly Games</h1>

            <ul className="space-y-4">
                {weeklyGames.map((g) => (
                    <li key={g.eventId} className="p-4 rounded-xl shadow bg-white">
                        
                        {/* Header */}
                        <div className="text-lg font-semibold">
                            {g.homeTeamName} vs {g.awayTeamName}
                        </div>
                        <div className="text-sm text-gray-500 mb-3">
                            {new Date(g.date).toLocaleString()}
                        </div>

                        {/* Odds */}
                        <div className="mb-4">
                            <h3 className="font-medium text-gray-700">Vegas Odds</h3>

                            {gameOdds[g.eventId] ? (
                                <ul className="ml-2 list-disc text-sm">
                                    {gameOdds[g.eventId].map((o, i) => (
                                        <li key={i}>
                                            Provider: <b>{o.provider}</b> — Spread: {o.spread}, Total: {o.total}
                                            <br/>
                                            <span className="text-gray-500">{o.details}</span>
                                        </li>
                                    ))}
                                </ul>
                            ) : (
                                <div className="text-sm text-gray-400">Loading odds…</div>
                            )}
                        </div>

                        {/* Prediction */}
                        <div className="mb-4">
                            <h3 className="font-medium text-gray-700">Model Prediction</h3>
                            
                            {gamePredictions[g.eventId] ? (
                                <div className="text-sm ml-2 space-y-1">
                                    <div>
                                        Winner: <b>{gamePredictions[g.eventId].gamePrediction.winnerPrediction}</b> 
                                        ({gamePredictions[g.eventId].gamePrediction.winnerConfidence}%)
                                    </div>

                                    <div>
                                        Spread Prediction: <b>{gamePredictions[g.eventId].gamePrediction.spreadPrediction}</b>
                                    </div>

                                    <div>
                                        Total Prediction: <b>{gamePredictions[g.eventId].gamePrediction.totalPrediction}</b>
                                    </div>

                                    <div className="text-gray-600 mt-2">
                                        Home Win Probability: {gamePredictions[g.eventId].gamePrediction.homeWinProbability}%
                                    </div>
                                    <div className="text-gray-600">
                                        Away Win Probability: {gamePredictions[g.eventId].gamePrediction.awayWinProbability}%
                                    </div>
                                </div>
                            ) : (
                                <div className="text-sm text-gray-400">Loading prediction…</div>
                            )}
                        </div>

                        {/* Vegas Summary */}
                        {gamePredictions[g.eventId] && (
                            <div>
                                <h3 className="font-medium text-gray-700">Vegas Summary</h3>
                                <div className="text-sm ml-2 space-y-1">
                                    <div>
                                        Lowest Spread: {gamePredictions[g.eventId].vegasLowestSpread.sportsBook} — 
                                        {gamePredictions[g.eventId].vegasLowestSpread.oddsValue}
                                    </div>

                                    <div>
                                        Lowest Total: {gamePredictions[g.eventId].vegasLowestTotal.sportsBook} — 
                                        {gamePredictions[g.eventId].vegasLowestTotal.oddsValue}
                                    </div>

                                    <div>
                                        Vegas Winner: <b>{gamePredictions[g.eventId].vegasWinner}</b>
                                    </div>
                                </div>
                            </div>
                        )}
                    </li>
                ))}
            </ul>
        </div>
    );

}
import { useParams } from "react-router-dom";
import { useGames } from "../hooks/useGames"
import { GameData } from "../models/GameData";
import { usePrediction } from "../hooks/useGamePrediction";
import GamePredictionComponent from "../components/gamePredictionComponent";


export function AllPredictions() {
    const {weekNumber} = useParams();
    const {data: weeklyGames, isLoading: gamesAreLoading, error: gamesError} = useGames(parseInt(weekNumber!));

    if (weeklyGames){
        (weeklyGames as GameData[])
        .sort((a, b) => {
            const time_a = new Date(a.date);
            const time_b = new Date(b.date);
            return time_a.getHours() - time_b.getHours();
        })
    }

    return (
        <div>
            {weeklyGames.map((game: GameData, idx: number) => {
                <GamePredictionComponent eventId={game.eventId} key={idx}/>
            })}
        </div>
    )
}


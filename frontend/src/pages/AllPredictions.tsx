import { useParams } from "react-router-dom";
import { useGames } from "../hooks/useGames"
import { GameData } from "../models/GameData";
import GamePredictionComponent from "../components/gamePredictionComponent";
import styles from "../css/allPredictions.module.css"
import { useNavigate } from "react-router-dom";


export function AllPredictions() {
    const {weekNumber} = useParams();
    const {data: weeklyGames, isLoading: gamesAreLoading, error: gamesError} = useGames(parseInt(weekNumber!));
    const navigate = useNavigate();

    if (weeklyGames){
        (weeklyGames as GameData[])
        .sort((a, b) => {
            const time_a = new Date(a.date);
            const time_b = new Date(b.date);
            return time_a.getHours() - time_b.getHours();
        })
    }

    return (
        <div className={styles.allPredictionsContainer}>
            <h1>All Predictions</h1> 
            <div className={styles.predictionsGrid}>
                {weeklyGames.map((game: GameData) => (
                    <GamePredictionComponent
                    key={game.eventId}
                    eventId={game.eventId}
                    homeTeamId={game.homeTeamId}
                    awayTeamId={game.awayTeamId}
                />
                ))}
            </div>

            <div className={styles.backButtonContainer}>
                <button className={styles.backButton} onClick={() => navigate("/")}>Return Home</button>
            </div>
        </div>


    )
}


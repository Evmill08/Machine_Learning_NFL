import { GameData } from "../models/GameData";
import styles from "../css/gameDataComponent.module.css";
import { TeamLogos } from "../models/TeamLogos";
import { useNavigate } from "react-router-dom";

export interface GameDataComponentProps {
    gameData: GameData
}

export function GameDataComponent({gameData}: GameDataComponentProps){
    const dateObj = new Date(gameData.date);
    const navigate = useNavigate();

    return (
        <div className={styles.gameContainer} onClick={() => navigate(`/game/${gameData.eventId}`)}>
            <div className={styles.teamContainer}>
                {/* Make this below a component, we use the same thing basically in the gameDetails page at the top */}
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

            <div className={styles.detailsContainer}>
                <h1>{dateObj.toDateString()}</h1>  
                <h1>{dateObj.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</h1>         
            </div>
        </div>
    )
}

export default GameDataComponent



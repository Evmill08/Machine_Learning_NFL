import { GameData } from "../models/GameData";
import styles from "../css/gameDataComponent.module.css";
import { TeamLogos } from "../models/TeamLogos";

interface GameDataComponentProps {
    gameData: GameData
}

export function GameDataComponent({gameData}: GameDataComponentProps){
    const dateObj = new Date(gameData.date);

    return (
        <div className={styles.gameContainer}>
            <div className={styles.teamContainer}>
                <div className={styles.homeTeamContainer}>
                    <img src={TeamLogos[gameData.homeTeamId]}/>
                    <h1>{gameData.homeTeamName}</h1>
                </div>
                <h1>Vs</h1>
                <div className={styles.awayTeamContainer}>
                    <img src={TeamLogos[gameData.awayTeamId]}/> 
                    <h1>{gameData.awayTeamName}</h1>
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



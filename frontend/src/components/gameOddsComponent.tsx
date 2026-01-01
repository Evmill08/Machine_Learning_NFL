import { GameOdds } from "../models/GameOdds";
import styles from "../css/gameOddsComponent.module.css"

interface GameOddsComponentProps {
    gameOdds: GameOdds;
} 

export function GameOddsComponent({gameOdds}: GameOddsComponentProps) {
    console.log("Game odds have been loaded");
    return (
        <div className={styles.gameOddsContainer}>
            <h1>Odds: {gameOdds.details}</h1>
            <h1>Total: {gameOdds.overUnder}</h1>
            <h1>Spread: {gameOdds.spread}</h1>
            <h1>Provider: {gameOdds.provider}</h1>
        </div>
    )
}

export default GameOddsComponent;
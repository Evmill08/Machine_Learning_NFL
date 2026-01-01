import { useGameOdds } from "../hooks/useGameOdds";
import styles from "../css/gameOddsTabComponent.module.css"
import GameOddsComponent from "./gameOddsComponent";
import { GameOdds } from "../models/GameOdds";

export interface GameDetailsComponentProps {
    eventId: string | undefined;
}

export function GameOddsTabComponent({eventId}: GameDetailsComponentProps) {

    const {data: gameOdds, isLoading, error} = useGameOdds(eventId);

    // Again need to think about loading and errors more
    if (isLoading) return <div>Loading games...</div>;
    if (error) return <div>Error loading games</div>;
    console.log("odds tab has been loaded");
    console.log("GameOdds: ", gameOdds);

    return (
        <div className={styles.oddsContainer}>
           {gameOdds.map((odds: GameOdds, idx: number) => (
            <GameOddsComponent gameOdds={odds} key={idx}/>
           ))}
        </div>
    );
}

export default GameOddsTabComponent;
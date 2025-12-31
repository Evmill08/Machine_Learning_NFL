export interface GameDetailsComponentProps {
    eventId: string | undefined;
}

export function GameOddsComponent({eventId}: GameDetailsComponentProps) {
    return (
        <div>
            <h1>This is where Game Odds go</h1>
        </div>
    );
}

export default GameOddsComponent;
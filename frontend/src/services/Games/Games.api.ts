import { GameData } from "../../models/GameData";

export async function GetCurrentWeekNumber(): Promise<number | null> {
    const url = `http://localhost:5145/game/currentWeek`;

    try {
        const gameResponse = await fetch(url);

        if (!gameResponse.ok){
            throw new Error(`HTTP Error. Status ${gameResponse.status}`);
        }

        const gameData: number = await gameResponse.json();
        return gameData;

    } catch (error){
        console.error("Error fetching users: ", error);
        return null;
    }
}

export async function GetGameData(week: number): Promise<GameData[]>{
    const url = `http://localhost:5145/game/currentWeekGames`;
    
    const gameResponse = await fetch(url);

    if (!gameResponse.ok){
        throw new Error(`HTTP Error. Status ${gameResponse.status}`);
    }

    return gameResponse.json();
}
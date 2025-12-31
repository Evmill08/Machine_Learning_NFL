import { GameData } from "../../models/GameData";

export async function GetCurrentWeekNumber(): Promise<number | null> {
    const url = `http://localhost:5145/game/currentWeek`;

    try {
        const currentWeekResponse = await fetch(url);
        console.log("currentWeekResponse: ", currentWeekResponse);

        if (!currentWeekResponse.ok){
            throw new Error(`HTTP Error. Status ${currentWeekResponse.status}`);
        }

        const currentWeekNumber: number = await currentWeekResponse.json();
        console.log("currentWeekNumber: ", currentWeekNumber);
        return currentWeekNumber;

    } catch (error){
        console.error("Error fetching users: ", error);
        return null;
    }
}

export async function GetWeeklyGameData(week: number){    
    console.log("Getting games...");
    const gameResponse = await fetch(`/game/currentWeekGames/${week}`);

    if (!gameResponse.ok){
        throw new Error(`HTTP Error. Status ${gameResponse.status}`);
    }

    const gameData = await gameResponse.json();
    console.log("gameData: ", gameData);
    return gameData;
}


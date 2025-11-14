// TODO: Store this in async storage to keep this from having to reach out constantly
// This will also run on app launch

import { GameData } from "../models/GameData";

export async function GetGameData(eventId: number): Promise<Array<GameData> | undefined>{
    const url = `http://localhost:5145/game/currentWeekGames`;

    try {
        const gameResponse = await fetch(url);

        if (!gameResponse.ok){
            throw new Error(`HTTP Error. Status ${gameResponse.status}`);
        }

        const gameData: Array<GameData> = await gameResponse.json();
        return gameData;

    } catch (error){
        console.error("Error fetching users: ", error);
        return undefined;
    }
}
// TODO: Cache and run on app launch 

import { GameOdds } from "../models/GameOdds";

export async function GetGameOdds(eventId: number): Promise<Array<GameOdds> | undefined> {
    var url = `http://localhost:5145/odds/allOdds/${eventId}`;

    try {
        const oddsResponse = await fetch(url);

        if (!oddsResponse.ok){
            throw new Error(`HTTP Error. Status ${oddsResponse.status}`);
        }

        const gameOdds: Array<GameOdds> = await oddsResponse.json();
        return gameOdds;
    } catch (error){
        console.error("Error fetching users: ", error);
        return undefined;
    }
}
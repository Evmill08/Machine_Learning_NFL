// TODO: Cache and run on app launch 

import { GameOdds } from "../../models/GameOdds";

export async function GetGameOdds(eventId: string): Promise<GameOdds[]>{
    var url = `http://localhost:5145/odds/allOdds/${eventId}`;

    const oddsResponse = await fetch(url);

    if (!oddsResponse.ok){
        throw new Error(`HTTP Error. Status ${oddsResponse.status}`);
    }

    return await oddsResponse.json();
}
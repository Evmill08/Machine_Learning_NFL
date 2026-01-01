export async function GetGameOdds(eventId: string){
    var url = `http://localhost:5145/odds/allOdds/${eventId}`;

    const oddsResponse = await fetch(url);

    if (!oddsResponse.ok){
        throw new Error(`HTTP Error. Status ${oddsResponse.status}`);
    }

    const oddsResponseJson = await oddsResponse.json();
    console.log("Odds Response: ", oddsResponseJson);

    return oddsResponseJson;
}
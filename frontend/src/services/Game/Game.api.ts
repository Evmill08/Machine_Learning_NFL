export async function GetGameData(eventId: string) {
    const gameResponse = await fetch(`/game/${eventId}`);

    if (!gameResponse.ok){
        throw new Error(`HTTP Error. Status ${gameResponse.status}`);
    }

    const gameData = await gameResponse.json();
    console.log("gameData: ", gameData);
    return gameData;
}
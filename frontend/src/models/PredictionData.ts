export interface GamePrediction{
    spreadPrediction: number;
    spreadRange: Array<number>;
    totalPrediction: number;
    totalRange: Array<number>;
    winnerPrediction: string;
    homeWinProbability: number;
    awayWinProbability: number;
}

interface VegasPrediction{
    sportsBook: string;
    oddsValue: number
}

export interface PredictionData{
    homeTeamName: string;
    awayTeamName: string;
    eventId: string;
    gamePrediction: GamePrediction;
    vegasLowestSpread: VegasPrediction;
    vegasLowestTotal: VegasPrediction;
    vegasWinner: string;
}


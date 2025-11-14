export interface GamePrediction{
    spreadPrediction: number;
    spreadRange: Array<number>;
    spreadConfidence: Array<number>;
    totalPrediction: number;
    totalRange: Array<number>;
    totalConfidence: Array<number>;
    winnerPrediction: string;
    winnerConfidence: number;
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


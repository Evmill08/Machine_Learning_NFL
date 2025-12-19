from pydantic import BaseModel
from typing import Optional

class PredictionData(BaseModel):
    TotalPoints: int
    Spread: int  # Positive if home team
    HomeWin: int  # Money line, 0 if home
    SeasonYear: int
    WeekNumber: int
    EventId: str
    HomeTeamName: str
    AwayTeamName: str
    HomeTeamId: str
    AwayTeamId: str
    HomeWinner: int  # 1 if true
    AwayWinner: int  # 1 if true
    AveragePredictedTotal: float
    AveragePredictedSpread: float
    BestPredictedTotal: float
    BestPredictedSpread: float
    MeanTemperature: float
    MaxTemperature: float
    MinTemperature: float
    ApparentTemperature: float
    PrecipitationSum: float
    SnowfallSum: float
    PrecipitationHours: float
    RainSum: float
    WindSpeedMax: float
    WindGustsMax: float
    DominantWindDirection: float
    MeanRelativeHumidity: float
    MeanWindGusts: float
    MeanWindSpeed: float
    PredictedHomeWinPercent: float
    PredicitedHomeMatchupQuality: float
    PredictedHomeLossPercent: float
    PredictedHomeDefenseEfficiency: float
    PredictedHomeOffenseEfficiency: float
    PredictedHomeTotalEfficiency: float
    PredictedHomePointDifferential: float
    PredictedAwayWinPercent: float
    PredicitedAwayMatchupQuality: float
    PredictedAwayLossPercent: float
    PredictedAwayPointDifferential: float
    PredictedAwayDefenseEfficiency: float
    PredictedAwayOffenseEfficiency: float
    PredictedAwayTotalEfficiency: float


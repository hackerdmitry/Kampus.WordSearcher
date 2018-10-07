namespace Kampus.WordSearcher
{
    public interface IGameClient
    {
        Result<SessionInfo> InitSession();
        Result<PointsStatistic> FinishSession();
        Result<FullStatistic> GetStatistics();
        Result<bool[,]> MakeMove(Direction direction);
    }
}
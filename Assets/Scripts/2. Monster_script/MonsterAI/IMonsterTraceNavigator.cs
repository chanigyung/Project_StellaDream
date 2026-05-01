public interface IMonsterTraceNavigator
{
    void Initialize(MonsterContext context);
    MonsterTraceMovePlan CalculateMove(MonsterContext context);
}

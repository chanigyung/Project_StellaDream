public interface IMonsterAction
{
    bool CanExecute(MonsterContext context);
    void Execute(MonsterContext context);
}

public enum ActionType { Attack, Trace, Wander, }
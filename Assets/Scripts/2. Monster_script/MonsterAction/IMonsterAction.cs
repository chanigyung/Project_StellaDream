public interface IMonsterAction
{
bool CanExecute(MonsterContext context);
void Execute(MonsterContext context);
}
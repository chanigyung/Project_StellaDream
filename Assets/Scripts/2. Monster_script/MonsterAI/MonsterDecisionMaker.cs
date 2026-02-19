using System.Collections.Generic;
using UnityEngine;

public class MonsterDecisionMaker : MonoBehaviour
{
    private List<IMonsterAction> actions = new();
    private MonsterContext context;

    public void Initialize(MonsterContext ctx)
    {
        context = ctx;
    }

    public void AddAction(IMonsterAction action)
    {
        actions.Add(action);
    }

    // 외부에서 액션 리스트를 한 번에 주입하기 위한 함수
    public void SetActions(List<IMonsterAction> actionList)
    {
        actions = actionList ?? new List<IMonsterAction>();
    }

    // 액션 리스트 초기화
    public void ClearActions()
    {
        actions.Clear();
    }

    void Update()
    {
        context.UpdateContext();
        DecideAndExecute();
    }

    public void DecideAndExecute()
    {
        foreach (var action in actions)
        {
            if (action.CanExecute(context))
            {
                action.Execute(context);
                break; // 하나만 실행
            }
        }
    }
}

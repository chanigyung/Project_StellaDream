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
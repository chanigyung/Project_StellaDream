using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LockRule
{
    public SkillLockHook hook;
    public SkillLockReason reason;
    public bool addLock = true;
}

[CreateAssetMenu(menuName = "SkillModule/Lock")]
public class LockModuleData : SkillModuleData
{
    [Header("Hook별 Lock 추가/해제 규칙")]
    public List<LockRule> ruleList = new();

    public override ISkillModule CreateModule()
    {
        return new LockModule(this);
    }
}
using UnityEngine;

public interface ISkillModule
{
    void OnDelay(SkillContext context);
    void OnExecute(SkillContext context);
    void OnObjectSpawned(SkillContext context);
    void OnHit(SkillContext context);
    void OnTick(SkillContext context);
    void OnExpire(SkillContext context);
    void OnPostDelay(SkillContext context);
}

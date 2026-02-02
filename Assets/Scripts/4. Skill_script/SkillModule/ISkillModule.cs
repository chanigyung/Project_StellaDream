using UnityEngine;

public interface ISkillModule
{
    void OnDelay(GameObject attacker);
    void OnExecute(GameObject attacker, Vector2 direction);
    void OnHit(GameObject attacker, GameObject target);
    void OnTick(GameObject attacker);
    void OnExpire(GameObject attacker);
    void OnPostDelay(GameObject attacker);
}

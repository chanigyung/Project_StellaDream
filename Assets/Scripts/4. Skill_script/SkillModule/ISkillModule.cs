using UnityEngine;

public interface ISkillModule
{
    void OnDelay(GameObject attacker, Vector2 direction);
    void OnExecute(GameObject attacker, Vector2 direction);
    void OnHit(GameObject attacker, GameObject target);
    void OnTick(GameObject attacker, GameObject target, GameObject sourceObject);
    void OnExpire(GameObject attacker, GameObject sourceObject);
    void OnPostDelay(GameObject attacker, Vector2 direction);
}

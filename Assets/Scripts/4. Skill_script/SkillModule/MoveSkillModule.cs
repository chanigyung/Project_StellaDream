using UnityEngine;

public class MoveSkillModule : SkillModuleBase
{
    private readonly MoveSkillModuleData data;

    public MoveSkillModule(MoveSkillModuleData data)
    {
        this.data = data;
    }

    public override void OnExecute(SkillContext context)
    {
        if (data == null)
            return;

        if (context.attacker == null)
            return;

        UnitMovement movement = context.attacker.GetComponent<UnitMovement>();
        if (movement == null)
            return;

        Vector2 moveDirection = context.hasDirection && context.direction.sqrMagnitude > 0.0001f
            ? context.direction.normalized
            : Vector2.right;

        movement.StartMoveSkill(moveDirection, data.distance, data.duration);
    }

    public override void OnHit(SkillContext context)
    {
        if (data == null)
            return;

        if (!data.stopOnHit)
            return;

        if (context.attacker == null)
            return;

        UnitMovement movement = context.attacker.GetComponent<UnitMovement>();
        if (movement == null)
            return;

        movement.StopMoveSkill();
    }
}
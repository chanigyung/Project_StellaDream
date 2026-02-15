using UnityEngine;

public struct VFXContext
{
    public GameObject caster;
    public GameObject target;
    public GameObject sourceObject;
    public Vector2 direction;
    public bool hasDirection;
}

public class VFXModule : SkillModuleBase
{
    private readonly VFXModuleData data;

    public VFXModule(SkillInstance owner, VFXModuleData data) : base(owner)
    {
        this.data = data;
    }

    public override void OnDelay(GameObject attacker, Vector2 direction)
    {
        var ctx = new VFXContext { caster = attacker, direction = direction, hasDirection = true };
        Play(VFXHook.Delay, ctx);
    }

    public override void OnExecute(GameObject attacker, Vector2 direction)
    {
        var ctx = new VFXContext { caster = attacker, direction = direction, hasDirection = true };
        Play(VFXHook.Execute, ctx);
    }

    public override void OnPostDelay(GameObject attacker, Vector2 direction)
    {
        var ctx = new VFXContext { caster = attacker, direction = direction, hasDirection = true };
        Play(VFXHook.PostDelay, ctx);
    }

    public override void OnHit(GameObject attacker, GameObject target)
    {
        var ctx = new VFXContext { caster = attacker, target = target, hasDirection = false };
        Play(VFXHook.Hit, ctx);
    }

    public override void OnTick(GameObject attacker, GameObject target, GameObject sourceObject)
    {
        var ctx = new VFXContext { caster = attacker, target = target, sourceObject = sourceObject, hasDirection = false };
        Play(VFXHook.Tick, ctx);
    }

    public override void OnExpire(GameObject attacker, GameObject sourceObject)
    {
        var ctx = new VFXContext { caster = attacker, sourceObject = sourceObject, hasDirection = false };
        Play(VFXHook.Expire, ctx);
    }

    private void Play(VFXHook hook, VFXContext ctx)
    {
        if (data == null || data.vfxEntryList == null || data.vfxEntryList.Count == 0) return;

        for (int i = 0; i < data.vfxEntryList.Count; i++)
        {
            var entry = data.vfxEntryList[i];
            if (entry == null) continue;
            if (entry.hook != hook) continue;
            if (entry.prefab == null) continue;

            GameObject spawnOwner = ResolveAnchorOwner(entry.anchor, ctx);

            Vector2 dir = ResolveVFXDirection(entry.useDirection, ctx);

            SkillUtils.SpawnVFX(spawnOwner, owner, dir, entry);
        }
    }

    private GameObject ResolveAnchorOwner(VFXAnchor anchor, VFXContext ctx)
    {
        switch (anchor)
        {
            case VFXAnchor.Target:
                return ctx.target != null ? ctx.target : ctx.caster;

            case VFXAnchor.SourceObject:
                return ctx.sourceObject != null ? ctx.sourceObject : ctx.caster;

            default:
                return ctx.caster;
        }
    }

    private Vector2 ResolveVFXDirection(bool useDirection, VFXContext ctx)
    {
        if (!useDirection)
            return Vector2.right;

        if (ctx.hasDirection)
            return ctx.direction;

        // hit/tick/expire 훅에서 방향이 필요하다면, 타겟이 있으면 caster->target 방향을 사용
        if (ctx.caster != null && ctx.target != null)
        {
            Vector2 dir = (ctx.target.transform.position - ctx.caster.transform.position);
            if (dir.sqrMagnitude > 0.0001f)
                return dir.normalized;
        }

        // fallback
        return Vector2.right;
    }

}

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

            Vector3 basePos = ResolveAnchorPosition(entry.anchor, ctx);
            Vector3 spawnPos = basePos + entry.localOffset;

            Quaternion rot = Quaternion.identity;
            if (entry.useDirection)
            {
                Vector2 dir = ctx.hasDirection ? ctx.direction : Vector2.right;
                rot = SkillUtils.CalculateRotation(dir);
            }

            SkillUtils.SpawnVFX(entry.prefab, spawnPos, rot, entry.animator, entry.trigger);
        }
    }

    private Vector3 ResolveAnchorPosition(VFXAnchor anchor, VFXContext ctx)
    {
        switch (anchor)
        {
            case VFXAnchor.Caster:
                return ctx.caster != null ? ctx.caster.transform.position : Vector3.zero;

            case VFXAnchor.Target:
                return ctx.target != null ? ctx.target.transform.position
                    : (ctx.caster != null ? ctx.caster.transform.position : Vector3.zero);

            case VFXAnchor.SourceObject:
                return ctx.sourceObject != null ? ctx.sourceObject.transform.position
                    : (ctx.caster != null ? ctx.caster.transform.position : Vector3.zero);

            default:
                return ctx.caster != null ? ctx.caster.transform.position : Vector3.zero;
        }
    }
}

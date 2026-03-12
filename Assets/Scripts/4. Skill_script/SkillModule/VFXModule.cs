using UnityEngine;

public class VFXModule : SkillModuleBase
{
    private readonly VFXModuleData data;

    public VFXModule(VFXModuleData data)
    {
        this.data = data;
    }

    public override void OnDelay(SkillContext context)
    {
        Play(VFXHook.Delay, context);
    }

    public override void OnExecute(SkillContext context)
    {
        Play(VFXHook.Execute, context);
    }

    public override void OnObjectSpawned(SkillContext context)
    {
        Play(VFXHook.SourceSpawned, context);
    }

    public override void OnPostDelay(SkillContext context)
    {
        Play(VFXHook.PostDelay, context);
    }

    public override void OnHit(SkillContext context)
    {
        Play(VFXHook.Hit, context);
    }

    public override void OnTick(SkillContext context)
    {
        Play(VFXHook.Tick, context);
    }

    public override void OnExpire(SkillContext context)
    {
        Play(VFXHook.Expire, context);
    }

    private void Play(VFXHook hook, SkillContext context)
    {
        if (data == null || data.vfxEntryList == null || data.vfxEntryList.Count == 0) return;

        for (int i = 0; i < data.vfxEntryList.Count; i++)
        {
            var entry = data.vfxEntryList[i];
            if (entry == null) continue;
            if (entry.hook != hook) continue;
            if (entry.prefab == null) continue;

            SkillContext vfxContext = CreateVFXContextForEntry(entry, context);
            SkillUtils.SpawnVFX(vfxContext, entry);
        }
    }

    // VFX Anchor에 맞춰 스폰용 SkillContext 재구성
    private SkillContext CreateVFXContextForEntry(VFXEntry entry, SkillContext context)
    {
        GameObject anchorObject = ResolveAnchorObject(entry.anchor, context);
        Vector2 dir = ResolveVFXDirection(entry.useDirection, context);

        SkillContext vfxContext = context.Clone();
        vfxContext.contextOwner = anchorObject;

        if (anchorObject != null)
        {
            vfxContext.position = anchorObject.transform.position;
            vfxContext.rotation = anchorObject.transform.rotation;
        }
        else
        {
            vfxContext.position = context.position;
            vfxContext.rotation = context.rotation;
        }

        vfxContext.direction = dir;
        vfxContext.hasDirection = entry.useDirection;
        vfxContext.spawnPointType = entry.spawnPointType;

        return vfxContext;
    }

    private GameObject ResolveAnchorObject(VFXAnchor anchor, SkillContext context)
    {
        switch (anchor)
        {
            case VFXAnchor.Target:
                return context.targetObject;

            case VFXAnchor.Object:
                return context.sourceObject;

            default:
                return context.attacker;
        }
    }

    private Vector2 ResolveVFXDirection(bool useDirection, SkillContext context)
    {
        if (!useDirection)
        return Vector2.right;

        if (context.hasDirection && context.direction.sqrMagnitude > 0.0001f)
            return context.direction.normalized;

        if (context.attacker != null && context.targetObject != null)
        {
            Vector2 dir = context.targetObject.transform.position - context.attacker.transform.position;
            if (dir.sqrMagnitude > 0.0001f)
                return dir.normalized;
        }

        return Vector2.right;
    }

}

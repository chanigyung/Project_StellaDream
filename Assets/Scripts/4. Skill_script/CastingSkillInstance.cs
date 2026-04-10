using UnityEngine;

public class CastingSkillInstance : SkillInstance
{
    private readonly CastingSkillData castingData;

    public override SkillUseType UseType => SkillUseType.Casting;

    public float MaxCastTime => castingData != null ? Mathf.Max(0f, castingData.maxCastTime) : 0f;
    public float CastTickInterval => castingData != null ? Mathf.Max(0f, castingData.castTickInterval) : 0f;
    
    public CastingSkillInstance(CastingSkillData data) : base(data)
    {
        castingData = data;
    }

    public void BeginCast(SkillContext context)
    {
        Delay(context);
    }

    // 캐스팅 진행 중 실제 발동 함수
    public void StartCast(SkillContext context)
    {
        Execute(context);
    }

    // 캐스팅 지속 중 발동 함수
    public void TickCast(SkillContext context)
    {
        OnTick(context);
    }

    // 캐스팅 종료 함수
    public void EndCast(SkillContext context)
    {
        OnExpire(context);
        PostDelay(context);
    }

    // 캐스팅 취소 시 정리 함수
    public void CancelCast()
    {
        ReleaseAllSpawnedObjects();
    }
}

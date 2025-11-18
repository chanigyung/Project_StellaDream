using System.Collections.Generic;
using UnityEngine;

public class ComboSkillInstance : SkillInstance
{
    private List<SkillInstance> comboSteps;
    private int currentIndex = 0;
    private float comboResetTime = 1.0f;
    private float lastUsedTime = -999f;

    public ComboSkillInstance(ComboSkillData comboData) : base(comboData)
    {
        comboSteps = new();
        comboResetTime = comboData.comboResetTime;

        foreach (var data in comboData.comboSteps)
        {
            if (data != null)
                comboSteps.Add(data.CreateInstance());
        }
    }

    public override void Execute(GameObject attacker, Vector2 direction)
    {
        float now = Time.time;

        if (comboSteps == null || comboSteps.Count == 0)
            return;

        // 콤보 타이밍 초과 시 초기화
        if (now - lastUsedTime > comboResetTime)
            currentIndex = 0;

        SkillInstance currentStep = comboSteps[currentIndex];
        lastUsedTime = now;
        currentIndex = (currentIndex + 1) % comboSteps.Count;

        // 현재 단계의 정보를 ComboSkillInstance에 복사
        CopyFieldsFrom(currentStep);

        // 실제 스킬 실행
        currentStep.Execute(attacker, direction);
    }

    public override void OnHit(GameObject attacker, GameObject target)
    {
        // OnHit은 단계 스킬이 직접 처리하므로 여기선 비워둠
    }

    public override void ApplyUpgrade(WeaponUpgradeInfo upgrade)
    {
        foreach (var step in comboSteps)
        {
            step.ApplyUpgrade(upgrade);
        }
    }

    /// <summary>
    /// 현재 콤보 단계의 SkillInstance를 반환 (외부용)
    /// </summary>
    public SkillInstance GetCurrentStep()
    {
        int index = (currentIndex == 0) ? comboSteps.Count - 1 : currentIndex - 1;
        return comboSteps[Mathf.Clamp(index, 0, comboSteps.Count - 1)];
    }

    /// <summary>
    /// 현재 콤보 단계 스킬의 주요 데이터를 ComboSkillInstance에 복사하여
    /// 외부에서 이 스킬처럼 보이도록 만든다.
    /// </summary>
    private void CopyFieldsFrom(SkillInstance step)
    {
        this.baseData = step.baseData;
        this.cooldown = step.cooldown;
        this.spawnOffset = step.spawnOffset;
        this.statusEffects = step.statusEffects;
        this.effectDuration = step.effectDuration;

        this.spawnedEffect = step.spawnedEffect;
        this.spawnedHitbox = step.spawnedHitbox;
        this.spawnedProjectile = step.spawnedProjectile;
    }
}

using UnityEngine;

public class ComboWeaponControl : WeaponControlBase
{
    private readonly ComboWeaponControlData comboControlData;
    private int comboStepIndex;
    private float lastComboSuccessTime = float.NegativeInfinity;
    private SkillInstance lastActivatedSkillInstance;

    private float ComboTimeLimit
    {
        get
        {
            return Mathf.Max(0f, comboControlData.comboTimeLimit);
        }
    }

    public ComboWeaponControl(WeaponInstance weaponInstance, SkillExecutor skillExecutor, ComboWeaponControlData comboControlData)
        : base(weaponInstance, skillExecutor)
    {
        this.comboControlData = comboControlData;
    }

    public override bool HandleMainInput(WeaponSkillInputPhase inputPhase, Vector2 direction)
    {
        if (inputPhase == WeaponSkillInputPhase.Released)
            return CancelCastingSkill(lastActivatedSkillInstance);

        if (inputPhase != WeaponSkillInputPhase.Pressed)
            return false;

        if (weaponInstance == null)
            return false;

        if (ShouldResetCombo())
            ResetCombo();

        SkillInstance currentComboSkill = weaponInstance.GetMainComboSkillAt(comboStepIndex);
        if (currentComboSkill == null)
        {
            ResetCombo();
            currentComboSkill = weaponInstance.GetMainComboSkillAt(comboStepIndex);
        }

        if (currentComboSkill == null)
            return false;

        if (!currentComboSkill.IsInstantSkill)
        {
            Debug.LogWarning($"[ComboWeaponControl] Combo weapon '{weaponInstance.weaponData?.name}' contains a non-instant skill '{currentComboSkill.data?.name}'.");
            return false;
        }

        bool success = RequestSkillUse(currentComboSkill, direction);
        if (!success)
            return false;

        lastActivatedSkillInstance = currentComboSkill;
        lastComboSuccessTime = Time.time;
        AdvanceComboStep();
        return true;
    }

    protected override SkillInstance GetMainSkillInstance()
    {
        return weaponInstance?.GetMainComboSkillAt(comboStepIndex);
    }

    private bool ShouldResetCombo()
    {
        if (comboStepIndex == 0)
            return false;

        return Time.time - lastComboSuccessTime > ComboTimeLimit;
    }

    private void AdvanceComboStep()
    {
        int comboSkillCount = weaponInstance != null ? weaponInstance.GetMainComboSkillCount() : 0;
        if (comboSkillCount <= 1)
        {
            ResetCombo();
            return;
        }

        comboStepIndex++;
        if (comboStepIndex >= comboSkillCount)
            ResetCombo();
    }

    private void ResetCombo()
    {
        comboStepIndex = 0;
        lastComboSuccessTime = float.NegativeInfinity;
    }
}

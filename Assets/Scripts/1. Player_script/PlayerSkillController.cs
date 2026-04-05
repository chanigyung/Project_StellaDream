using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerSkillController : MonoBehaviour
{
    public PlayerController playerController;
    public PlayerWeaponManager weaponManager;

    void Update()
    {
        PlayerContext context = playerController != null ? playerController.Context : null;
        if (context == null)
            return;

        if (InputBlocker.IsBlocked || !context.canAct)
            return;

        if (context.leftMouseDown)
            weaponManager.HandleMainInput(WeaponSkillInputPhase.Pressed, context.aimDirection);

        if (context.leftMouseHeld)
            weaponManager.HandleMainInput(WeaponSkillInputPhase.Held, context.aimDirection);

        if (context.leftMouseUp)
            weaponManager.HandleMainInput(WeaponSkillInputPhase.Released, context.aimDirection);

        if (context.rightMouseDown)
            weaponManager.HandleSubInput(WeaponSkillInputPhase.Pressed, context.aimDirection);

        if (context.rightMouseHeld)
            weaponManager.HandleSubInput(WeaponSkillInputPhase.Held, context.aimDirection);

        if (context.rightMouseUp)
            weaponManager.HandleSubInput(WeaponSkillInputPhase.Released, context.aimDirection);
    }
}
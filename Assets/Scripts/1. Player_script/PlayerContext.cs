using UnityEngine;

public class PlayerContext : UnitContext
{
    // === 플레이어 전용 참조 ===
    public PlayerController controller;
    public PlayerMovement movement;
    public PlayerAnimator animator;
    public PlayerArmControl armControl;
    public PlayerSkillController skillController;
    public PlayerInteractor interactor;

    public PlayerInstance playerInstance => instance as PlayerInstance; // [추가] 강타입 접근용

    // === 입력 상태 ===
    public Vector2 moveInput;
    public bool jumpPressed;
    public int mouseButton = -1;
    public bool interactPressed;
    public bool leftMouseDown;
    public bool leftMouseHeld;
    public bool leftMouseUp;
    public bool rightMouseDown;
    public bool rightMouseHeld;
    public bool rightMouseUp;

    // === 조준 관련 ===
    public Vector3 mouseWorldPosition;
    public Vector2 aimDirection = Vector2.right;

    // === 기존 PlayerMovement 상태를 context로 이동 ===
    public bool isJumping = false;
    public bool jumpedBefore = false;

    public override void UpdateContext()
    {
        base.UpdateContext();

        canMove = !isKnockbacked && !isMoveSkillActive;
        canAct = !isKnockbacked && !isMoveSkillActive;
        canAttack = !isCastingSkill && !isMoveSkillActive;
    }

    public bool IsMouseDown(int button)
    {
        return button == 0 ? leftMouseDown : rightMouseDown;
    }

    public bool IsMouseHeld(int button)
    {
        return button == 0 ? leftMouseHeld : rightMouseHeld;
    }

    public bool IsMouseUp(int button)
    {
        return button == 0 ? leftMouseUp : rightMouseUp;
    }
}
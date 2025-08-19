using UnityEngine;

public class PlayerArmControl : MonoBehaviour
{
    public Transform body;
    public bool isFacingLeft { get; private set; }
    public bool isTwoHanded;

    public Transform leftArm;
    public Transform rightArm;
    public Animator leftArmAnimator;

    private Vector3 leftArmDefaultPos;
    private Vector3 rightArmDefaultPos;

    public PlayerWeaponManager weaponManager;

    public float rightOffset = 0f;
    public float leftOffset = 0f;

    void Start()
    {
        leftArmDefaultPos = leftArm.localPosition;
        rightArmDefaultPos = rightArm.localPosition;
    }

    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isFacingLeft = mouseWorldPos.x < transform.position.x;
        body.localScale = new Vector3(isFacingLeft ? 1 : -1, 1, 1);

        if (weaponManager.mainWeaponInstance == null || weaponManager.mainWeaponInstance.data == null)
            isTwoHanded = false;
        else
        {
            isTwoHanded = weaponManager.mainWeaponInstance.data.weaponType == WeaponType.TwoHanded;
        }

        float rightAngle = GetArmAngle(rightArm.position, mouseWorldPos, isFacingLeft, rightOffset);
        float leftAngle = GetArmAngle(rightArm.position, mouseWorldPos, isFacingLeft, leftOffset);

        leftArm.rotation = Quaternion.Euler(0, 0, leftAngle);

        if (isTwoHanded)
        {
            rightArm.rotation = Quaternion.Euler(0, 0, rightAngle);
        }
        else if (weaponManager.subWeaponInstance == null || weaponManager.subWeaponInstance.data == null)
        {
            Quaternion baseRotation = Quaternion.Euler(0, 0, isFacingLeft ? 80f : -80f);
            rightArm.rotation = Quaternion.Lerp(baseRotation, leftArm.rotation, 0.3f);
        }
        else
        {
            Quaternion rightRot = Quaternion.Slerp(Quaternion.identity, leftArm.rotation, 0.6f);
            rightArm.rotation = rightRot;
        }
    }

    // 좌우 대칭을 고려한 팔 각도 계산 함수
    float GetArmAngle(Vector3 from, Vector3 to, bool isFacingLeft, float angleOffset = 0f)
    {
        Vector3 dir = to - from;

        // 좌우 대칭 처리: x 방향 반전
        if (isFacingLeft)
            dir.x = -dir.x;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 방향에 따라 회전 반전 (일관된 대칭 회전을 위해)
        return isFacingLeft ? -angle + angleOffset : angle + angleOffset;
    }
}

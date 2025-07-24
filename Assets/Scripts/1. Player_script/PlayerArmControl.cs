using UnityEngine;

public class PlayerArmControl : MonoBehaviour
{
    public Transform body;
    public bool isFacingLeft { get; private set; }

    public Transform leftArm; //왼쪽팔 오브젝트 할당해줄 변수
    public Transform rightArm; //오른쪽팔 오브젝트 할당해줄 변수
    public Animator leftArmAnimator; //팔 애니메이터 호출

    private Vector3 leftArmDefaultPos; //왼쪽팔 상대좌표 기본값
    private Vector3 rightArmDefaultPos; //오른쪽팔 상대좌표 기본값

    public PlayerWeaponManager weaponManager; //장착 무기 참조

    void Start()
    {
        leftArmDefaultPos = leftArm.localPosition;
        rightArmDefaultPos = rightArm.localPosition;
    }

    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (mouseWorldPos.x < transform.position.x)
        {
            body.localScale = new Vector3(1, 1, 1);
            isFacingLeft = true;
        }
        else
        {
            body.localScale = new Vector3(-1, 1, 1);
            isFacingLeft = false;
        }

        bool isTwoHanded;

        if(weaponManager.mainWeaponInstance == null)
        {
            isTwoHanded = false;
        }            
        else
        {
            isTwoHanded = weaponManager.mainWeaponInstance.data != null &&
                            weaponManager.mainWeaponInstance.data.weaponType == WeaponType.TwoHanded; //무기 참조해서 한손/양손무기 여부 파악
        }

        // leftArmAnimator.SetBool("isTwoHanded", isTwoHanded); //양손무기일 경우 왼손 애니메이션 변경

        float rightAngle = GetMouseAngle(rightArm.position, mouseWorldPos);
        float leftAngle = GetMouseAngle(rightArm.position, mouseWorldPos)+180;

        if (isFacingLeft)
        {
            rightAngle += 180f;
            leftAngle += 180f;
        }

        leftArm.rotation = Quaternion.Euler(0, 0, leftAngle); //왼팔은 항상 회전                    

        //무기 참조해서 한손무기일땐 오른손만 / 양손무기일땐 양손 다 돌리기
        if (isTwoHanded)
        {
            rightArm.rotation = Quaternion.Euler(0, 0, rightAngle);
        }
        else
        {
            rightArm.rotation = Quaternion.Euler(0, 0, rightAngle);
            // Quaternion rightRot = Quaternion.Slerp(Quaternion.identity, leftArm.rotation, 0.5f);
            // rightArm.rotation = rightRot;
        }
    }

    float GetMouseAngle(Vector3 fromPosition, Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - fromPosition;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }
}
using UnityEngine;

public class PlayerArmControl : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public bool isFacingLeft { get; private set; }

    public Transform leftArm; //왼쪽팔 오브젝트 할당해줄 변수
    public Transform rightArm; //오른쪽팔 오브젝트 할당해줄 변수
    public Animator leftArmAnimator; //팔 애니메이터 호출

    private Vector3 leftArmDefaultPos; //왼쪽팔 상대좌표 기본값
    private Vector3 rightArmDefaultPos; //오른쪽팔 상대좌표 기본값

    public PlayerWeaponManager weaponManager; //장착 무기 참조

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(); //몸통 스프라이트 가져오기용

        leftArmDefaultPos = leftArm.localPosition;
        rightArmDefaultPos = rightArm.localPosition;
    }

    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (mouseWorldPos.x < transform.position.x)
        {
            spriteRenderer.flipX = true;
            isFacingLeft = true;

            leftArm.localPosition = new Vector3(-leftArmDefaultPos.x, leftArmDefaultPos.y, leftArmDefaultPos.z); //왼쪽 볼때 기존 상대좌표값에서 x좌표만 -곱해주기
            rightArm.localPosition = new Vector3(-rightArmDefaultPos.x, rightArmDefaultPos.y, rightArmDefaultPos.z);

            // 왼쪽 바라볼 때 팔도 반전
            leftArm.localScale = new Vector3(-1, 1, 1);
            rightArm.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            spriteRenderer.flipX = false;
            isFacingLeft = false;

            leftArm.localPosition = leftArmDefaultPos; //오른쪽 볼땐 원래 상대좌표값으로 돌아오기
            rightArm.localPosition = rightArmDefaultPos;

            // 오른쪽 바라볼 때 팔 기본 방향
            leftArm.localScale = new Vector3(1, 1, 1);
            rightArm.localScale = new Vector3(1, 1, 1);
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

        leftArmAnimator.SetBool("isTwoHanded", isTwoHanded); //양손무기일 경우 왼손 애니메이션 변경

        float rightAngle = GetMouseAngle(rightArm.position, mouseWorldPos) + 90f;
        rightArm.rotation = Quaternion.Euler(0, 0, rightAngle); //오른팔은 항상 회전                    

        //무기 참조해서 한손무기일땐 오른손만 / 양손무기일땐 양손 다 돌리기
        if (isTwoHanded)
        {
            leftArm.rotation = Quaternion.Euler(0, 0, rightAngle);
        }
        else
        {
            Quaternion leftRot = Quaternion.Slerp(Quaternion.identity, rightArm.rotation, 0.5f);
            leftArm.rotation = leftRot;
        }
    }

    float GetMouseAngle(Vector3 fromPosition, Vector3 targetPosition) //팔 회전시키는 함수
    {
        Vector3 dir = targetPosition - fromPosition;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        // 팔 이미지가 아래 방향이므로 보정
    }
}
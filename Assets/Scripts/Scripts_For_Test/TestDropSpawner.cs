using UnityEngine;

// 테스트용, 첫 무기 획득방법 생긴 이후 삭제할 코드.
public class TestDropSpawner : MonoBehaviour
{
    public GameObject weaponDropPrefab; // 드랍 오브젝트 프리팹
    public WeaponInstance testWeapon;        // 드랍할 무기 데이터
    public Transform player;             // 플레이어 위치 참조

    public WeaponUpgradeInfo testUpgradeInfo; // 필요시

    void Start()
    {
        // 플레이어 기준 우측 2 유닛 앞에 생성
        Vector3 spawnPos = player.position + new Vector3(2f, 0f, 0f);
        GameObject drop = Instantiate(weaponDropPrefab, spawnPos, Quaternion.identity);

        WeaponInstance weapon = new WeaponInstance(testWeapon.data, false);

        weapon.ApplyUpgrade(testUpgradeInfo);

        // weaponData 세팅
        ItemDrop dropScript = drop.GetComponent<ItemDrop>();
        dropScript.weaponInstance = weapon;
        dropScript.Initialize(dropScript.weaponInstance);
        // dropScript.iconRenderer = drop.GetComponent<SpriteRenderer>();
    }
}
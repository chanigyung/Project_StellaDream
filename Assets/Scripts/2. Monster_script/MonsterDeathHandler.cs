using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDeathHandler : MonoBehaviour
{
    public MonsterDropHandler dropHandler;

    private bool isDead = false;

    public GameObject dieEffectPrefab; //죽는 이펙트 재생용 프리팹
    public RuntimeAnimatorController deathAnimatorController; //몬스터마다 별개로 적용될 죽는 애니메이션

    public void Die()
    {
        if (isDead) return; // 이미 죽었으면 무시
        isDead = true;

        dropHandler?.DropItem();

        if (dieEffectPrefab != null && deathAnimatorController != null)
        {
            GameObject effect = Instantiate(dieEffectPrefab, transform.position, Quaternion.identity);

             Vector3 scale = effect.transform.localScale;
            scale.x *= Mathf.Sign(transform.localScale.x); // 현재 몬스터의 방향 따라 반전
            effect.transform.localScale = scale;

            Animator animator = effect.GetComponent<Animator>();
            if (animator != null)
            {
                animator.runtimeAnimatorController = deathAnimatorController;
            }
        }

        Destroy(gameObject); // 몬스터 제거
    }

}

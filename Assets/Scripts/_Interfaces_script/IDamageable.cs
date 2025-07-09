using UnityEngine;

public interface IDamageable
{
    //데미지를 받을 수 있는 모든 오브젝트에 적용되는 인터페이스
    void TakeDamage(float damage);
}
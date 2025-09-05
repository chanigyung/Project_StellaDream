using UnityEngine;

public class SkillVFXController : MonoBehaviour
{
    public bool applyRotation = true;
    public Animator effectAnimator;

    public void Initialize(Vector2 direction, float duration, RuntimeAnimatorController effectAnimation = null, bool flipY = true)
    {
        if (applyRotation)
        {
            transform.right = direction;

            if (direction.x < 0 && flipY)
            {
                Vector3 scale = transform.localScale;
                scale.y *= -1;
                transform.localScale = scale;
            }
        }

        if (effectAnimator != null && effectAnimation != null)
        {
            effectAnimator.runtimeAnimatorController = effectAnimation;
        }

        if (duration > 0f)
        {
            Destroy(gameObject, duration);
        }
    }
}

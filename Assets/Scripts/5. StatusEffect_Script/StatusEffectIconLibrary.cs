using UnityEngine;

public class StatusEffectIconLibrary : MonoBehaviour
{
    public static StatusEffectIconLibrary Instance { get; private set; }

    [Header("상태이상 아이콘들")]
    public Sprite stunSprite;
    public Sprite slowSprite;
    public Sprite poisonSprite;
    public Sprite igniteSprite;
    public Sprite rootSprite;
    public Sprite bleedSprite;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
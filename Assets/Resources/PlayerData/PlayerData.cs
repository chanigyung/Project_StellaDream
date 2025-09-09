using UnityEngine;

[CreateAssetMenu(menuName = "Player/PlayerData")]
public class PlayerData : ScriptableObject
{
    public string characterID;
    public string characterName;
    public GameObject characterPrefab;
    public PlayerVisualData visualData;

    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public float jumpPower = 12f;

    public bool knockbackImmune = false;
    public float knockbackResistance = 0f;
}
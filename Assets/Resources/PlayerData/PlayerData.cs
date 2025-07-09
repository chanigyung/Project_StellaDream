using UnityEngine;

[CreateAssetMenu(menuName = "Player/PlayerData")]
public class PlayerData : ScriptableObject
{
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public float jumpPower = 12f;

    public bool knockbackImmune = false;
    public float knockbackResistance = 0f;
}
[System.Serializable]
public class PoisonEffectInfo : StatusEffectInfo
{
    public float damagePerTick;
    public float tickInterval;
    public float slowRate;

    public PoisonEffectInfo()
    {
        type = StatusEffectType.Poison;
    }
}
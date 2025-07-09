[System.Serializable] //출혈
public class BleedEffectInfo : StatusEffectInfo
{
    public float damagePerTick;
    public float tickInterval;

    public BleedEffectInfo()
    {
        type = StatusEffectType.Bleed;
    }
}
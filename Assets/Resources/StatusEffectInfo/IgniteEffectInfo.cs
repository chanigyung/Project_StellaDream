[System.Serializable] //발화
public class IgniteEffectInfo : StatusEffectInfo
{
    public float damagePerTick;
    public float tickInterval;

    public IgniteEffectInfo()
    {
        type = StatusEffectType.Ignite;
    }
}
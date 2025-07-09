[System.Serializable] //슬로우
public class SlowEffectInfo : StatusEffectInfo
{
    public float slowRate;

    public SlowEffectInfo()
    {
        type = StatusEffectType.Slow;
    }
}

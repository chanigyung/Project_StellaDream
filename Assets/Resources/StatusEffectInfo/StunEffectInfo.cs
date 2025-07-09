[System.Serializable]
public class StunEffectInfo : StatusEffectInfo
{
    // 현재는 duration만 사용
    public StunEffectInfo()
    {
        type = StatusEffectType.Stun;
    }
}
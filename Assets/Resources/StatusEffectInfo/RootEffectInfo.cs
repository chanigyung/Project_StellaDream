[System.Serializable]
public class RootEffectInfo : StatusEffectInfo
{
    // 현재는 duration만 사용
    public RootEffectInfo()
    {
        type = StatusEffectType.Root;
    }
}
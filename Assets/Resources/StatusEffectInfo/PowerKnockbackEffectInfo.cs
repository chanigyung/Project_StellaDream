[System.Serializable] //파워넉백
public class PowerKnockbackEffectInfo : StatusEffectInfo
{
    public float power;

    public PowerKnockbackEffectInfo()
    {
        type = StatusEffectType.PowerKnockback;
    }   
}

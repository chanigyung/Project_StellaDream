[System.Serializable]
public class WeaponUpgradeInfo
{
    public int baseUpgradeLevel;
    public int efficiencyUpgradeLevel;
    public int masteryUpgradeLevel;

    public WeaponUpgradeInfo(int baseLevel = 0, int effLevel = 0, int masterLevel = 0)
    {
        baseUpgradeLevel = baseLevel;
        efficiencyUpgradeLevel = effLevel;
        masteryUpgradeLevel = masterLevel;
    }
}

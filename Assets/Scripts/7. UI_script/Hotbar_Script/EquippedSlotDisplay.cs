using UnityEngine;
using UnityEngine.UI;

public class EquippedSlotDisplay : MonoBehaviour
{
    public Image mainSlotImage;
    public Image subSlotImage;

    void Start()
    {
        mainSlotImage.color = new Color(1, 1, 1, 0);
        subSlotImage.color = new Color(1, 1, 1, 0);
    }

    public void UpdateMainSlot(WeaponInstance weapon)
    {
        if (weapon != null && weapon.data?.icon != null)
        {
            mainSlotImage.sprite = weapon.data.icon;
            mainSlotImage.color = new Color(1f,1f,1f,1f);
        }
        else
        {
            mainSlotImage.sprite = null;
            mainSlotImage.color = new Color(1, 1, 1, 0);
        }
    }

    public void UpdateSubSlot(WeaponInstance weapon)
    {
        if (weapon != null && weapon.data?.icon != null)
        {
            subSlotImage.sprite = weapon.data.icon;
            subSlotImage.color = new Color(1f,1f,1f,1f);  
        }
        else
        {
            subSlotImage.sprite = null;
            subSlotImage.color = new Color(1, 1, 1, 0);
        }
    }
}
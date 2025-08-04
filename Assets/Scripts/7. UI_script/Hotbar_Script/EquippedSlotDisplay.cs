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

    public void UpdateMainSlot(WeaponInstance instance)
    {
        if (instance != null && instance.data != null)
        {
            mainSlotImage.sprite = instance.data.icon;
            mainSlotImage.color = Color.white;
        }
        else
        {
            mainSlotImage.sprite = null;
            mainSlotImage.color = new Color(1, 1, 1, 0);
        }
    }

    public void UpdateSubSlot(WeaponInstance instance)
    {
        if (instance != null && instance.data != null)
        {
            subSlotImage.sprite = instance.data.icon;
            subSlotImage.color = Color.white;
        }
        else
        {
            subSlotImage.sprite = null;
            subSlotImage.color = new Color(1, 1, 1, 0);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusEffectIcon : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image timeFillImage;
    [SerializeField] private TextMeshProUGUI stackText;

    public StatusEffectType EffectType { get; private set; }

    private StatusEffect linkedEffect;

    public void Initialize(StatusEffect effect)
    {
        linkedEffect = effect;
        EffectType = effect.effectType;

        iconImage.sprite = effect.icon;
        UpdateStackText(effect.GetStackCount());
    }

    public void Refresh(StatusEffect effect)
    {
        linkedEffect = effect;
        UpdateStackText(effect.GetStackCount());
        Debug.Log("현재 스택 수 : " + effect.GetStackCount());
    }

    public void UpdateProgress(float elapsed, float duration)
    {
        float ratio = Mathf.Clamp01(elapsed / duration); // 또는 1 - elapsed / duration
        timeFillImage.fillAmount = ratio;
    }

    private void UpdateStackText(int stack)
    {
        if (stack > 1)
        {
            stackText.text = stack.ToString();
            stackText.gameObject.SetActive(true);
        }
        else
        {
            stackText.text = "";
            stackText.gameObject.SetActive(false); // 안 보이게
        }
    }

    public bool Matches(StatusEffect effect) => linkedEffect == effect;
}

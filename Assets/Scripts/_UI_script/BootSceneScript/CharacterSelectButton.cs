using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectButton : MonoBehaviour
{
    public Text nameText;
    private PlayerData data;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Init(PlayerData playerData)
    {
        data = playerData;
        // nameText.text = playerData.characterID; // 또는 UI용 이름

        if (button == null)
            button = GetComponent<Button>();
        // if (nameText != null)
        //     nameText.text = playerData.characterID;

        button.onClick.RemoveAllListeners(); // ✅ 중복 방지
        button.onClick.AddListener(OnClick_Select);
    }

    public void OnClick_Select()
    {
        Debug.Log("캐릭터 선택 버튼 눌림" + data.characterID);
        CharacterSelectUI.Instance.OnCharacterSelected(data);
    }
}

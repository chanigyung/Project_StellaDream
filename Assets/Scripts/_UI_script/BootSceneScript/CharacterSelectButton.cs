using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectButton : MonoBehaviour
{
    public Text nameText;
    public PlayerData playerData;   // Inspector에서 직접 연결

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick_Select);
        }

        if (nameText != null && playerData != null)
        {
            nameText.text = playerData.characterName; // 또는 playerData.characterName;
        }
    }

    public void OnClick_Select()
    {
        if (playerData == null) return;
        Debug.Log("캐릭터 선택 버튼 눌림: " + playerData.characterName);
        CharacterSelectUI.Instance.OnCharacterSelected(playerData);
    }
}

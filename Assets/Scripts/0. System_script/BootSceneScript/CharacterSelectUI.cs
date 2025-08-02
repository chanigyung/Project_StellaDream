using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    public static CharacterSelectUI Instance { get; private set; }
    public Button startButton;

    private PlayerData selectedData;
    public Text selectedCharacterText;

     private void Awake()
    {
        Instance = this;
        startButton.interactable = false;
        startButton.onClick.AddListener(OnClick_StartGame);
    }

    public void OnCharacterSelected(PlayerData data)
    {
        selectedData = data;
        startButton.interactable = true;
        if (selectedCharacterText != null)
        {
            selectedCharacterText.text = $"선택한 캐릭터 : {data.characterName}";
        }
    }

    private void OnClick_StartGame()
    {
        if (selectedData == null) return;

        GameController.Instance.SetSelectedCharacter(selectedData);
        GameController.Instance.RequestSceneLoad("SampleScene");
    }
}

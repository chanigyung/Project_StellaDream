using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelectUI : MonoBehaviour
{
    public static CharacterSelectUI Instance { get; private set; }

    public Transform buttonParent;
    public GameObject buttonPrefab;

    public PlayerData[] availableCharacters;

    public Button startButton;
    private PlayerData selectedData;

    private void Awake()
    {
        Instance = this;
        GenerateCharacterButtons();
        startButton.interactable = false;
        startButton.onClick.AddListener(OnClick_StartGame);
    }

    void GenerateCharacterButtons()
    {
        foreach (var data in availableCharacters)
        {
            GameObject obj = Instantiate(buttonPrefab, buttonParent);
            CharacterSelectButton button = obj.GetComponent<CharacterSelectButton>();
            button.Init(data);
        }
    }

    public void OnCharacterSelected(PlayerData data)
    {
        selectedData = data;
        startButton.interactable = true;
        Debug.Log($"선택된 캐릭터: {data.characterID}");
    }

    private void OnClick_StartGame()
    {
        if (selectedData == null) return;

        GameController.Instance.SetSelectedCharacter(selectedData);
        GameController.Instance.ChangeState(GameState.Loading);
        SceneManager.LoadScene("SampleScene");
    }
}

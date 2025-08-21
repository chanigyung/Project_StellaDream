using UnityEngine;

public class PortalNPC : MonoBehaviour, IInteractable
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private GameObject highlightObject;

    private bool hasInteracted = false;

    public void SetHighlight(bool isOn)
    {
        if (highlightObject != null)
            highlightObject.SetActive(isOn);
    }

    public void Interact()
    {
        if (hasInteracted) return; //중복 상호작용 금지
        hasInteracted = true; //호출완료

        if (!string.IsNullOrEmpty(targetSceneName))
        {
            GameController.Instance.RequestSceneLoad(targetSceneName);
        }
    }
}

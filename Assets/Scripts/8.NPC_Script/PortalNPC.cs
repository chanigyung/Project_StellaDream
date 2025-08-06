using UnityEngine;

public class PortalNPC : MonoBehaviour, IInteractable
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private GameObject highlightObject;

    public void SetHighlight(bool isOn)
    {
        if (highlightObject != null)
            highlightObject.SetActive(isOn);
    }

    public void Interact()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            GameController.Instance.RequestSceneLoad(targetSceneName);
        }
    }
}

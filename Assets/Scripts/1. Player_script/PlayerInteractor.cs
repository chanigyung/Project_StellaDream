using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public PlayerController playerController;
    public float interactRange = 0.5f;
    public LayerMask interactLayer;

    private IInteractable currentTarget;

    void Update()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange, interactLayer);
        if (hits.Length == 0)
        {
            ClearHighlight();
            return;
        }

        Collider2D nearest = null;
        float minDist = float.MaxValue;

        foreach (var col in hits)
        {
            float dist = Vector2.Distance(transform.position, col.transform.position);
            if (dist < minDist)
            {
                nearest = col;
                minDist = dist;
            }
        }

        var interactable = nearest.GetComponentInParent<IInteractable>();
        if (interactable == null)
        {
            ClearHighlight();
            return;
        }

        if (interactable != currentTarget)
        {
            currentTarget?.SetHighlight(false);
            interactable.SetHighlight(true);
            currentTarget = interactable;
        }

        if (playerController.interactPressed)
        {
            interactable.Interact();
            currentTarget = null;
        }
    }

    void ClearHighlight()
    {
        currentTarget?.SetHighlight(false);
        currentTarget = null;
    }
}

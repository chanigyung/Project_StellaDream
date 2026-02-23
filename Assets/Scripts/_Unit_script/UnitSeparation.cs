using System.Collections.Generic;
using UnityEngine;

public class UnitSeparation : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private LayerMask unitLayerMask;

    [Header("Separation (Fixed Push)")]
    [SerializeField] private float overlapRadius = 0.75f;
    [SerializeField] private float minSeparationX = 0.7f;
    [SerializeField] private float pushSpeed = 3.0f;
    [SerializeField] private float maxSepDeltaXPerFrame = 1.5f;

    [Header("Optional")]
    [SerializeField] private Rigidbody2D rigid;

    // 내부 재사용 버퍼
    private readonly Collider2D[] _hitBuffer = new Collider2D[32];

    private int _selfId;

    private void Awake()
    {
        _selfId = GetInstanceID();

        if (rigid == null)
        {
            rigid = GetComponent<Rigidbody2D>();
            if (rigid == null) rigid = GetComponentInParent<Rigidbody2D>();
        }
    }

    private void FixedUpdate()
    {
        if (rigid == null) return;

        Vector2 selfPos = rigid.position;

        int hitCount = Physics2D.OverlapCircleNonAlloc(selfPos, overlapRadius, _hitBuffer, unitLayerMask);
        if (hitCount <= 0) return;

        Dictionary<Rigidbody2D, float> deltaXMap = null;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D col = _hitBuffer[i];
            if (col == null) continue;

            Rigidbody2D otherRb = col.attachedRigidbody;
            if (otherRb == null) continue;
            if (otherRb == rigid) continue; // 자기 자신 제외

            int otherId = otherRb.GetInstanceID();
            if (_selfId > otherId) continue;

            float dx = selfPos.x - otherRb.position.x;
            float absDx = Mathf.Abs(dx);

            if (absDx >= minSeparationX) continue;

            float dir = (absDx > 0.0001f) ? Mathf.Sign(dx) : ((_selfId & 1) == 0 ? 1f : -1f);

            float halfPush = pushSpeed * 0.5f;

            if (deltaXMap == null) deltaXMap = new Dictionary<Rigidbody2D, float>(8);

            AddDelta(deltaXMap, rigid, dir * halfPush);
            AddDelta(deltaXMap, otherRb, -dir * halfPush);
        }

        if (deltaXMap == null || deltaXMap.Count == 0) return;

        foreach (var kvp in deltaXMap)
        {
            Rigidbody2D rb = kvp.Key;
            if (rb == null) continue;

            float deltaX = Mathf.Clamp(kvp.Value, -maxSepDeltaXPerFrame, maxSepDeltaXPerFrame);
            if (Mathf.Abs(deltaX) <= 0.0001f) continue;

            Vector2 v = rb.velocity;
            v.x += deltaX;
            rb.velocity = v;
        }
    }

    private static void AddDelta(Dictionary<Rigidbody2D, float> map, Rigidbody2D rb, float delta)
    {
        if (rb == null) return;

        if (map.TryGetValue(rb, out float cur))
            map[rb] = cur + delta;
        else
            map.Add(rb, delta);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, overlapRadius);
    }
}
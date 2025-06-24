using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class WireColliderUpdater : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    private EdgeCollider2D edgeCollider;

    void Awake()
    {
        edgeCollider = GetComponent<EdgeCollider2D>();
    }

    void LateUpdate()
    {
        UpdateCollider();
    }

    void UpdateCollider()
    {
        int pointCount = lineRenderer.positionCount;
        Vector2[] points = new Vector2[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            Vector3 worldPos = lineRenderer.GetPosition(i);
            points[i] = transform.InverseTransformPoint(worldPos);
        }

        edgeCollider.points = points;
    }
}

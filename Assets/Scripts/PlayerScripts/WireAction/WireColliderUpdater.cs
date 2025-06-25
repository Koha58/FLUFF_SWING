using UnityEngine;

/// <summary>
/// LineRendererの形状に合わせてEdgeCollider2Dの形状を自動更新するスクリプト。
/// Wire（ロープなど）の当たり判定をLineRendererと同期させたいときに使用する。
/// </summary>
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class WireColliderUpdater : MonoBehaviour
{
    /// <summary>
    /// 当たり判定の元になるLineRenderer
    /// </summary>
    [SerializeField] private LineRenderer lineRenderer;

    /// <summary>
    /// 実際に形状を更新するEdgeCollider2D
    /// </summary>
    private EdgeCollider2D edgeCollider;

    /// <summary>
    /// 初期化処理。EdgeCollider2Dを取得する。
    /// </summary>
    void Awake()
    {
        edgeCollider = GetComponent<EdgeCollider2D>();
        edgeCollider.enabled = false; // 念のため無効化
    }

    /// <summary>
    /// 毎フレームのLateUpdateでコライダーを更新する。
    /// </summary>
    void LateUpdate()
    {
        UpdateCollider();
    }

    /// <summary>
    /// LineRendererの頂点をEdgeCollider2Dのポイントに変換して設定する。
    /// LineRendererが無効ならColliderも無効化する。
    /// </summary>
    void UpdateCollider()
    {
        if (lineRenderer == null || !lineRenderer.enabled)
        {
            // LineRendererが非表示ならコライダーも無効化
            edgeCollider.enabled = false;
            return;
        }

        int pointCount = lineRenderer.positionCount;

        if (pointCount < 2)
        {
            // 頂点が足りない場合も無効化
            edgeCollider.points = new Vector2[0];
            edgeCollider.enabled = false;
            return;
        }

        // 頂点を設定
        Vector2[] points = new Vector2[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 worldPos = lineRenderer.GetPosition(i);
            points[i] = transform.InverseTransformPoint(worldPos);
        }

        edgeCollider.points = points;
        edgeCollider.enabled = true;
    }
}

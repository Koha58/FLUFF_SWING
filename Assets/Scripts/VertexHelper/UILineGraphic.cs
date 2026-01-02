using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI上に「曲がる線」を描画するためのカスタムGraphic。
/// 
/// 主な特徴：
/// ・MaskableGraphic を継承し、Canvas上で軽量に描画
/// ・UI(RectTransform) / World(Transform) のどちらも終点に指定可能
/// ・ベジェ曲線を分割して、太さのあるラインとして描画
/// ・重力方向に少し垂れるような“紐・ワイヤー表現”が可能
/// 
/// 想定用途：
/// ・ワイヤー、コード、糸、リンク表現
/// ・UIと3Dオブジェクトをつなぐ視覚的ガイド
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class UILineGraphic : MaskableGraphic
{
    // =========================================================
    // Line Settings
    // =========================================================

    [Header("Line")]
    [Tooltip("線の太さ（px感覚）")]
    [SerializeField] private float thickness = 6f;

    [Tooltip("線を何分割で描画するか（多いほど滑らか・重くなる）")]
    [Range(6, 128)]
    [SerializeField] private int segmentCount = 30;

    // =========================================================
    // Endpoints
    // =========================================================

    [Header("Endpoints (UI优先 / WorldでもOK)")]
    [Tooltip("始点（UI座標）。指定されていればWorldより優先")]
    [SerializeField] private RectTransform handUI;

    [Tooltip("終点（UI座標）。指定されていればWorldより優先")]
    [SerializeField] private RectTransform needleUI;

    [Tooltip("始点（ワールド座標）。UIが無い場合に使用")]
    [SerializeField] private Transform handWorld;

    [Tooltip("終点（ワールド座標）。UIが無い場合に使用")]
    [SerializeField] private Transform needleWorld;

    // =========================================================
    // Bend / Curve Settings
    // =========================================================

    [Header("Bend")]
    [Tooltip("曲がり具合（UIなのでpx感覚）")]
    [SerializeField] private float bendAmount = 80f;

    [Tooltip("どれだけ下方向（重力方向）に寄せるか")]
    [Range(0f, 1f)]
    [SerializeField] private float gravityInfluence = 0.7f;

    [Tooltip("終点が始点より上にある場合に反転させるか")]
    [SerializeField] private bool flipWhenNeedleAbove = false;

    // =========================================================
    // Internal State
    // =========================================================

    /// <summary>
    /// ベジェ曲線を分割した結果の点列。
    /// Mesh生成時に参照される。
    /// </summary>
    private readonly List<Vector2> points = new();

    /// <summary>
    /// 外部から太さを変更するためのプロパティ。
    /// 値変更時にメッシュ再生成を要求する。
    /// </summary>
    public float Thickness
    {
        get => thickness;
        set
        {
            thickness = Mathf.Max(0.01f, value);
            SetVerticesDirty();
        }
    }

    // =========================================================
    // Unity Lifecycle
    // =========================================================

    private void LateUpdate()
    {
        // 有効な始点・終点が無ければ描画しない
        if (!HasValidEndpoints()) return;

        // Canvasローカル座標へ変換
        Vector2 start = GetCanvasLocalPoint(handUI, handWorld);
        Vector2 end = GetCanvasLocalPoint(needleUI, needleWorld);

        // ベジェ曲線を再計算
        RebuildBezierPoints(start, end);

        // メッシュ再生成要求
        SetVerticesDirty();
    }

    // =========================================================
    // Endpoint Utilities
    // =========================================================

    /// <summary>
    /// UIまたはWorldのいずれかで有効な参照があるか判定
    /// </summary>
    private bool HasValidEndpoints()
    {
        bool uiOk = handUI && needleUI;
        bool worldOk = handWorld && needleWorld;
        return uiOk || worldOk;
    }

    /// <summary>
    /// UI優先でCanvasローカル座標を取得する
    /// </summary>
    private Vector2 GetCanvasLocalPoint(RectTransform ui, Transform world)
    {
        if (ui) return WorldToCanvasLocal(ui.position);
        return WorldToCanvasLocal(world.position);
    }

    /// <summary>
    /// ワールド座標を、このGraphicのRectTransform基準のローカル座標に変換
    /// </summary>
    private Vector2 WorldToCanvasLocal(Vector3 worldPos)
    {
        // CanvasのRenderModeに応じて使用カメラを決定
        var canvas = GetComponentInParent<Canvas>();
        Camera cam = null;

        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;

        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, worldPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, screen, cam, out var local);

        return local;
    }

    // =========================================================
    // Bezier Calculation
    // =========================================================

    /// <summary>
    /// 始点・終点から二次ベジェ曲線を生成し、分割点を points に格納する
    /// </summary>
    private void RebuildBezierPoints(Vector2 start, Vector2 end)
    {
        // 中点（制御点のベース）
        Vector2 mid = (start + end) * 0.5f;

        Vector2 dir = end - start;
        float len = dir.magnitude;

        // 距離がほぼ無い場合は直線扱い
        if (len < 0.001f)
        {
            points.Clear();
            points.Add(start);
            points.Add(end);
            return;
        }

        dir /= len;

        // 曲げ方向（法線）を計算
        Vector2 gravityDir = Vector2.down;
        Vector2 normal = new Vector2(-dir.y, dir.x).normalized;

        // 重力方向へ補正
        normal = Vector2.Lerp(normal, gravityDir, gravityInfluence).normalized;

        // 必要なら上下関係で反転
        if (flipWhenNeedleAbove)
        {
            float verticalSign = Mathf.Sign(end.y - start.y);
            normal *= verticalSign;
        }

        // 制御点を曲げ方向にオフセット
        mid += normal * bendAmount;

        int n = Mathf.Max(2, segmentCount);
        points.Clear();
        points.Capacity = n;

        // 二次ベジェ曲線を等間隔でサンプリング
        for (int i = 0; i < n; i++)
        {
            float t = i / (n - 1f);
            points.Add(GetQuadraticBezierPoint(start, mid, end, t));
        }
    }

    /// <summary>
    /// 二次ベジェ曲線の点を取得
    /// </summary>
    private static Vector2 GetQuadraticBezierPoint(
        Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        float u = 1f - t;
        return (u * u) * p0 + (2f * u * t) * p1 + (t * t) * p2;
    }

    // =========================================================
    // Mesh Generation
    // =========================================================

    /// <summary>
    /// points を元に、太さを持つラインメッシュを生成する
    /// </summary>
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points.Count < 2 || thickness <= 0f)
            return;

        float half = thickness * 0.5f;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[i + 1];

            Vector2 dir = b - a;
            float len = dir.magnitude;
            if (len < 0.0001f) continue;

            dir /= len;
            Vector2 n = new Vector2(-dir.y, dir.x) * half;

            Vector2 v0 = a - n;
            Vector2 v1 = a + n;
            Vector2 v2 = b + n;
            Vector2 v3 = b - n;

            AddQuad(vh, v0, v1, v2, v3, color);
        }
    }

    /// <summary>
    /// 四角形（2三角形）を VertexHelper に追加する
    /// </summary>
    private static void AddQuad(
        VertexHelper vh,
        Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3,
        Color32 col)
    {
        int start = vh.currentVertCount;

        UIVertex vert = UIVertex.simpleVert;
        vert.color = col;

        vert.position = v0; vh.AddVert(vert);
        vert.position = v1; vh.AddVert(vert);
        vert.position = v2; vh.AddVert(vert);
        vert.position = v3; vh.AddVert(vert);

        vh.AddTriangle(start + 0, start + 1, start + 2);
        vh.AddTriangle(start + 2, start + 3, start + 0);
    }
}

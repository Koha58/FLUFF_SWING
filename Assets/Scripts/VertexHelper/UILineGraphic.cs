using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UILineGraphic : MaskableGraphic
{
    [Header("Line")]
    [SerializeField] private float thickness = 6f;
    [SerializeField, Range(6, 128)] private int segmentCount = 30;

    [Header("Endpoints (UI优先 / WorldでもOK)")]
    [SerializeField] private RectTransform handUI;
    [SerializeField] private RectTransform needleUI;
    [SerializeField] private Transform handWorld;
    [SerializeField] private Transform needleWorld;

    [Header("Bend")]
    [SerializeField] private float bendAmount = 80f;               // UIなのでpx感覚
    [SerializeField, Range(0f, 1f)] private float gravityInfluence = 0.7f; // 下方向へ寄せる
    [SerializeField] private bool flipWhenNeedleAbove = false;     // 針が上なら反転させたい時

    // 内部点列（Inspectorから触る必要はないので非公開）
    private readonly List<Vector2> points = new();

    public float Thickness
    {
        get => thickness;
        set { thickness = Mathf.Max(0.01f, value); SetVerticesDirty(); }
    }

    private void LateUpdate()
    {
        // 有効な参照が無いなら何もしない
        if (!HasValidEndpoints()) return;

        Vector2 start = GetCanvasLocalPoint(handUI, handWorld);
        Vector2 end = GetCanvasLocalPoint(needleUI, needleWorld);

        RebuildBezierPoints(start, end);
        SetVerticesDirty(); // mesh再生成
    }

    private bool HasValidEndpoints()
    {
        bool uiOk = handUI && needleUI;
        bool worldOk = handWorld && needleWorld;
        return uiOk || worldOk;
    }

    private Vector2 GetCanvasLocalPoint(RectTransform ui, Transform world)
    {
        // UIがあればUI優先
        if (ui) return WorldToCanvasLocal(ui.position);
        return WorldToCanvasLocal(world.position);
    }

    private Vector2 WorldToCanvasLocal(Vector3 worldPos)
    {
        // Canvasモードに応じたカメラを使う
        var canvas = GetComponentInParent<Canvas>();
        Camera cam = null;
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;

        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, screen, cam, out var local);

        return local;
    }

    private void RebuildBezierPoints(Vector2 start, Vector2 end)
    {
        Vector2 mid = (start + end) * 0.5f;

        Vector2 dir = end - start;
        float len = dir.magnitude;
        if (len < 0.001f)
        {
            points.Clear();
            points.Add(start);
            points.Add(end);
            return;
        }

        dir /= len;

        Vector2 gravityDir = Vector2.down;
        Vector2 normal = new Vector2(-dir.y, dir.x).normalized;
        normal = Vector2.Lerp(normal, gravityDir, gravityInfluence).normalized;

        if (flipWhenNeedleAbove)
        {
            float verticalSign = Mathf.Sign(end.y - start.y);
            normal *= verticalSign;
        }

        mid += normal * bendAmount;

        int n = Mathf.Max(2, segmentCount);
        points.Clear();
        points.Capacity = n;

        for (int i = 0; i < n; i++)
        {
            float t = i / (n - 1f);
            points.Add(GetQuadraticBezierPoint(start, mid, end, t));
        }
    }

    private static Vector2 GetQuadraticBezierPoint(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        float u = 1f - t;
        return (u * u) * p0 + (2f * u * t) * p1 + (t * t) * p2;
    }

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

    private static void AddQuad(VertexHelper vh, Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, Color32 col)
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

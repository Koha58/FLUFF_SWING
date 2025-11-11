using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TutorialWireBezier : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Transform playerHand; // 始点
    [SerializeField] private Transform needle;     // 終点
    [SerializeField] private float bendAmount = 0.5f;
    [SerializeField, Range(6, 64)] private int segmentCount = 20;
    [SerializeField] private float gravityInfluence = 0.5f; // 重力方向へのしなり補正

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.enabled = false;
    }

    private void LateUpdate()
    {
        if (!line.enabled) return;

        Vector3 start = playerHand.position;
        Vector3 end = needle.position;

        // ベースの中間点
        Vector3 mid = (start + end) * 0.5f;

        // 手と針の方向
        Vector3 dir = (end - start).normalized;

        // 重力方向補正
        Vector3 gravityDir = Vector3.down;

        // しなり方向を「重力に少し寄せる」
        Vector3 normal = Vector3.Cross(dir, Vector3.forward).normalized;
        normal = Vector3.Lerp(normal, gravityDir, gravityInfluence).normalized;

        // しなりを動的に反転させる（針が手より上にあるときは逆方向）
        float verticalSign = Mathf.Sign(end.y - start.y);
        mid += normal * bendAmount * verticalSign;

        // ベジェ補間でワイヤー更新
        line.positionCount = segmentCount;
        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (segmentCount - 1f);
            line.SetPosition(i, GetQuadraticBezierPoint(start, mid, end, t));
        }
    }

    private Vector3 GetQuadraticBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
    }

    // AnimationEvent用
    public void ShowWireOn() => line.enabled = true;
    public void ShowWireOff() => line.enabled = false;
}

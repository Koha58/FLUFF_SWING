using UnityEngine;
using UnityEditor;

/// <summary>
/// SpawnMarker に対してシーンビュー上でギズモ（Gizmo）を描画するカスタムエディタ。
/// - Marker の種類に応じて色を変更（Enemy=赤, Coin=黄）
/// - Marker の位置に球体を描画
/// - Marker のプレハブ名をラベル表示
/// </summary>
[CustomEditor(typeof(SpawnMarker))]
public class SpawnMarkerGizmoDrawer : Editor
{
    /// <summary>
    /// Sceneビューでギズモを描画するコールバック
    /// </summary>
    void OnSceneGUI()
    {
        // 対象の SpawnMarker を取得
        SpawnMarker marker = (SpawnMarker)target;

        // ---------------- 1. ギズモの色を Marker type に応じて設定 ----------------
        Color gizmoColor = Color.white; // デフォルト白
        switch (marker.type.ToLower()) // 小文字化して比較
        {
            case "enemy":
                gizmoColor = Color.red; // 敵マーカーは赤
                break;
            case "coin":
                gizmoColor = Color.yellow; // コインは黄色
                break;
        }

        // ---------------- 2. ギズモ描画 ----------------
        Handles.color = gizmoColor;

        // 球体ギズモを描画
        // 第1引数: controlID (0で問題なし)
        // 第2引数: 描画位置
        // 第3引数: 回転 (Quaternion.identity = 回転なし)
        // 第4引数: 大きさ (0.5f)
        // 第5引数: イベントタイプ（Repaint で描画のみ）
        Handles.SphereHandleCap(0, marker.transform.position, Quaternion.identity, 0.5f, EventType.Repaint);

        // ---------------- 3. ラベル描画 ----------------
        // マーカー位置の少し上にプレハブ名を表示
        Handles.Label(marker.transform.position + Vector3.up * 0.6f, marker.prefabName);
    }
}

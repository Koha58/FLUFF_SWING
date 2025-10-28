using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpawnMarker))]
public class SpawnMarkerGizmoDrawer : Editor
{
    void OnSceneGUI()
    {
        SpawnMarker marker = (SpawnMarker)target;

        Color gizmoColor = Color.white;
        switch (marker.type.ToLower())
        {
            case "enemy": gizmoColor = Color.red; break;
            case "coin": gizmoColor = Color.yellow; break;
            case "item": gizmoColor = Color.cyan; break;
        }

        Handles.color = gizmoColor;
        Handles.SphereHandleCap(0, marker.transform.position, Quaternion.identity, 0.5f, EventType.Repaint);
        Handles.Label(marker.transform.position + Vector3.up * 0.6f, marker.prefabName);
    }
}

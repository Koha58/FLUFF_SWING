using UnityEngine;

public class CursorChanger : MonoBehaviour
{
    public Texture2D cursorTexture;  // インスペクターで設定するカーソル画像
    public Vector2 hotspot = Vector2.zero;  // カーソルのホットスポット（クリック位置）
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 cursorSize = new Vector2(64, 64);  // 好きなサイズを設定

    void Start()
    {
        Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
    }
}

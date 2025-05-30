using UnityEngine;

public class CursorChanger : MonoBehaviour
{
    public Texture2D cursorTexture;  // �C���X�y�N�^�[�Őݒ肷��J�[�\���摜
    public Vector2 hotspot = Vector2.zero;  // �J�[�\���̃z�b�g�X�|�b�g�i�N���b�N�ʒu�j
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 cursorSize = new Vector2(64, 64);  // �D���ȃT�C�Y��ݒ�

    void Start()
    {
        Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
    }
}

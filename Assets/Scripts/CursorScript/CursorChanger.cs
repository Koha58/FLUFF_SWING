using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �}�E�X�̈ʒu��UI��Image��Ǐ]�����邱�ƂŁA�J�X�^���J�[�\������������N���X�B
/// Canvas��Image���J�[�\���Ƃ��Ďg���A�T�C�Y��A�j���[�V�����̎��R�x�����߂���B
/// </summary>
public class CustomCursor : MonoBehaviour
{
    // UI��̃J�[�\���摜�iImage�R���|�[�l���g�j��Inspector�ŃA�^�b�`����
    [SerializeField] private Image cursorImage;

    // �}�E�X�N���b�N�̊�ʒu�i�z�b�g�X�|�b�g�j�𒲐����邽�߂̃I�t�Z�b�g
    [SerializeField] private Vector2 offset = Vector2.zero;

    void Start()
    {
        // OS�f�t�H���g�̃J�[�\�����\���ɂ���iImage�J�[�\���݂̂�\���j
        Cursor.visible = false;
    }

    void Update()
    {
        // �}�E�X�̃X�N���[�����W��Canvas�̃��[�J�����W�ianchoredPosition�j�ɕϊ�
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cursorImage.canvas.transform as RectTransform, // �Ώۂ�Canvas
            Input.mousePosition,                           // ���݂̃}�E�X���W�i�X�N���[�����W�j
            cursorImage.canvas.worldCamera,                // �J�����iOverlay�Ȃ�null�ł�OK�j
            out pos                                        // �ϊ���̍��W
        );

        // UI�J�[�\�����}�E�X�ʒu�Ɉړ��i�I�t�Z�b�g���l���j
        cursorImage.rectTransform.anchoredPosition = pos + offset;
    }
}


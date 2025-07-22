using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI�{�^���ɃJ�[�\��������Ă���Ԃ����A�q�I�u�W�F�N�g�i�����e�L�X�g�Ȃǁj��\������N���X�B
/// �{�^���ɂ��̃X�N���v�g���A�^�b�`���A�\���Ώۂ̃I�u�W�F�N�g���C���X�y�N�^�[����ݒ肷��B
/// </summary>
public class HoverTextDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("�}�E�X�I�[�o�[���ɕ\��������q�I�u�W�F�N�g")]
    [SerializeField]
    private GameObject targetTextObject;

    /// <summary>
    /// ���������ɑΏۃI�u�W�F�N�g���\���ɂ���B
    /// </summary>
    private void Awake()
    {
        if (targetTextObject != null)
        {
            targetTextObject.SetActive(false); // �Q�[���J�n���͔�\��
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] targetTextObject ���ݒ肳��Ă��܂���B");
        }
    }

    /// <summary>
    /// �}�E�X�J�[�\�����{�^���ɓ��������ɌĂ΂��B
    /// �w�肳�ꂽ�I�u�W�F�N�g��\������B
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetTextObject != null)
        {
            targetTextObject.SetActive(true);
        }
    }

    /// <summary>
    /// �}�E�X�J�[�\�����{�^�����痣�ꂽ���ɌĂ΂��B
    /// �w�肳�ꂽ�I�u�W�F�N�g���\���ɂ���B
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetTextObject != null)
        {
            targetTextObject.SetActive(false);
        }
    }
}

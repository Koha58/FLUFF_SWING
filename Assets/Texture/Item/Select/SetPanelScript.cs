// �X�e�[�W�Z���N�g>SceneManager�Ŏg�p

using UnityEngine;

public class SetPanelScript : MonoBehaviour
{
    [Header("���ʐݒ�p�l��")]
    [SerializeField] private GameObject setPanel;

    // ������Ԃ͔�\��
    private void Start()
    {
        if (setPanel != null)
            setPanel.SetActive(false);
    }

    // �\��
    public void OnPanel()
    {
        if (setPanel == null) return;
        setPanel.SetActive(true);

        Debug.Log("�\��");
    }

    // ��\��
    public void OffPanel()
    {
        if (setPanel != null)
            setPanel.SetActive(false);

        Debug.Log("��\��");
    }

}

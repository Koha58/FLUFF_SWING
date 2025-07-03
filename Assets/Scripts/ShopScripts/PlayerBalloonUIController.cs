using UnityEngine;

/// <summary>
/// �v���C���[��UI�\���𐧌䂷��N���X�B
/// NPC�̐����o�����S���\������A���������ɂ���Ƃ��A
/// Player�̎qUI�i�͂��^�������{�^���j��\���B
/// �������A�����O�ɏo���UI�͔�\���ɁB
/// �Ăы������ɓ���΁A�ĕ\�������B
/// </summary>
public class PlayerBalloonUIController : MonoBehaviour
{
    /// <summary>�͂�/�������{�^�����z�u���ꂽUI�p�l��</summary>
    [SerializeField]
    private GameObject uiPanel;

    /// <summary>NPC�̐����o���e�L�X�g����R���|�[�l���g</summary>
    [SerializeField]
    private BalloonTextController npcBalloon;

    /// <summary>NPC��Transform�i�ʒu���擾�p�j</summary>
    [SerializeField]
    private Transform npcTransform;

    /// <summary>UI�\�����g���K�[���鋗���i3���[�g���j</summary>
    private float triggerDistance = 3f;

    /// <summary>�v���C���[���������ɂ��邩�̏�ԊǗ�</summary>
    private bool isInRange = false;

    void Start()
    {
        // UI�͍ŏ���\���ɂ���
        uiPanel.SetActive(false);

        // NPC�̐����o�����S���\�����ꂽ�C�x���g�ɃR�[���o�b�N�o�^
        npcBalloon.OnFullyDisplayed += OnBalloonFullyDisplayed;
    }

    void Update()
    {
        // �v���C���[��NPC�Ԃ̋������v�Z
        float distance = Vector3.Distance(transform.position, npcTransform.position);

        // �O�t���[���̋����������ۑ�
        bool wasInRange = isInRange;

        // ���t���[���̋�����������X�V
        isInRange = distance < triggerDistance;

        // �����O���狗�����ɓ������u��
        if (!wasInRange && isInRange)
        {
            TryShowUI(); // UI�\������E�\������
        }

        // ���������狗���O�ɏo���u��
        if (wasInRange && !isInRange)
        {
            HideUI(); // UI��\��
        }
    }

    /// <summary>
    /// �������ɓ��������ɌĂ΂�A
    /// �����o�����S���\������Ă����UI��\������B
    /// </summary>
    private void TryShowUI()
    {
        Debug.Log("TryShowUI() �Ă΂ꂽ - IsFullyDisplayed = " + npcBalloon.IsFullyDisplayed);

        if (npcBalloon.IsFullyDisplayed)
        {
            Debug.Log("UI�\���I");
            uiPanel.SetActive(true); // UI��\��
        }
        else
        {
            Debug.Log("�����o���������Ȃ̂�UI��\��");
            // UI�͔�\���̂܂܁i�\�����Ȃ��j
        }
    }

    /// <summary>
    /// NPC�̐����o�����S���\�����ꂽ�ۂɌĂ΂��C�x���g�n���h���B
    /// �v���C���[���������ɂ����UI��\������B
    /// </summary>
    private void OnBalloonFullyDisplayed()
    {
        float distance = Vector3.Distance(transform.position, npcTransform.position);

        if (distance < triggerDistance)
        {
            uiPanel.SetActive(true); // UI��\��
        }
    }

    /// <summary>
    /// �v���C���[���u�͂��v�{�^�������������̏����B
    /// UI���\���ɂ��A�K�v�ɉ����Ēǉ��������s���B
    /// </summary>
    public void OnClickYes()
    {
        HideUI();
        // �����Ɂu�͂��v�I�����̏�����ǉ��\�i��F�A�C�e�������Ȃǁj
    }

    /// <summary>
    /// �v���C���[���u�������v�{�^�������������̏����B
    /// UI���\���ɂ��A�K�v�ɉ����ăL�����Z���������s���B
    /// </summary>
    public void OnClickNo()
    {
        HideUI();
        // �����Ɂu�������v�I�����̃L�����Z��������ǉ��\
    }

    /// <summary>
    /// UI�p�l�����\���ɂ��鋤�ʏ����B
    /// </summary>
    private void HideUI()
    {
        uiPanel.SetActive(false);
    }
}

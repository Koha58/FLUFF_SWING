using UnityEngine;

/// <summary>
/// �����o�����̕����I�u�W�F�N�g���A
/// �e�I�u�W�F�N�g�̍��E���]�iscale.x = -1�j�ɉe�����ꂸ�A
/// ��ɐ����������ŕ\�������悤�ɂ���N���X�B
/// </summary>
public class BalloonTextUnflipper : MonoBehaviour
{
    /// <summary>
    /// �����̃��[�J���X�P�[���iText�̌��̑傫���E�����j���L�^
    /// </summary>
    private Vector3 originalScale;

    void Start()
    {
        // �ŏ��̃X�P�[�����L�����Ă����i�ύX����Ȃ��O��j
        originalScale = transform.localScale;
    }

    void LateUpdate()
    {
        // �e�I�u�W�F�N�g�̃��[���h�X�P�[���ilossyScale�j���擾
        // �e�� scale.x �����̏ꍇ�͍��E���]���Ă���Ɣ���
        Vector3 parentScale = transform.parent.lossyScale;

        // �����̌�������ɐ��ɕۂ��߁A�e�����]���Ă����玩����X���𔽓]���đł�����
        float xScale = parentScale.x < 0 ? -originalScale.x : originalScale.x;

        // �X�P�[�����X�V���Ĕ��]��ł������iY��Z�͂��̂܂܁j
        transform.localScale = new Vector3(xScale, originalScale.y, originalScale.z);
    }
}

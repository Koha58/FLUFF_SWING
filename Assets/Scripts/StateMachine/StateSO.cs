using UnityEngine;

/// <summary>
/// ScriptableObject�ō���Ԃ̊��N���X�i�W�F�l���b�N�j
/// IState<T>�C���^�[�t�F�[�X���������A��ԂƂ��Ďg�����߂̒��ۃN���X
/// </summary>
/// <typeparam name="T">��Ԃ̃I�[�i�[�N���X�̌^</typeparam>
public abstract class StateSO<T> : ScriptableObject, IState<T>
{
    /// <summary>
    /// ��Ԃɓ��������̏����i���ۃ��\�b�h�j
    /// </summary>
    /// <param name="owner">��Ԃ̃I�[�i�[�N���X</param>
    public abstract void Enter(T owner);

    /// <summary>
    /// ���t���[���Ă΂���Ԃ̍X�V�����i���ۃ��\�b�h�j
    /// </summary>
    /// <param name="owner">��Ԃ̃I�[�i�[�N���X</param>
    /// <param name="deltaTime">�O�t���[������̌o�ߎ��ԁi�b�j</param>
    public abstract void Tick(T owner, float deltaTime);

    /// <summary>
    /// ��Ԃ𔲂��鎞�̏����i���ۃ��\�b�h�j
    /// </summary>
    /// <param name="owner">��Ԃ̃I�[�i�[�N���X</param>
    public abstract void Exit(T owner);
}

using UnityEngine;
using System.Collections.Generic;

public class BackgroundManager : MonoBehaviour
{
    public GameObject backgroundPrefab;     // �w�i�v���n�u
    public Transform player;                // �v���C���[�i�E�ɐi�ށj
    public float backgroundWidth = 20f;     // �w�i�摜�̉����iUnity�P�ʁj
    public int preloadCount = 3;            // �����ɕ\�����Ă�������
    public float deleteDistance = 30f;      // �J�������炱��ȏ㗣�ꂽ��폜

    private List<GameObject> backgrounds = new List<GameObject>();
    private float nextSpawnX = 0f;

    void Start()
    {
        for (int i = 0; i < preloadCount; i++)
        {
            SpawnBackground();
        }
    }

    void Update()
    {
        // �v���C���[�����̔w�i�ʒu�ɋ߂Â�����ǉ�
        if (player.position.x + backgroundWidth * preloadCount > nextSpawnX)
        {
            SpawnBackground();
        }

        // �w�i�폜����
        for (int i = backgrounds.Count - 1; i >= 0; i--)
        {
            if (player.position.x - backgrounds[i].transform.position.x > deleteDistance)
            {
                Destroy(backgrounds[i]);
                backgrounds.RemoveAt(i);
            }
        }
    }

    void SpawnBackground()
    {
        // �v���n�u�Ɠ���Y���W���g�����߂ɁA�ŏ��̔w�i�̍������L�^���Ďg���̂��x�X�g
        float backgroundY = backgroundPrefab.transform.position.y;  // �� �������d�v�I

        Vector3 spawnPos = new Vector3(nextSpawnX, backgroundY, 0f);
        GameObject bg = Instantiate(backgroundPrefab, spawnPos, Quaternion.identity);
        backgrounds.Add(bg);
        nextSpawnX += backgroundWidth;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public GameObject backgroundPrefab;  // �w�i�v���n�u
    public Transform player;             // �v���C���[
    public int preloadCount = 2;         // ���O�ɐ������Ă�����
    public float backgroundWidth = 20f;  // �w�i�摜�̕�
    public float deleteDistance = 30f;   // ��苗�����߂����w�i�͍폜

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
        // �v���C���[����苗���i�񂾂�w�i�𐶐�
        if (player.position.x + backgroundWidth * preloadCount > nextSpawnX)
        {
            SpawnBackground();
        }

        // ��苗�����߂����w�i���폜
        for (int i = backgrounds.Count - 1; i >= 0; i--)
        {
            if (player.position.x > backgrounds[i].transform.position.x + deleteDistance)
            {
                Destroy(backgrounds[i]);
                backgrounds.RemoveAt(i);
            }
        }
    }

    void SpawnBackground()
    {
        // nextSpawnX�̈ʒu�ɔw�i�𐶐����鏈��
        float backgroundY = backgroundPrefab.transform.position.y; // ���̔w�i�Ɠ�������
        Vector3 spawnPos = new Vector3(nextSpawnX, backgroundY, 0f);
        GameObject bg = Instantiate(backgroundPrefab, spawnPos, Quaternion.identity);
        backgrounds.Add(bg);
        nextSpawnX += backgroundWidth;
    }
}

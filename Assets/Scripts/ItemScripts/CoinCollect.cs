using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class CoinCollect : MonoBehaviour
{
    bool isGet;             // �l���ς݃t���O

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isGet && collision.gameObject.CompareTag("Player"))
        {
            GameManagerScript.tempCoinNum++;
            Debug.Log("�R�C���̖����F" + GameManagerScript.tempCoinNum);

            Destroy(gameObject);
        }
    }

}

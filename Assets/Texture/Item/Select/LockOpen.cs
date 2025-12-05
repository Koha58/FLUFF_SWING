using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KeyUnlockAnimation : MonoBehaviour
{
    [SerializeField] private Image keyImage;      // Œ®‚ÌImage
    [SerializeField] private Sprite[] keySprites; // 3–‡‚Ì‰æ‘œi0¨1¨2j

    [SerializeField] private float interval = 0.5f; // Ø‘ÖŠÔŠui2`3•b‚È‚ç0.5 ~ 3 ‚È‚Çj

    public void PlayUnlockAnimation()
    {
        StartCoroutine(UnlockRoutine());
    }

    private IEnumerator UnlockRoutine()
    {
        for (int i = 0; i < keySprites.Length; i++)
        {
            keyImage.sprite = keySprites[i];
            yield return new WaitForSeconds(interval);
        }
    }
}

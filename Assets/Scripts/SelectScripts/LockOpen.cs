using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LockOpen : MonoBehaviour
{
    [Header("アニメーション用スプライト3枚")]
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameTime = 0.25f;

    private Image img;
    private bool isPlaying = false;

    private void Awake()
    {
        img = GetComponent<Image>();
    }

    public void PlayUnlockAnimation(Action onFinished = null)
    {
        if (isPlaying) return;
        StartCoroutine(PlayRoutine(onFinished));
    }

    private IEnumerator PlayRoutine(Action onFinished)
    {
        isPlaying = true;

        foreach (var frame in frames)
        {
            img.sprite = frame;
            yield return new WaitForSeconds(frameTime); // アニメーションの速度
        }

        isPlaying = false;
        onFinished?.Invoke();
    }

}

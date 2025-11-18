using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CloseTransition : MonoBehaviour
{
    [SerializeField]
    private Material _transitionIn;

    void Start()
    {

    }

    /// <summary>
    /// time秒かけてトランジションを行う
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator Animate(Material material, float time)
    {
        GetComponent<Image>().material = material;
        float current = 0;
        while (current < time)
        {
            material.SetFloat("_Alpha", current / time);
            yield return new WaitForEndOfFrame();
            current += Time.deltaTime;
        }
        material.SetFloat("_Alpha", 1);
    }

    public IEnumerator Play()   // ← Start() を使わずこちらを呼ぶ
    {
        yield return Animate(_transitionIn, 1);
    }

}

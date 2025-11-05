using UnityEngine;

namespace FlatShadowCaster.Sample
{
    public class Rotater : MonoBehaviour
    {
        [SerializeField] float speed = 10f;

        void Update()
        {
            transform.Rotate(0, 0, speed * Time.deltaTime);
        }
    }
}
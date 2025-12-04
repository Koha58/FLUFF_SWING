//using UnityEngine;

//public class ParallaxBackground : MonoBehaviour
//{
//    [System.Serializable]
//    public class Layer
//    {
//        public Renderer renderer;   // SpriteRenderer or MeshRenderer
//        public float speed = 0.1f;  // スクロール速度
//    }

//    [Header("背景レイヤー3つ")]
//    public Layer[] layers = new Layer[3];

//    void Update()
//    {
//        float dt = Time.deltaTime;

//        foreach (var layer in layers)
//        {
//            if (layer.renderer == null) continue;

//            // 現在のオフセット
//            Vector2 offset = layer.renderer.material.mainTextureOffset;

//            // 時間によるスクロール
//            offset.x += layer.speed * dt;

//            // ループのために0?1で循環
//            if (offset.x > 1f) offset.x -= 1f;

//            layer.renderer.material.mainTextureOffset = offset;
//        }
//    }
//}

using UnityEngine;

public class ParallaxScroll : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Renderer renderer; // SpriteRenderer か MeshRenderer
        public float speed = 0.1f; // 左方向に流れる速度（正の値）
    }

    public Layer[] layers;

    void Update()
    {
        float dt = Time.deltaTime;

        foreach (var layer in layers)
        {
            if (layer.renderer == null) continue;

            var mat = layer.renderer.material;
            Vector2 offset = mat.mainTextureOffset;

            // 横方向だけスクロール（左方向）
            offset.x = (offset.x + layer.speed * dt) % 1f;

            mat.mainTextureOffset = offset;
        }
    }
}

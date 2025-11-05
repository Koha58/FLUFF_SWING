using UnityEngine;

namespace FlatShadowCaster
{
    public class FlatShadowCaster : MonoBehaviour
    {
        [Header("Shadow Settings")]
        [SerializeField] Color32 shadowColor;
        [SerializeField] Vector2 shadowDistance;
        [SerializeField] float shadowResolution = 1f;
        [SerializeField] FilterMode shadowFilterMode = FilterMode.Bilinear;
        [SerializeField] string shadowSortingLayerName;
        [SerializeField] int shadowOrderInLayer;

        [Header("Target Settings")]
        [SerializeField] Camera targetCamera;
        [SerializeField] LayerMask targetLayer;
        
        private Camera shadowCamera;
        private RenderTexture shadowTexture;
        private GameObject shadowSpriteObject;
        private SpriteRenderer shadowSpriteRenderer;

        void Start()
        {
            // Automatically load shadowPrefab from Resources
            GameObject shadowPrefab = Resources.Load<GameObject>("ShadowSprite");
            if (shadowPrefab == null)
            {
                Debug.LogError("ShadowSprite.prefab not found in FlatShadowCaster/Resources/ folder");
                return;
            }

            // Create shadow camera
            shadowCamera = new GameObject("shadowCamera").AddComponent<Camera>();
            shadowCamera.transform.SetParent(targetCamera.transform);
            shadowCamera.CopyFrom(targetCamera);
            shadowCamera.cullingMask = targetLayer;
            shadowCamera.clearFlags = CameraClearFlags.SolidColor;
            shadowCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            shadowCamera.transform.localPosition = -shadowDistance;
            shadowCamera.transform.localRotation = Quaternion.identity;

            // Create shadow sprite
            shadowSpriteObject = Instantiate(shadowPrefab);
            shadowSpriteObject.transform.SetParent(transform);
            shadowSpriteRenderer = shadowSpriteObject.GetComponent<SpriteRenderer>();
            shadowSpriteRenderer.sortingLayerName = shadowSortingLayerName;
            shadowSpriteRenderer.sortingOrder = shadowOrderInLayer;

            // Create shadow texture
            shadowTexture = new RenderTexture((int)(Screen.width * shadowResolution), (int)(Screen.height * shadowResolution), 24);
            shadowTexture.filterMode = shadowFilterMode;
            shadowCamera.targetTexture = shadowTexture;
        }

        void LateUpdate()
        {
            shadowCamera.transform.localPosition = -shadowDistance;

            // Render shadow texture
            RenderTexture.active = shadowTexture;
            shadowCamera.Render();
            RenderTexture.active = null;
            // Set shadow texture to sprite
            shadowSpriteRenderer.material.SetTexture("_Texture", shadowTexture);
            shadowSpriteRenderer.material.SetColor("_Color", shadowColor);

            // Position shadow sprite
            shadowSpriteObject.transform.position = targetCamera.transform.position + (targetCamera.transform.forward * 1f);
            shadowSpriteRenderer.transform.localScale = new Vector3(targetCamera.orthographicSize * 2f * targetCamera.aspect, targetCamera.orthographicSize * 2f, 1f);
        }

        void OnDestroy()
        {
            if (shadowCamera != null && shadowCamera.gameObject != null)
            {
                Destroy(shadowCamera.gameObject);
            }
            if (shadowSpriteObject != null)
            {
                Destroy(shadowSpriteObject);
            }
            if (shadowTexture != null)
            {
                shadowTexture.Release();
                Destroy(shadowTexture);
            }
        }
    }
}
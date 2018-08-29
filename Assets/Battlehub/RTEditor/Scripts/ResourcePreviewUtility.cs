using Battlehub.RTSaveLoad2;
using Battlehub.Utils;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTEditor
{
    public interface IResourcePreviewUtility
    {
        byte[] CreatePreviewData(UnityObject obj);
        byte[] CreatePreviewData(AssetItem projectItem);
    }

    public class ResourcePreviewUtility : MonoBehaviour, IResourcePreviewUtility
    {
        [SerializeField]
        private ObjectToTexture m_objectToTextureCamera;
  
        [SerializeField]
        private GameObject m_fallbackPrefab;
        
        [SerializeField]
        private Vector3 m_scale = new Vector3(0.9f, 0.9f, 0.9f);

        private Shader m_unlitTexShader;


        private void Awake()
        {
            m_unlitTexShader = Shader.Find("Unlit/Texture");
        }

        private void OnDestroy()
        {
         
        }

        private byte[] TakeSnapshot(GameObject obj)
        {
            m_objectToTextureCamera.defaultScale = m_scale;
            m_objectToTextureCamera.gameObject.SetActive(true);
            Texture2D texture = m_objectToTextureCamera.TakeObjectSnapshot(obj, m_fallbackPrefab);
            m_objectToTextureCamera.gameObject.SetActive(false);
            byte[] result = texture.EncodeToPNG();
            Destroy(texture);
            return result;
        }

        public byte[] CreatePreviewData(UnityObject obj)
        {
            byte[] previewData = new byte[0];
            if(obj is GameObject)
            {
                GameObject go = (GameObject)obj;
                previewData = TakeSnapshot(go);
            }
            else if(obj is Material)
            {
                Material material = (Material)obj;
                Shader shader = material.shader;
                bool replaceParticlesShader = shader != null && shader.name.StartsWith("Particles/");
                if (replaceParticlesShader)
                {
                    material.shader = m_unlitTexShader;
                }

                GameObject materialSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                materialSphere.transform.position = Vector3.zero;

                MeshRenderer renderer = materialSphere.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = material;

                previewData = TakeSnapshot(materialSphere);
                DestroyImmediate(materialSphere);

                if(replaceParticlesShader)
                {
                    material.shader = shader;
                }
            }
            else if(obj is Texture2D)
            {
                Texture2D texture = (Texture2D)obj;
                if(texture.IsReadable())
                {
                    texture = Instantiate(texture);
                    TextureScale.Bilinear(texture, m_objectToTextureCamera.snapshotTextureWidth, m_objectToTextureCamera.snapshotTextureHeight);
                    previewData = texture.EncodeToPNG();
                    Destroy(texture);
                }
            }
            else if(obj is Sprite)
            {
                Sprite sprite = (Sprite)obj;
                if(sprite.texture != null && sprite.texture.IsReadable())
                {
                    Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                    Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                                 (int)sprite.textureRect.y,
                                                                 (int)sprite.textureRect.width,
                                                                 (int)sprite.textureRect.height);
                    texture.SetPixels(newColors);
                    texture.Resize(m_objectToTextureCamera.snapshotTextureWidth, m_objectToTextureCamera.snapshotTextureHeight);
                    previewData = texture.EncodeToPNG();

                    Destroy(texture);
                }                
            }

            return previewData;
        }

        public byte[] CreatePreviewData(AssetItem projectItem)
        {
            return new byte[0];
        }
    }
}




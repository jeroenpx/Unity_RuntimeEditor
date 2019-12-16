using Battlehub.RTCommon;
using Battlehub.RTHandles;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public interface IGameObjectCmd
    {
        bool CanExec(string cmd);
        void Exec(string cmd);
    }

    public class GameObjectCmd : MonoBehaviour, IGameObjectCmd
    {
        private IRuntimeEditor m_editor;

        [SerializeField]
        private Material m_defaultMaterial = null;

     
        private void Awake()
        {
            m_editor = IOC.Resolve<IRuntimeEditor>();   

            if(RenderPipelineInfo.Type != RPType.Standard)
            {
                m_defaultMaterial = RenderPipelineInfo.DefaultMaterial;
            }
        }

        public bool CanExec(string cmd)
        {
            return true;
        }

        public void Exec(string cmd)
        {
            cmd = cmd.ToLower();
            GameObject go = null;
            switch (cmd)
            {
                case "createempty":
                    go = new GameObject();
                    go.name = "Empty";
                    break;
                case "createemptychild":
                    go = new GameObject();
                    go.name = "Empty";
                    IRuntimeSelection selection = m_editor.Selection;
                    go.transform.SetParent(selection.activeTransform, false);
                    break;
                case "cube":
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.GetComponent<Renderer>().sharedMaterial = m_defaultMaterial;
                    break;
                case "sphere":
                    go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    go.GetComponent<Renderer>().sharedMaterial = m_defaultMaterial;
                    break;
                case "capsule":
                    go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    go.GetComponent<Renderer>().sharedMaterial = m_defaultMaterial;
                    break;
                case "cylinder":
                    go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    go.GetComponent<Renderer>().sharedMaterial = m_defaultMaterial;
                    break;
                case "plane":
                    go = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    go.GetComponent<Renderer>().sharedMaterial = m_defaultMaterial;
                    break;
                case "quad":
                    go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    go.GetComponent<Renderer>().sharedMaterial = m_defaultMaterial;
                    break;
                case "directionallight":
                    {
                        go = new GameObject();
                        go.name = "Directional Light";
                        Light light = go.AddComponent<Light>();
                        light.type = LightType.Directional;
                    }
                    break;
                case "pointlight":
                    {
                        go = new GameObject();
                        go.name = "Point Light";
                        Light light = go.AddComponent<Light>();
                        light.type = LightType.Point;
                    }
                    break;
                case "spotlight":
                    {
                        go = new GameObject();
                        go.name = "Spot Light";
                        Light light = go.AddComponent<Light>();
                        light.type = LightType.Spot;
                    }
                    break;
                case "camera":
                    {
                        go = new GameObject();
                        go.SetActive(false);
                        go.name = "Camera";
                        go.AddComponent<Camera>();
                        go.AddComponent<GameViewCamera>();
                    }
                    break;
            }

            if(go != null)
            {
                m_editor.AddGameObjectToScene(go);
            }
            
        }
    }
}



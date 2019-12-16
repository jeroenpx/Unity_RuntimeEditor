using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.UIControls.MenuControl;
using UnityEngine;
namespace Battlehub.RTTerrain
{
    [MenuDefinition(-1)]
    public class TerrainInit : EditorOverride
    {
        [SerializeField]
        private GameObject m_terrainView = null;

        [SerializeField]
        private TerrainComponentEditor m_terrainComponentEditor = null;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();
            Register();
        }

        private void Register()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            if (m_terrainView != null)
            {
                RegisterWindow(wm, "TerrainEditor", "Terrain Editor", 
                    Resources.Load<Sprite>("icons8-earth-element-24"), m_terrainView, false); 
            }

            if(m_terrainComponentEditor != null)
            {
                IEditorsMap editorsMap = IOC.Resolve<IEditorsMap>();
                editorsMap.AddMapping(typeof(Terrain), m_terrainComponentEditor.gameObject, true, false);
            }
        }

        private void RegisterWindow(IWindowManager wm, string typeName, string header, Sprite icon, GameObject prefab, bool isDialog)
        {
            wm.RegisterWindow(new CustomWindowDescriptor
            {
                IsDialog = isDialog,
                TypeName = typeName,
                Descriptor = new WindowDescriptor
                {
                    Header = header,
                    Icon = icon,
                    MaxWindows = 1,
                    ContentPrefab = prefab
                }
            });
        }

        [MenuCommand("MenuWindow/Terrain Editor")]
        public static void OpenTerrainEditor()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.CreateWindow("TerrainEditor");
        }

        [MenuCommand("MenuGameObject/3D Object/Terrain")]
        public static void CreateTerrain()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            TerrainData terrainData = new TerrainData();
            terrainData.SetDetailResolution(1024, 32);
            terrainData.heightmapResolution = 513;
            terrainData.size = new Vector3(200, 20, 200);

            ITerrainSettings terrainSettings = IOC.Resolve<ITerrainSettings>();

            terrainData.terrainLayers = new[]
            {
                new TerrainLayer() { diffuseTexture = terrainSettings != null ? terrainSettings.DefaultTexture : (Texture2D)Resources.Load("Textures/RTT_DefaultGrass") }
            };

            GameObject go = Terrain.CreateTerrainGameObject(terrainData);
            go.isStatic = false;
            if (go != null)
            {
                editor.AddGameObjectToScene(go);
            }
        }
    }
}



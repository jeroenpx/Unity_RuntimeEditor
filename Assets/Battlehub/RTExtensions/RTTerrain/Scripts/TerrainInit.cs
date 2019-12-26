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

            if(IOC.Resolve<ITerrainSettings>() == null && gameObject.GetComponent<TerrainSettings>() == null)
            {
                gameObject.AddComponent<TerrainSettings>();
            }
            if(IOC.Resolve<ITerrainCutoutMaskRenderer>() == null && gameObject.GetComponent<TerrainCutoutMaskRenderer>() == null)
            {
                gameObject.AddComponent<TerrainCutoutMaskRenderer>();
            }

            Register();
        }

        private void Register()
        {
            ILocalization lc = IOC.Resolve<ILocalization>();
            lc.LoadStringResources("RTTerrain.StringResources");

            IWindowManager wm = IOC.Resolve<IWindowManager>();
            if (m_terrainView != null)
            {
                RegisterWindow(wm, "TerrainEditor", lc.GetString("ID_RTTerrain_WM_Header_TerrainEditor", "Terrain Editor"),
                    Resources.Load<Sprite>("icons8-earth-element-24"), m_terrainView, false);

                IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();
                appearance.ApplyColors(m_terrainView);
            }

            if(m_terrainComponentEditor != null)
            {
                IEditorsMap editorsMap = IOC.Resolve<IEditorsMap>();
                editorsMap.AddMapping(typeof(Terrain), m_terrainComponentEditor.gameObject, true, false);

                IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();
                appearance.ApplyColors(m_terrainComponentEditor.gameObject);
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

        [MenuCommand("MenuWindow/ID_RTTerrain_WM_Header_TerrainEditor")]
        public static void OpenTerrainEditor()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.CreateWindow("TerrainEditor");
        }

        [MenuCommand("MenuGameObject/3D Object/ID_RTTerrain_MenuGameObject_Terrain")]
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

            float[,,] alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            int amapY = alphaMaps.GetLength(0);
            int amapX = alphaMaps.GetLength(1);

            for (int y = 0; y < amapY; y++)
            {
                for (int x = 0; x < amapX; x++)
                {
                    alphaMaps[y, x, 0] = 1;
                }
            }

            terrainData.SetAlphamaps(0, 0, alphaMaps);

            GameObject go = Terrain.CreateTerrainGameObject(terrainData);
            go.isStatic = false;
            if (go != null)
            {
                editor.AddGameObjectToScene(go);
            }
        }
    }
}



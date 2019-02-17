using Battlehub.RTSaveLoad2;
using System.IO;
using UnityEditor;

namespace Battlehub.AssetStoreTools
{
    public static class ToolsMenu
    {
        [MenuItem("Asset Store Tools/ RT SaveLoad Clean")]
        public static void Clean()
        {
            AssetDatabase.DeleteAsset("Assets/Battlehub/RTSaveLoad2_Data/CustomImplementation");
            AssetDatabase.DeleteAsset("Assets/Battlehub/RTSaveLoad2_Data/Mappings");
            AssetDatabase.DeleteAsset("Assets/Battlehub/RTSaveLoad2_Data/Scripts");
            AssetDatabase.DeleteAsset("Assets/Battlehub/RTSaveLoad2_Data/RTSLTypeModel.dll");

        }
    }

}

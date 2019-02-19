using Battlehub.RTSL;
using System.IO;
using UnityEditor;

namespace Battlehub.AssetStoreTools
{
    public static class ToolsMenu
    {
        [MenuItem("Asset Store Tools/ RT SaveLoad Clean")]
        public static void Clean()
        {
            AssetDatabase.DeleteAsset("Assets/Battlehub/RTSL_Data/CustomImplementation");
            AssetDatabase.DeleteAsset("Assets/Battlehub/RTSL_Data/Mappings");
            AssetDatabase.DeleteAsset("Assets/Battlehub/RTSL_Data/Scripts");
            AssetDatabase.DeleteAsset("Assets/Battlehub/RTSL_Data/RTSLTypeModel.dll");

        }
    }

}

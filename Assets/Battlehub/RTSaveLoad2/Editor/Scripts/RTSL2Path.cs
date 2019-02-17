using System.Linq;
using UnityEditor;

namespace Battlehub.RTSaveLoad2
{
    public static class RTSL2Path 
    {
        public static string SaveLoadRoot
        {
            get { return @"/" + BHPath.Root + @"/RTSaveLoad2"; }
        }
        
        public static string UserRoot
        {
            get
            {
                string userRoot = EditorPrefs.GetString("RTSL2DataRoot");
                if(string.IsNullOrEmpty(userRoot))
                {
                    string dll = AssetDatabase.FindAssets(TypeModelDll.Replace(".dll", string.Empty)).FirstOrDefault();
                    if(string.IsNullOrEmpty(dll))
                    {
                        return "/" + BHPath.Root + "/RTSaveLoad2_Data";
                    }
                    string path = AssetDatabase.GUIDToAssetPath(dll).Replace(TypeModelDll, "");
                    if(string.IsNullOrEmpty(path))
                    {
                        return "/" + BHPath.Root + "/RTSaveLoad2_Data";
                    }
                    int firstIndex = path.IndexOf("/");
                    if(firstIndex < 0)
                    {
                        return "/" + BHPath.Root + "/RTSaveLoad2_Data";
                    }

                    return path.Remove(0, firstIndex + 1);
                }
                if(!userRoot.StartsWith("/"))
                {
                    userRoot = "/" + userRoot;
                }
                return userRoot;
            }
            set
            {
                EditorPrefs.SetString("RTSL2DataRoot", value);
            }
        }

        public static string EditorPrefabsPath { get { return SaveLoadRoot + "/Editor/Prefabs"; } }
        public static string UserPrefabsPath { get { return UserRoot + "/Mappings/Editor"; } }

        public static string FilePathStoragePath { get { return "Assets" + UserPrefabsPath + @"/FilePathStorage.prefab"; } }
        public static string ClassMappingsStoragePath { get { return "Assets" + UserPrefabsPath + @"/ClassMappingsStorage.prefab"; } }
        public static string ClassMappingsTemplatePath { get { return "Assets" + EditorPrefabsPath + @"/ClassMappingsTemplate.prefab"; } }
        public static string SurrogatesMappingsStoragePath { get { return "Assets" + UserPrefabsPath + @"/SurrogatesMappingsStorage.prefab"; } }
        public static string SurrogatesMappingsTemplatePath { get { return "Assets" + EditorPrefabsPath + @"/SurrogatesMappingsTemplate.prefab"; } }

        public const string ScriptsAutoFolder = "Scripts";
        public const string PersistentClassesFolder = "PersistentClasses";
        public const string PersistentCustomImplementationClasessFolder = "CustomImplementation";
        public const string LibrariesFolder = "Libraries";
        public const string TypeModelDll = "RTSLTypeModel.dll";
    }
}

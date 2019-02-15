
namespace Battlehub.RTSaveLoad2
{
    public static class RTSL2Path 
    {
        public const string SaveLoadRoot = @"/" + BHPath.Root + @"/RTSaveLoad2";

        public const string UserRoot = SaveLoadRoot + "/User";
        public const string SystemRoot = SaveLoadRoot + "/System";

        public const string EditorPrefabsPath = SystemRoot + "/Editor/Prefabs";
        public const string UserPrefabsPath = UserRoot + "/Mappings/Editor";
        

        public const string FilePathStoragePath = "Assets" + UserPrefabsPath + @"/FilePathStorage.prefab";
        public const string ClassMappingsStoragePath = "Assets" + UserPrefabsPath + @"/ClassMappingsStorage.prefab";
        public const string ClassMappingsTemplatePath = "Assets" + EditorPrefabsPath + @"/ClassMappingsTemplate.prefab";
        public const string SurrogatesMappingsStoragePath = "Assets" + UserPrefabsPath + @"/SurrogatesMappingsStorage.prefab";
        public const string SurrogatesMappingsTemplatePath = "Assets" + EditorPrefabsPath + @"/SurrogatesMappingsTemplate.prefab";

        public const string ScriptsAutoFolder = "Scripts";
        public const string PersistentClassesFolder = "PersistentClasses";
        public const string PersistentCustomImplementationClasessFolder = "CustomImplementation";
        public const string LibrariesFolder = "Libraries";
    }
}

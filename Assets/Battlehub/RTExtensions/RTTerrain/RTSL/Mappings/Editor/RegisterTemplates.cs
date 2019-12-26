using Battlehub.RTSL;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public static class RegisterTemplates 
    {
        [InitializeOnLoadMethod]
        public static void Register()
        {
            RTSLPath.ClassMappingsTemplatePath.Add("Assets/" + BHPath.Root +"/RTExtensions/RTTerrain/RTSL/Mappings/Editor/RTTerrain.ClassMappingsTemplate.prefab");
        }
    }
}

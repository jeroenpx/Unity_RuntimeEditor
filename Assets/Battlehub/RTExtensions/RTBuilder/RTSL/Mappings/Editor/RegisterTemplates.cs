using Battlehub.RTSL;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public static class RegisterTemplates 
    {
        [InitializeOnLoadMethod]
        public static void Register()
        {
            RTSLPath.ClassMappingsTemplatePath.Add("Assets/" + BHPath.Root + "/RTExtensions/RTBuilder/RTSL/Mappings/Editor/RTBuilder.ClassMappingsTemplate.prefab");
            RTSLPath.SurrogatesMappingsTemplatePath.Add("Assets/" + BHPath.Root + "/RTExtensions/RTBuilder/RTSL/Mappings/Editor/RTBuilder.SurrogatesMappingsTemplate.prefab");
        }
    }
}

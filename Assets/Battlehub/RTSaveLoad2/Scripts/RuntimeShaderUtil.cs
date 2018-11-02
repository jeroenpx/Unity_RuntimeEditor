using Battlehub.RTCommon;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTSaveLoad2
{
    public enum RTShaderPropertyType
    {
        //     Color Property.
        Color = 0,
        //     Vector Property.
        Vector = 1,
        //     Float Property.
        Float = 2,
        //     Range Property.
        Range = 3,
        //     Texture Property.
        TexEnv = 4,

        Unknown,

        Procedural
    }

    [System.Serializable]
    public class RuntimeShaderInfo
    {
        public int dummy;
        public string Name;
        public long InstanceId;

        [System.Serializable]
        public struct RangeLimits
        {
            public float Def;
            public float Min;
            public float Max;

            public RangeLimits(float def, float min, float max)
            {
                Def = def;
                Min = min;
                Max = max;
            }
        }

        public int PropertyCount;
        public string[] PropertyDescriptions;
        public string[] PropertyNames;
        public RTShaderPropertyType[] PropertyTypes;
        public RangeLimits[] PropertyRangeLimits;
        public TextureDimension[] PropertyTexDims;
        public bool[] IsHidden;
    }

    public interface IRuntimeShaderUtil
    {
        RuntimeShaderInfo GetShaderInfo(Shader shader);
    }

    public class RuntimeShaderUtil : IRuntimeShaderUtil
    {
        private static readonly Dictionary<string, TextAsset[]> m_textAssets = new Dictionary<string, TextAsset[]>();

        private const string Path = "/" + BHPath.Root + "/RTSaveLoad/ShaderInfo";

        private IIDMap m_idMap;
        
        public RuntimeShaderUtil()
        {
            m_idMap = IOC.Resolve<IIDMap>();
        }

        public static string GetPath(bool resourcesFolder)
        {
            return Path + (resourcesFolder ? "/Resources" : string.Empty);
        }

        public static long FileNameToInstanceID(string fileName)
        {
            int index = fileName.LastIndexOf("_");
            if (index == -1)
            {
                return 0;
            }
            long id;
            if (long.TryParse(fileName.Substring(index + 1).Replace(".txt", string.Empty), out id))
            {
                return id;
            }
            return 0;
        }

        public static string GetShaderInfoFileName(Shader shader, long id, bool withoutExtension = false)
        {
            return string.Format("rt_shader_{0}_{1}" + (withoutExtension ? string.Empty : ".txt"), shader.name.Replace("/", "__"), id.ToString());
        }

        public static void AddExtra(string key, TextAsset[] textAssets)
        {
            if (!m_textAssets.ContainsKey(key))
            {
                m_textAssets.Add(key, textAssets);
            }
        }

        public static void RemoveExtra(string key)
        {
            m_textAssets.Remove(key);
        }

        public RuntimeShaderInfo GetShaderInfo(Shader shader)
        {
            if (shader == null)
            {
                throw new System.ArgumentNullException("shader");
            }

            string shaderName = GetShaderInfoFileName(shader, m_idMap.ToID(shader), true);
            TextAsset shaderInfo = Resources.Load<TextAsset>(shaderName);
            if (shaderInfo == null)
            {
                foreach (TextAsset[] extraTextAssets in m_textAssets.Values)
                {
                    shaderInfo = extraTextAssets.Where(t => t.name == shaderName).FirstOrDefault();
                    if (shaderInfo != null)
                    {
                        break;
                    }
                }
            }

            if (shaderInfo == null)
            {
                Debug.LogFormat("Shader {0} is not found", shaderName);
                return null;
            }
            return JsonUtility.FromJson<RuntimeShaderInfo>(shaderInfo.text);
        }
    }
}
    


using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using Battlehub.RTSaveLoad2.Interface;
namespace Battlehub.RTSaveLoad2
{
    public class UnityObjectFactory : IUnityObjectFactory
    {
        private static Shader m_standardShader;
        public UnityObjectFactory()
        {
            m_standardShader = Shader.Find("Standard");
            Debug.Assert(m_standardShader != null, "Standard shader is not found");
        }

        public UnityObject CreateInstance(Type type, PersistentSurrogate surrogate)
        {
            if (type == typeof(Material))
            {
                Material material = new Material(m_standardShader);
                return material;
            }
            else if (type == typeof(Texture2D))
            {
                Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, true);
                return texture;
            }
            else if(type == typeof(Shader))
            {
                Debug.LogWarning("Unable to instantiate Shader");
                return null;
            }

            try
            {
                if (surrogate != null)
                {
                    return (UnityObject)surrogate.Instantiate(type);
                }

                return (UnityObject)Activator.CreateInstance(type);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                Debug.LogWarning("Collecting scene dependencies could fix this exeption. Tools->Runtime Save Load->Collect Scene Dependencies"); 
                return null;

            }
            
        }
    }

}


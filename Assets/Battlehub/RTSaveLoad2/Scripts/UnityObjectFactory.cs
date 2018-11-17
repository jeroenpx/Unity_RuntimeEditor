using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2
{
    public interface IUnityObjectFactory
    {
        UnityObject CreateInstance(Type type);
    }

    public class UnityObjectFactory : IUnityObjectFactory
    {
        private static Shader m_standardShader;
        public UnityObjectFactory()
        {
            m_standardShader = Shader.Find("Standard");
            Debug.Assert(m_standardShader != null, "Standard shader is not found");
        }

        public UnityObject CreateInstance(Type type)
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

            return (UnityObject)Activator.CreateInstance(type);
        }
    }

}


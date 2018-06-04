using Battlehub.Utils;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2
{
    [System.Serializable]
    public struct ObjectToID
    {
        [HideInInspector]
        public string Name;
        public UnityObject Object;
        [ReadOnly]
        public int Id;

        public ObjectToID(UnityObject obj, int id)
        {
            Name = obj.name;
            Object = obj;
            Id = id;
        }
    }
    public class AssetLibrary : MonoBehaviour
    {
        public string Name;

        public ObjectToID[] Mapping;
    }
}

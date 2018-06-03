using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2
{
    public interface IIdentifiersMap
    {
        int ToId(UnityObject obj);
        UnityObject FromId(int id);
    }

    public class IdentifiersMap 
    {
        void A()
        {
            
        }
    }
}

using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battlehub.RTSaveLoad2
{
    [ProtoContract(AsReferenceDefault = true)]
    public class PersistentScene : PersistentObject
    {
        public PersistentObject[] rootGameObjects;

        public override void ReadFrom(object obj)
        {
            Scene scene = (Scene)obj;
            GameObject[] rootGameObjects = scene.GetRootGameObjects();



        }
    }

}



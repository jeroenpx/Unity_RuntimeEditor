using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

using UnityObject = UnityEngine.Object;

[System.Serializable]
public class Test
{
    public int Value;
}

public class TestBehavior : MonoBehaviour {

    public List<int> Values;

    public List<Test> Values2;

    //public List<PersistentTest> Values21;

    public Test[] Values25;

    public Vector3[] Values26;

    public List<Material> Values3;

    public UnityEvent Event;

    public Shader Shader;

  

    void Start () {

        //AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/bundledemo");
        //Action<AsyncOperation> completed = null;
        //GameObject go = null;
        //completed = result =>
        //{
        //    AssetBundle bundle = request.assetBundle;
        //    if(bundle != null)
        //    {
        //        //string[] allNames = bundle.GetAllAssetNames();
        //        //foreach(string name in allNames)
        //        //{
        //        //    Debug.Log(name);
        //        //}

        //        go = bundle.LoadAsset<GameObject>("assets/battlehub/rteditor/demo/bundledobject/monkey.prefab");
        //        Debug.Log(go);
        //    }
        //    bundle.Unload(false);
        //    Compare(go);
        //    request.completed -= completed;
        //};
        //request.completed += completed;



    }

    private void Compare(UnityObject obj)
    {
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/bundledemo");
        Action<AsyncOperation> completed = null;
        UnityObject loaded = null;
        completed = result =>
        {
            AssetBundle bundle = request.assetBundle;
            if (bundle != null)
            {
                loaded = bundle.LoadAsset<GameObject>("assets/battlehub/rteditor/demo/bundledobject/monkey.prefab");
                Debug.Assert(loaded == obj);
            }
            bundle.Unload(false);
            request.completed -= completed;
        };
        request.completed += completed;
    }

  


    // Update is called once per frame
    void Update () {
        //Values2 = Values21;

      
         
	}


    //public class Convert
}

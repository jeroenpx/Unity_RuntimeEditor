using Battlehub.RTCommon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExposeToEditorNodeGen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 30000; ++i)
        {
            GameObject go = new GameObject();
            go.name = "GameObject " + i;
            go.AddComponent<ExposeToEditor>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

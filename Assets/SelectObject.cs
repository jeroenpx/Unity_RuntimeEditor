using Battlehub.RTCommon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            IRTE rte = IOC.Resolve<IRTE>();
            rte.Selection.activeGameObject = gameObject;
        }
    }
}

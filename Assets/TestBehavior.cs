using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

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
        if(Shader != null)
        {
            Debug.Log(Shader.name);
        }

       
    }

  


    // Update is called once per frame
    void Update () {
        //Values2 = Values21;

      
         
	}


    //public class Convert
}

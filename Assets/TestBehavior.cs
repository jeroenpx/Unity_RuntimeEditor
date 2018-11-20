using System.Collections;
using System.Collections.Generic;
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

    public List<Material> Values3;

    public UnityEvent Event;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

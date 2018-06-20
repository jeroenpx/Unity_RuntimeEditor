using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ProtoBuf.ProtoContract(ImplicitFields = ProtoBuf.ImplicitFields.AllFields)]
[System.Serializable]
public class Item
{
    public List<string> Items;
}


public class ListSerializationTest : MonoBehaviour {

    public List<Item> Items;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

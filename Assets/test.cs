using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ProtoBuf.ProtoContract]
[System.Serializable]
public class Schedule1
{
    public enum ArgType { T1, T2};

    [ProtoBuf.ProtoMember(1)]
    public ArgType Arg;
}


public class test : MonoBehaviour {


    [SerializeField]
    private List<Schedule1> List;

    //[SerializeField]
    //private Mesh m_resourceMesh;
	// Use this for initialization
	void Start () {
     //   Mesh mesh = new Mesh();
     //   Resources.UnloadAsset(mesh);

        //Resources.UnloadAsset(m_resourceMesh);

        //Resources.LoadAsync()
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

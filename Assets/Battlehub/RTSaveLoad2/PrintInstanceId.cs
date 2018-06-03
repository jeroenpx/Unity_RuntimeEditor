using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PrintInstanceId : MonoBehaviour {

    [SerializeField]
    private Text m_txt;
    
    [SerializeField]
    private Material m_referencedObj;
     
	void Start () {
        m_txt.text = m_referencedObj.GetInstanceID().ToString();

        string guid;
        int localId;
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m_referencedObj.GetInstanceID(), out guid, out localId);



        Debug.Log(guid + " " + localId);

    }
	
}

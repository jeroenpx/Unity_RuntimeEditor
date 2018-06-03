using UnityEngine;

public class test : MonoBehaviour {

    public Object[] m_o;

	// Use this for initialization
	void Start () {
        Resources.UnloadUnusedAssets();
        m_o = Resources.FindObjectsOfTypeAll<Object>();
        Debug.Log(m_o.Length);
        Resources.UnloadUnusedAssets();

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

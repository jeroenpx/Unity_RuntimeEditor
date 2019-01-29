using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CustomType1000
{
   // public float[] m_array;// causes exception in PropertyEditor

    public List<float> m_list;

  //  public List<CustomType1000> m_customTypeList;

    public int intValue;
}


public class TestBehaviour : MonoBehaviour
{
    [SerializeField]
    private Color m_color;

    [SerializeField]
    private GameObject m_gameObject;

    [SerializeField]
    private Vector2 m_vector2Field;

    [SerializeField]
    private Vector4 m_vector4Field;
    [SerializeField]
    private Vector4 m_vector4FieldVeryLLLLLongLabel;

    [SerializeField]
    private string m_string;

    [SerializeField]
    private Quaternion m_quat;

    [SerializeField]
    private GameObject m_object;

    [SerializeField]
    private List<float> floatList;

    [SerializeField]
    private List<GameObject> gameObjectsList;

    [SerializeField]
    private int m_intValue;

    [SerializeField]
    private float m_floatValue;

    [SerializeField]
    private CustomType1000 m_customType;

    [SerializeField]
    private Bounds m_bounds;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

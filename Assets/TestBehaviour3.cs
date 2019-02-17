using Battlehub.RTCommon;
using Battlehub.RTHandles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomType10002
{
   // public float[] m_array;// causes exception in PropertyEditor

    public List<float> m_list;

  //  public List<CustomType1000> m_customTypeList;

    public int intValue;
}


public class TestBehaviour3 : MonoBehaviour
{
   

    [SerializeField]
    private Component Component;

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
    private CustomType10002 m_customType;

    [SerializeField]
    private Bounds m_bounds;

    [SerializeField]
    private GameObject m_selectObj;

    [SerializeField]
    private GameObject m_selectObj2;


    private BoxSelection m_boxSelection;

    private void Start()
    {
        m_boxSelection = GetComponent<BoxSelection>();
        m_boxSelection.Filtering += OnFiltering;
    }

    private void OnDestroy()
    {
        if(m_boxSelection != null)
        {
            m_boxSelection.Filtering -= OnFiltering;
        }
    }

    private void OnFiltering(object sender, FilteringArgs e)
    {
        if (e.Object.name == "Capsule")
        {
            e.Cancel = true;
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            IRTE editor = IOC.Resolve<IRTE>();
            editor.Tools.Current = RuntimeTool.Rotate;
        }
    }
}

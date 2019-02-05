#Common Infrastructure Docs
##Overview
##Expose To Editor
##IOC
##Input
##IRTE interface
##RTE Component
##Runtime Selection
##Runtime Objects
##Runtime Tools

``` C#
 using UnityEditor;
 using Battlehub.RTCommon;
 
 public class SwitchToolBehaviour : MonoBehaviour
 {
	[SerializeField]
	private GameObject m_selectObj;

	void Start()
	{
        IRTE editor = IOC.Resolve<IRTE>();
        editor.Tools.Current = RuntimeTool.Move;
    }
 }
	
```

##Runtime Selection

``` C#
 using UnityEditor;
 using Battlehub.RTCommon;
 
 public class SelectObjectBehaviour : MonoBehaviour
 {
	[SerializeField]
	private GameObject m_selectObj;

	void Start()
	{
        IRTE editor = IOC.Resolve<IRTE>();
        editor.Selection.activeObject = m_selectObj;
    }
 }
	
```

##Runtime Undo
##Drag And Drop
##Runtime Window
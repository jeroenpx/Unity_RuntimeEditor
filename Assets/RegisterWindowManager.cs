using Battlehub.RTCommon;
using Battlehub.RTEditor;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class RegisterWindowManager : MonoBehaviour
{
    private WindowManager m_wm;
    private void Awake()
    {
        m_wm = GetComponent<WindowManager>();
        IOC.Register<IWindowManager>(m_wm);
        m_wm.OverrideDefaultLayout(wm => new Battlehub.UIControls.DockPanels.LayoutInfo() { Content = new GameObject().AddComponent<RectTransform>() });
    }

    private void OnDestroy()
    {
        IOC.Unregister<IWindowManager>(m_wm);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            
        }
    }
}

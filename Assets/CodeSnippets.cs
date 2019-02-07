using Battlehub.RTCommon;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor.Demo
{
    public class CodeSnippets : MonoBehaviour
    {
        IDragDrop m_dragDrop;
        RuntimeWindow m_window;
        
        void Start()
        {
            m_window = IOC.Resolve<IRTE>().GetWindow(RuntimeWindowType.Console);            
            m_dragDrop = IOC.Resolve<IRTE>().DragDrop;
            m_dragDrop.Drop += OnDrop;
        }

        private void OnDestroy()
        {
            m_dragDrop.Drop -= OnDrop;
        }

        private void OnDrop(PointerEventData pointerEventData)
        {
            if(m_window != null && m_window.IsPointerOver)
            {
                Debug.Log(m_dragDrop.DragObjects[0]);
            }
        }
    }
}
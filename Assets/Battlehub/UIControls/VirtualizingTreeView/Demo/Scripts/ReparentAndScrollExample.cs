using UnityEngine;

namespace Battlehub.UIControls.Demo
{
    public class ReparentAndScrollExample : MonoBehaviour
    {
        [SerializeField]
        private Transform m_parent = null;

        [SerializeField]
        private Transform m_obj = null;

        [SerializeField]
        private VirtualizingTreeView m_treeView = null;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_obj.SetParent(m_parent, true);
                m_treeView.ChangeParent(m_parent.gameObject, m_obj.gameObject);

                VirtualizingScrollRect scrollRect = m_treeView.GetComponentInChildren<VirtualizingScrollRect>();
                scrollRect.Index = m_treeView.IndexOf(m_parent.gameObject);
            }
        }
    }
}

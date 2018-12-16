using UnityEngine;
using Battlehub.RTCommon;
namespace Battlehub.RTHandles
{
    public class InstantiatePrefab : MonoBehaviour
    {
        public GameObject Prefab;

        private EditorDemo m_editor;
        private GameObject m_instance;
        private Plane m_dragPlane;
        private bool m_spawn;

        private bool GetPointOnDragPlane(out Vector3 point)
        {
            Ray ray = m_editor.EditorCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            float distance;
            if (m_dragPlane.Raycast(ray, out distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }
            point = Vector3.zero;
            return false;
        }

        public void Spawn()
        {
            m_editor = IOC.Resolve<EditorDemo>();
            if (m_editor == null)
            {
                Debug.LogError("Editor.Instance is null");
                return;
            }
            
            Vector3 point;
            m_dragPlane = new Plane(Vector3.up, m_editor.Pivot);
            if(GetPointOnDragPlane(out point))
            {
                //m_instance = Prefab.InstantiatePrefab(point, Quaternion.identity);
                enabled = true;
                m_spawn = true;
            }
            else
            {
                //m_instance = Prefab.InstantiatePrefab(m_editor.Pivot, Quaternion.identity);
            }

            ExposeToEditor exposeToEditor = m_instance.GetComponent<ExposeToEditor>();
            if (!exposeToEditor)
            {
                exposeToEditor = m_instance.AddComponent<ExposeToEditor>();
            }
            exposeToEditor.SetName(Prefab.name);
            m_instance.SetActive(true);
            
            m_editor.Undo.BeginRecord();
            m_editor.Undo.RecordSelection();
            m_editor.Undo.BeginRegisterCreateObject(m_instance);
            m_editor.Undo.EndRecord();

            bool isEnabled = m_editor.Undo.Enabled;
            m_editor.Undo.Enabled = false;
            m_editor.Selection.activeGameObject = m_instance;
            m_editor.Undo.Enabled = isEnabled;

            m_editor.Undo.BeginRecord();
            m_editor.Undo.RegisterCreatedObject(m_instance);
            m_editor.Undo.RecordSelection();
            m_editor.Undo.EndRecord();
        }

        private void Update()
        {
            if(!m_spawn)
            {
                return;
            }


            Vector3 point;
            if (GetPointOnDragPlane(out point))
            {
                if(m_editor.AutoUnitSnapping)
                {
                    point.x = Mathf.Round(point.x);
                    point.y = Mathf.Round(point.y);
                    point.z = Mathf.Round(point.z);
                }

                m_instance.transform.position = point;
            }


            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                enabled = false;
                m_spawn = false;
                m_instance = null;
            }
        }
    }

}

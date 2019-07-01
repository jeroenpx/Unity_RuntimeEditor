using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Battlehub.ProBuilderIntegration
{
    public class PBPolyShape : MonoBehaviour
    {
        private PBPolyShapeSelection m_selection;
        private ProBuilderMesh m_targetMesh;
        private PBMesh m_target;

        private bool m_isEditing;

        private List<Vector3> m_positions;
        public List<Vector3> Positions
        {
            get { return m_positions; }
            set { m_positions = value; }
        }
        
        private void Awake()
        {
            m_target = GetComponent<PBMesh>();
            if(!m_target)
            {
                m_target = gameObject.AddComponent<PBMesh>();
            }

            m_targetMesh = m_target.GetComponent<ProBuilderMesh>();

        }

        private void OnDestroy()
        {
            EndEdit();
        }

        public void BeginEdit()
        {
            m_selection = gameObject.GetComponent<PBPolyShapeSelection>();
            if(m_selection == null)
            {
                m_selection = gameObject.AddComponent<PBPolyShapeSelection>();
            }
            else
            {
                m_selection.Clear();
            }
            
            m_isEditing = true;
        }

        public void EndEdit()
        {
            Destroy(m_selection);
            m_isEditing = false;
        }

        public void AddVertexWorld(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);
            AddVertex(position);
        }

        public void AddVertex(Vector3 position)
        {
            if(!m_isEditing)
            {
                BeginEdit();
            }

            m_selection.Add(position);
        }
    }
}


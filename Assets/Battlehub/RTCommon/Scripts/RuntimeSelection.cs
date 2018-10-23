using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Battlehub.RTCommon
{
    public delegate void RuntimeSelectionChanged(Object[] unselectedObjects);
    public interface IRuntimeSelection
    {
        event RuntimeSelectionChanged SelectionChanged;
        bool Enabled
        {
            get;
            set;
        }
        GameObject activeGameObject
        {
            get;
            set;
        }
        Object activeObject
        {
            get;
            set;
        }
        Object[] objects
        {
            get;
            set;
        }

        GameObject[] gameObjects
        {
            get;
        }

        Transform activeTransform
        {
            get;
        }

        bool IsSelected(Object obj);

        void Select(Object activeObject, Object[] selection);
    }
   
    public interface IRuntimeSelectionInternal : IRuntimeSelection
    {
        Object INTERNAL_activeObjectProperty
        {
            get;
            set;
        }

        Object[] INTERNAL_objectsProperty
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Runtime selection (rough equivalent of UnityEditor.Selection class) 
    /// </summary>
    public class RuntimeSelection : IRuntimeSelectionInternal
    {
        public event RuntimeSelectionChanged SelectionChanged;

        public Object INTERNAL_activeObjectProperty
        {
            get { return m_activeObject; }
            set
            {
                m_activeObject = value;
            }
        }

        public Object[] INTERNAL_objectsProperty
        {
            get { return m_objects; }
            set
            {
                SetObjects(value);
            }
        }

        private bool m_isEnabled = true;
        public bool Enabled
        {
            get { return m_isEnabled; }
            set
            {
                m_isEnabled = value;
                if(!m_isEnabled)
                {
                    objects = null;
                }
            }
        }

        private HashSet<Object> m_selectionHS;

        protected void RaiseSelectionChanged(Object[] unselectedObjects)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(unselectedObjects);
            }
        }

        public GameObject activeGameObject
        {
            get { return activeObject as GameObject; }
            set { activeObject = value; }
        }

        protected Object m_activeObject;
        public Object activeObject
        {
            get { return m_activeObject; }
            set
            {
                if (m_activeObject != value || value != null && m_objects != null && m_objects.Length > 1)
                {
                    if(!m_isEnabled)
                    {
                        return;
                    }

                    m_editor.Undo.RecordSelection();
                    m_activeObject = value;
                    Object[] unselectedObjects = m_objects;
                    if (m_activeObject != null)
                    {
                        m_objects = new[] { value };
                    }
                    else
                    {
                        m_objects = new Object[0];
                    }
                    UpdateHS();
                    m_editor.Undo.RecordSelection();
                    RaiseSelectionChanged(unselectedObjects);
                }
            }
        }

        protected Object[] m_objects;
        public Object[] objects
        {
            get { return m_objects; }
            set
            {
                if (!m_isEnabled)
                {
                    return;
                }

                if (IsSelectionChanged(value))
                {
                    m_editor.Undo.RecordSelection();
                    SetObjects(value);
                    m_editor.Undo.RecordSelection();
                }
            }
        }

        private IRTE m_editor;
        public RuntimeSelection(IRTE rte)
        {
            m_editor = rte;
        }

        public bool IsSelected(Object obj)
        {
            if(m_selectionHS == null)
            {
                return false;
            }
            return m_selectionHS.Contains(obj);
        }

        private void UpdateHS()
        {
            if (m_objects != null)
            {
                m_selectionHS = new HashSet<Object>(m_objects);
            }
            else
            {
                m_selectionHS = null;
            }
        }

        private bool IsSelectionChanged(Object[] value)
        {
            if(m_objects == value)
            {
                return false;
            }

            if(m_objects == null)
            {
                return value.Length != 0;
            }

            if(value == null)
            {
                return m_objects.Length != 0;
            }

            if(m_objects.Length != value.Length)
            {
                return true;
            }

            for (int i = 0; i < m_objects.Length; ++i)
            {
                if (m_objects[i] != value[i])
                {
                    return true;
                }
            }

            return false;
        }

        protected void SetObjects(Object[] value)
        {
            if(!IsSelectionChanged(value))
            {
                return;
            }
            Object[] oldObjects = m_objects;
            if (value == null)
            {
                m_objects = null;
                m_activeObject = null;
            }
            else
            {
                m_objects = value.ToArray();
                if (m_activeObject == null || !m_objects.Contains(m_activeObject))
                {
                    m_activeObject = m_objects.OfType<Object>().FirstOrDefault();
                }
            }

            UpdateHS();
            RaiseSelectionChanged(oldObjects);
        }

        public GameObject[] gameObjects
        {
            get
            {
                if (m_objects == null)
                {
                    return null;
                }

                return m_objects.OfType<GameObject>().ToArray();
            }
        }

        public Transform activeTransform
        {
            get
            {
                if (m_activeObject == null)
                {
                    return null;
                }

                if (m_activeObject is GameObject)
                {
                    return ((GameObject)m_activeObject).transform;
                }
                return null;
            }
        }

        public void Select(Object activeObject, Object[] selection)
        {
            if(IsSelectionChanged(selection))
            {
                m_editor.Undo.RecordSelection();
                m_activeObject = activeObject;
                SetObjects(selection);
                m_editor.Undo.RecordSelection();
            }
        }
    }
}

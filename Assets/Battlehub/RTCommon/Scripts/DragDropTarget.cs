using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTCommon
{
    public interface IDragDropTarget
    {
        void BeginDrag(object[] dragObjects, PointerEventData eventData);
        void DragEnter(object[] dragObjects, PointerEventData eventData);
        void DragLeave(PointerEventData eventData);
        void Drag(object[] dragObjects, PointerEventData eventData);
        void Drop(object[] dragObjects, PointerEventData eventData);
    }

    public class DragDropTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragDropTarget
    {
        [SerializeField]
        public GameObject m_dragDropTargetGO;

        private IDragDropTarget[] m_dragDropTargets = new IDragDropTarget[0];

        // Use this for initialization
        private void Awake()
        {
            if (m_dragDropTargetGO == null)
            {
                m_dragDropTargets = new[] { this };
            }
            else
            {
                m_dragDropTargets = m_dragDropTargetGO.GetComponents<Component>().OfType<IDragDropTarget>().ToArray();
                if(m_dragDropTargets.Length == 0)
                {
                    Debug.LogWarning("dragDropTargetGO does not contains components with IDragDropTarget interface implemented");
                    m_dragDropTargets = new[] { this };
                }
            }
            AwakeOverride();
        }

        private void OnDestroy()
        {
            m_dragDropTargets = null;
            OnDestroyOverride();
        }

        protected virtual void AwakeOverride()
        {

        }
        
        protected virtual void OnDestroyOverride()
        {

        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnterOverride(eventData);
            if (DragDrop.DragObjects != null)
            {
                for(int i = 0; i < m_dragDropTargets.Length; ++i)
                {
                    m_dragDropTargets[i].DragEnter(DragDrop.DragObjects, eventData);
                }
            }

            DragDrop.BeginDrag += OnBeginDrag;
            DragDrop.Drag += OnDrag;
            DragDrop.Drop += OnDrop;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            OnPointerExitOverride(eventData);
            DragDrop.BeginDrag -= OnBeginDrag;
            DragDrop.Drop -= OnDrop;
            DragDrop.Drag -= OnDrag;
            if (DragDrop.DragObjects != null)
            {
                for (int i = 0; i < m_dragDropTargets.Length; ++i)
                {
                    m_dragDropTargets[i].DragLeave(eventData);
                }
            }
        }

        protected virtual void OnPointerEnterOverride(PointerEventData eventData)
        {

        }

        protected virtual void OnPointerExitOverride(PointerEventData eventData)
        {

        }

        private void OnBeginDrag(PointerEventData pointerEventData)
        {
            if (DragDrop.DragObjects != null)
            {
                for (int i = 0; i < m_dragDropTargets.Length; ++i)
                {
                    m_dragDropTargets[i].BeginDrag(DragDrop.DragObjects, pointerEventData);
                }
            }
        }


        private void OnDrag(PointerEventData pointerEventData)
        {
            if(DragDrop.DragObjects != null)
            {
                for (int i = 0; i < m_dragDropTargets.Length; ++i)
                {
                    m_dragDropTargets[i].Drag(DragDrop.DragObjects, pointerEventData);
                }
            }   
        }

        private void OnDrop(PointerEventData eventData)
        {
            DragDrop.BeginDrag -= OnBeginDrag;
            DragDrop.Drop -= OnDrop;
            DragDrop.Drag -= OnDrag;
            if (DragDrop.DragObjects != null)
            {
                for (int i = 0; i < m_dragDropTargets.Length; ++i)
                {
                    m_dragDropTargets[i].Drop(DragDrop.DragObjects, eventData);
                }
            }
        }

        public virtual void BeginDrag(object[] dragObjects, PointerEventData eventData)
        {

        }

        public virtual void DragEnter(object[] dragObjects, PointerEventData eventData)
        {    
        }

        public virtual void Drag(object[] dragObjects, PointerEventData eventData)
        {

        }

        public virtual void DragLeave(PointerEventData eventData)
        {
            
        }

        public virtual void Drop(object[] dragObjects, PointerEventData eventData)
        { 
        }

    }

}


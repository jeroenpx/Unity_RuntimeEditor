using Battlehub.Utils;
using UnityEngine.EventSystems;

namespace Battlehub.RTCommon
{
    public delegate void DragDropEventHander(PointerEventData pointerEventData);

    public static class DragDrop 
    {
        private static object m_cursorLocker = new object();

        public static event DragDropEventHander BeginDrag;
        public static event DragDropEventHander Drag;
        public static event DragDropEventHander Drop;

        public static object[] DragObjects
        {
            get;
            private set;
        }

        public static void Reset()
        {
            DragObjects = null;
        }

        public static object DragObject
        {
            get
            {
                if (DragObjects == null || DragObjects.Length == 0)
                {
                    return null;
                }

                return DragObjects[0];
            }
        }

        public static void SetCursor(KnownCursor cursorType)
        {
            CursorHelper.SetCursor(m_cursorLocker, cursorType);
        }

        public static void ResetCursor()
        {
            CursorHelper.ResetCursor(m_cursorLocker);
        }

        public static void RaiseBeginDrag(object[] dragItems, PointerEventData pointerEventData)
        {
            DragObjects = dragItems;
            SetCursor(KnownCursor.DropNowAllowed);
            if (BeginDrag != null)
            {
                BeginDrag(pointerEventData);
            }
        }

        public static void RaiseDrag(PointerEventData eventData)
        {
            if(DragObjects != null)
            {
                if (Drag != null)
                {
                    Drag(eventData);
                }
            }
        }

        public static void RaiseDrop(PointerEventData pointerEventData)
        {
            if(DragObjects != null)
            {
                if (Drop != null)
                {
                    Drop(pointerEventData);
                }

                ResetCursor();
                DragObjects = null;
            }
        }
    }

}

#define USE_RTE
#if USE_RTE
using Battlehub.RTCommon;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor
{
    public class TimelineControlInput : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private TimelineControl m_timelineControl;
        #if USE_RTE
        private IRTE m_editor;
        private RuntimeWindow m_window;
        #endif

        private bool m_isPointerOver;

        private void Start()
        {
            #if USE_RTE
            m_editor = IOC.Resolve<IRTE>();
            m_window = GetComponentInParent<RuntimeWindow>();
            #endif

            m_timelineControl = GetComponent<TimelineControl>();
        }

        private void Update()
        {
            #if USE_RTE
            if(m_window != m_editor.ActiveWindow)
            {
                return;
            }
            #endif

            m_timelineControl.MultiselectMode = MultiselectAction();

            if (DeleteAction())
            {
                m_timelineControl.DeleteSelectedKeyframes();
            }

            Vector2 delta = GetIntervalDelta();
            if(delta != Vector2.zero)
            {
                m_timelineControl.ChangeInterval(delta);
            }

            if(Input.GetKeyDown(KeyCode.K))
            {
                for(int i = 0; i < m_timelineControl.VisibleRowsCount - 1; ++i)
                {
                    m_timelineControl.AddKeyframe(m_timelineControl.CurrentSample, i);
                }
            }
        }

        protected virtual bool MultiselectAction()
        {
            #if USE_RTE
            return m_editor.Input.GetKey(KeyCode.LeftShift);
            #else
            return Input.GetKey(KeyCode.LeftShift);
            #endif
        }

        protected virtual bool DeleteAction()
        {
            #if USE_RTE
            return m_editor.Input.GetKeyDown(KeyCode.Delete);
            #else
            return Input.GetKeyDown(KeyCode.Delete);
            #endif
        }

        protected virtual Vector2 GetIntervalDelta()
        {
            if(!m_isPointerOver)
            {
                return Vector2.zero;
            }

            #if USE_RTE
            if(!m_window.IsPointerOver)
            {
                return Vector2.zero;
            }

            float delta = m_editor.Input.GetAxis(InputAxis.Z);
            if(m_editor.Input.GetKeyDown(KeyCode.LeftAlt))
            {
                return new Vector2(0, delta);
            }
            return new Vector2(delta, 0);
            #else
            float delta = Input.GetAxis("Mouse ScrollWheel");
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                return new Vector2(0, delta);
            }
            return new Vector2(delta, 0);
            #endif
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_isPointerOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_isPointerOver = false;
        }
    }
}


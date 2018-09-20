using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTCommon
{
    public enum RuntimeWindowType
    {
        None,
        GameView,
        SceneView,
        Hierarchy,
        ProjectTree,
        Resources,
        Inspector,
        Other
    }

    public class RuntimeEditorWindow : DragDropTarget, IPointerDownHandler, IPointerUpHandler
    {
        public RuntimeWindowType WindowType;

        private bool m_isPointerOver;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            RuntimeEditorApplication.AddWindow(this);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
        
            RuntimeEditorApplication.ActivateWindow(null);
            RuntimeEditorApplication.PointerExit(this);
            RuntimeEditorApplication.RemoveWindow(this);
        }

        private void Update()
        {
            if (WindowType == RuntimeWindowType.GameView)
            {
                if (RuntimeEditorApplication.GameCameras == null || RuntimeEditorApplication.GameCameras.Length == 0)
                {
                    return;
                }

                Rect cameraRect = RuntimeEditorApplication.GameCameras[0].pixelRect;
                UpdateState(cameraRect, true);
            }
            else if (WindowType == RuntimeWindowType.SceneView)
            {
                if (RuntimeEditorApplication.ActiveSceneCamera == null)
                {
                    if (Camera.main != null)
                    {
                        RuntimeEditorApplication.SceneCameras = new[] { Camera.main };
                    }
                    else
                    {
                        return;
                    }
                }

                Rect cameraRect = RuntimeEditorApplication.ActiveSceneCamera.pixelRect;
                UpdateState(cameraRect, false);
            }
            else if (WindowType == RuntimeWindowType.None)
            {
                if(Camera.main == null)
                {
                    return;
                }

                Rect cameraRect = Camera.main.pixelRect;
                UpdateState(cameraRect, false);
            }
            else if(WindowType == RuntimeWindowType.Other)
            {
                return;
            }
            else
            {
                if (m_isPointerOver)
                {
                    if (InputController._GetMouseButtonUp(0) ||
                        InputController._GetMouseButtonUp(1) ||
                        InputController._GetMouseButtonUp(2) ||
                        InputController._GetMouseButtonDown(0) || 
                        InputController._GetMouseButtonDown(1) || 
                        InputController._GetMouseButtonDown(2))
                    {
                        RuntimeEditorApplication.ActivateWindow(this);
                    }
                }
            }

            UpdateOverride();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (WindowType == RuntimeWindowType.SceneView || WindowType == RuntimeWindowType.GameView)
            {
                return;
            }

            RuntimeEditorApplication.ActivateWindow(this);
            OnPointerDownOverride(eventData);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpOverride(eventData);
        }
    

        protected override void OnPointerEnterOverride(PointerEventData eventData)
        {
            base.OnPointerEnterOverride(eventData);
            if (WindowType == RuntimeWindowType.SceneView || WindowType == RuntimeWindowType.GameView)
            {
                return;
            }

            m_isPointerOver = true;
            RuntimeEditorApplication.PointerEnter(this);
        }

        protected override void OnPointerExitOverride(PointerEventData eventData)
        {
            base.OnPointerExitOverride(eventData);
            if (WindowType == RuntimeWindowType.SceneView || WindowType == RuntimeWindowType.GameView)
            {
                return;
            }
            m_isPointerOver = false;
            RuntimeEditorApplication.PointerExit(this);
        }

        protected virtual void OnPointerDownOverride(PointerEventData eventData)
        {

        }

        protected virtual void OnPointerUpOverride(PointerEventData eventData)
        {

        }

        protected virtual void UpdateOverride()
        {

        }
      
        private void UpdateState(Rect cameraRect, bool isGameView)
        {
            bool isPointerOver = cameraRect.Contains(InputController._MousePosition) && !RuntimeTools.IsPointerOverGameObject();
            if (RuntimeEditorApplication.IsPointerOverWindow(this))
            {
                if (!isPointerOver)
                {
                    RuntimeEditorApplication.PointerExit(this);
                }
            }
            else
            {
                if (isPointerOver)
                {
                    RuntimeEditorApplication.PointerEnter(this);
                }
            }

            if (isPointerOver)
            {
                if (InputController._GetMouseButtonUp(0) ||
                    InputController._GetMouseButtonUp(1) ||
                    InputController._GetMouseButtonUp(2) ||
                    InputController._GetMouseButtonDown(0) ||
                    InputController._GetMouseButtonDown(1) ||
                    InputController._GetMouseButtonDown(2))
                {
                    if (!isGameView || isGameView && RuntimeEditorApplication.IsPlaying)
                    {
                        RuntimeEditorApplication.ActivateWindow(this);
                    }
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }
    }

}

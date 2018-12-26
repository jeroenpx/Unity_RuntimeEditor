using Battlehub.RTCommon;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class GameView : RuntimeWindow
    {
        [SerializeField]
        private GameObject m_noCamerasRenderingTxt = null;

        private List<GameViewCamera> m_gameCameras;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Game;
            m_gameCameras = Editor.Object.Get(false).Select(obj => obj.GetComponent<GameViewCamera>()).Where(obj => obj != null && obj.IsAwaked).ToList();
            if(m_gameCameras.Count > 0)
            {
                m_camera = m_gameCameras[0].Camera;
            }
            UpdateVisualState();

            GameViewCamera._Awaked += OnCameraAwaked;
            GameViewCamera._Destroyed += OnCameraDestroyed;
            GameViewCamera._Enabled += OnCameraEnabled;
            GameViewCamera._Disabled += OnCameraDisabled;
            GameViewCamera._CameraEnabled += OnCameraComponentEnabled;
            GameViewCamera._CameraDisabled += OnCameraComponentDisabled;

            base.AwakeOverride();
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            GameViewCamera._Awaked -= OnCameraAwaked;
            GameViewCamera._Destroyed -= OnCameraDestroyed;
            GameViewCamera._Enabled -= OnCameraEnabled;
            GameViewCamera._Disabled -= OnCameraDisabled;
            GameViewCamera._CameraEnabled -= OnCameraComponentEnabled;
            GameViewCamera._CameraDisabled -= OnCameraComponentDisabled;

            for (int i = 0; i < m_gameCameras.Count; ++i)
            {
                GameViewCamera gameCamera = m_gameCameras[i];
                if(gameCamera != null)
                {
                    gameCamera.Camera.depth = gameCamera.Depth;
                    gameCamera.Camera.rect = gameCamera.Rect;
                }
            }
        }

        public override void SetCameraDepth(int depth)
        {
            base.SetCameraDepth(depth);
            for(int i = 0; i < m_gameCameras.Count; ++i)
            {
                GameViewCamera gameCamera = m_gameCameras[i];
                gameCamera.Camera.depth = depth + gameCamera.Depth;
            }
        }

        private void OnCameraAwaked(GameViewCamera gameCamera)
        {
            gameCamera.Camera.depth = CameraDepth + gameCamera.Depth;
            m_gameCameras.Add(gameCamera);
            if (Camera == null)
            {
                Camera = gameCamera.Camera;
            }
            UpdateVisualState();
            if(Editor.IsOpened && gameCamera.Camera != null)
            {
                SetCullingMask(gameCamera.Camera);
                HandleResize();
            }
        }

        private void OnCameraEnabled(GameViewCamera gameCamera)
        {
            UpdateVisualState();
        }

        private void OnCameraDisabled(GameViewCamera camera)
        {
            UpdateVisualState();
        }

        private void OnCameraComponentEnabled(GameViewCamera camera)
        {
            UpdateVisualState();
        }

        private void OnCameraComponentDisabled(GameViewCamera camera)
        {
            UpdateVisualState();
        }

        private void OnCameraDestroyed(GameViewCamera camera)
        {
            m_gameCameras.Remove(camera);
            if (m_gameCameras.Count > 0)
            {
                Camera = m_gameCameras[0].Camera;
            }
            else
            {
                Camera = null;
            }
            UpdateVisualState();
        }

        protected override void SetCullingMask()
        {
            for(int i = 0; i < m_gameCameras.Count; ++i)
            {
                SetCullingMask(m_gameCameras[i].Camera);
            }
        }

        protected override void ResetCullingMask()
        {
            for (int i = 0; i < m_gameCameras.Count; ++i)
            {
                ResetCullingMask(m_gameCameras[i].Camera);
            }
        }

        protected override void ResizeCamera(Rect pixelRect)
        {
            for(int i = 0; i < m_gameCameras.Count; ++i)
            {
                GameViewCamera gameCamera = m_gameCameras[i];
                Rect r = gameCamera.Rect;
                gameCamera.Camera.pixelRect = new Rect(pixelRect.x + r.x * pixelRect.width, pixelRect.y + r.y * pixelRect.height, r.width * pixelRect.width, r.height * pixelRect.height);
            }
        }

        private void UpdateVisualState()
        {
            m_noCamerasRenderingTxt.SetActive(m_gameCameras.Count == 0 || m_gameCameras.All(c => !c.gameObject.activeSelf || !c.Camera || !c.Camera.enabled));
        }
    }

}


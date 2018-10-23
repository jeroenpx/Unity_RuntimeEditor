using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace Battlehub.RTCommon
{

    public class Game : MonoBehaviour
    {
        public Button BtnRestart;
        private ExposeToEditor[] m_editorObjects;
        private ExposeToEditor[] m_enabledEditorObjects;
        private Object[] m_editorSelection;
        private bool m_applicationQuit;
        private IRTE m_editor;

        protected IRTE Editor
        {
            get { return m_editor; }
        }

        private void Awake()
        {
            m_editor = RTE.Get;
            if(m_editor == null)
            {
                Debug.LogError("editor is null");
                return;
            }

            if (BtnRestart != null)
            {
                BtnRestart.onClick.AddListener(RestartGame);
            }

            m_editor.ActiveWindowChanged += OnActiveWindowChanged;
            StartGame();

            AwakeOverride();
        }

        private void Start()
        {
            StartOverride();
        }

        private void OnDestroy()
        {
            if (m_applicationQuit)
            {
                return;
            }

            OnDestroyOverride();
            DestroyGame();
            if (BtnRestart != null)
            {
                BtnRestart.onClick.RemoveListener(RestartGame);
            }
            m_editor.ActiveWindowChanged -= OnActiveWindowChanged;
        }

        private void OnApplicationQuit()
        {
            m_applicationQuit = true;
        }

        private void RestartGame()
        {
            m_editor.IsPlaying = false;
            m_editor.IsPlaying = true;
        }

        private void StartGame()
        {
            DestroyGame();

            m_editorObjects = ExposeToEditor.FindAll(m_editor, ExposeToEditorObjectType.EditorMode, true).Select(go => go.GetComponent<ExposeToEditor>()).OrderBy(exp => exp.transform.GetSiblingIndex()).ToArray();
            m_enabledEditorObjects = m_editorObjects.Where(eo => eo.gameObject.activeSelf).ToArray();
            m_editorSelection = m_editor.Selection.objects;

            HashSet<GameObject> selectionHS = new HashSet<GameObject>(m_editor.Selection.gameObjects != null ? m_editor.Selection.gameObjects : new GameObject[0]);
            List<GameObject> playmodeSelection = new List<GameObject>();
            for (int i = 0; i < m_editorObjects.Length; ++i)
            {
                ExposeToEditor editorObj = m_editorObjects[i];
                if (editorObj.Parent != null)
                {
                    continue;
                }

                GameObject playmodeObj = Instantiate(editorObj.gameObject, editorObj.transform.position, editorObj.transform.rotation);

                ExposeToEditor playModeObjExp = playmodeObj.GetComponent<ExposeToEditor>();
                playModeObjExp.ObjectType = ExposeToEditorObjectType.PlayMode;
                playModeObjExp.SetName(editorObj.name);
                playModeObjExp.Init();

                ExposeToEditor[] editorObjAndChildren = editorObj.GetComponentsInChildren<ExposeToEditor>(true);
                ExposeToEditor[] playModeObjAndChildren = playmodeObj.GetComponentsInChildren<ExposeToEditor>(true);
                for (int j = 0; j < editorObjAndChildren.Length; j++)
                {
                    if (selectionHS.Contains(editorObjAndChildren[j].gameObject))
                    {
                        playmodeSelection.Add(playModeObjAndChildren[j].gameObject);
                    }
                }

                editorObj.gameObject.SetActive(false);
            }

            bool isEnabled = m_editor.Undo.Enabled;
            m_editor.Undo.Enabled = false;
            m_editor.Selection.objects = playmodeSelection.ToArray();
            m_editor.Undo.Enabled = isEnabled;
            m_editor.Undo.Store();
        }

        private void DestroyGame()
        {
            if (m_editorObjects == null)
            {
                return;
            }

            OnDestoryGameOverride();
            
            ExposeToEditor[] playObjects = ExposeToEditor.FindAll(m_editor, ExposeToEditorObjectType.PlayMode, true).Select(go => go.GetComponent<ExposeToEditor>()).ToArray();
            for (int i = 0; i < playObjects.Length; ++i)
            {
                ExposeToEditor playObj = playObjects[i];
                if(playObj != null)
                {
                    DestroyImmediate(playObj.gameObject);
                }
            }

            for (int i = 0; i < m_enabledEditorObjects.Length; ++i)
            {
                ExposeToEditor editorObj = m_enabledEditorObjects[i];
                if (editorObj != null)
                {
                    editorObj.gameObject.SetActive(true);
                }
            }


            bool isEnabled = m_editor.Undo.Enabled;
            m_editor.Undo.Enabled = false;
            m_editor.Selection.objects = m_editorSelection;
            m_editor.Undo.Enabled = isEnabled;
            m_editor.Undo.Restore();

            m_editorObjects = null;
            m_enabledEditorObjects = null;
            m_editorSelection = null;
        }

        protected virtual void OnActiveWindowChanged()
        {

        }

        protected virtual void AwakeOverride()
        {

        }

        protected virtual void StartOverride()
        {

        }

        protected virtual void OnDestroyOverride()
        {

        }

        protected virtual void OnDestoryGameOverride()
        {

        }


    }
}


using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using UnityEngine.UI;
using System.IO;

namespace Battlehub.RTSL
{
    public class RTSLDemo : MonoBehaviour
    {
        [SerializeField]
        private Button m_btnSave = null;

        [SerializeField]
        private Button m_btnLoad = null;

        private IProject m_project;

        [SerializeField]
        private string m_scenePath = "Scenes/Demo/MyScene";

        [SerializeField]
        private string m_projectName = "My Project";

        IEnumerator Start()
        {
            m_project = IOC.Resolve<IProject>();

            yield return m_project.OpenProject(m_projectName);
            yield return m_project.CreateFolder(Path.GetDirectoryName(m_scenePath).Replace(@"\", "/"));   
            
            if(m_btnSave != null)
            {
                m_btnSave.onClick.AddListener(OnSaveClick);
            }

            if(m_btnLoad != null)
            {
                m_btnLoad.interactable = m_project.Exist<Scene>(m_scenePath);
                m_btnLoad.onClick.AddListener(OnLoadClick);
            }
        }

        private void OnDestroy()
        {
            if (m_btnSave != null)
            {
                m_btnSave.onClick.RemoveListener(OnSaveClick);
            }

            if (m_btnLoad != null)
            {
                m_btnLoad.onClick.RemoveListener(OnLoadClick);
            }

            StopAllCoroutines();
        }

        private void OnSaveClick()
        {
            StartCoroutine(SaveScene());
        }

        private void OnLoadClick()
        {
            if (m_project.Exist<Scene>(m_scenePath))
            {
                StartCoroutine(LoadScene());
            }
        }

        IEnumerator SaveScene()
        {
            ProjectAsyncOperation ao = m_project.Save(m_scenePath, SceneManager.GetActiveScene());
            yield return ao;

            if (ao.Error.HasError)
            {
                Debug.LogError(ao.Error.ToString());
            }
            else
            {
                if(m_btnLoad != null)
                {
                    m_btnLoad.interactable = m_project.Exist<Scene>(m_scenePath);
                }
                
            }
        }

        IEnumerator LoadScene()
        {
            ProjectAsyncOperation ao = m_project.Load<Scene>(m_scenePath);
            yield return ao;

            if (ao.Error.HasError)
            {
                Debug.LogError(ao.Error.ToString());
            }
        }
    }
}



using UnityEngine;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using Battlehub.RTSL.Interface;
using Battlehub.RTCommon;
using UnityEngine.SceneManagement;
using System.Collections;
using Battlehub.RTScripting;

namespace Battlehub.RTEditor.Demo
{
    public class InitializeSurvialGameDemo : EditorOverride
    {
        protected override void Awake()
        {
            if(!PlayerPrefs.HasKey("InitializeSurvialGameDemo"))
            {
                PlayerPrefs.SetInt("InitializeSurvialGameDemo", 1);
                TextAsset textAsset = Resources.Load("SurvivalGameProject", typeof(TextAsset)) as TextAsset;
    
                using (Stream stream = new MemoryStream(textAsset.bytes))
                {
                    string path = Application.persistentDataPath + "/SurvivalGameProject";
                    FastZip zip = new FastZip();
                    zip.ExtractZip(stream, path, FastZip.Overwrite.Always, arg => true, null, null, true, true);
                }
            }
            base.Awake();
        }

        protected override void OnEditorExist()
        {
            base.OnEditorExist();
            StartCoroutine(CoLoad());
        }

        private IEnumerator CoLoad()
        {
            IProject project = IOC.Resolve<IProject>();
            yield return project.OpenProject("SurvivalGameProject");
            yield return new WaitUntil(() => project.IsOpened);

            IRuntimeScriptManager scriptManager = IOC.Resolve<IRuntimeScriptManager>();
            yield return new WaitUntil(() => scriptManager.IsLoaded);
            
            IRTE editor = IOC.Resolve<IRTE>();
            if(project.Exist<Scene>("Game"))
            {
                editor.IsBusy = true;
                yield return project.Load<Scene>("Game");
                editor.IsBusy = false;
            }
        }


    }

}

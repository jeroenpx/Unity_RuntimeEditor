using Battlehub.CodeAnalysis;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTSL.Interface;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using System;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTScripting
{
    public interface IRuntimeScriptManager
    {
        event Action Loading;
        event Action Loaded;

        string Ext
        {
            get;
        }

        void CreateScript(ProjectItem folder);
        ProjectAsyncOperation<RuntimeTextAsset> LoadScript(AssetItem assetItem);
        ProjectAsyncOperation SaveScript(AssetItem assetItem, RuntimeTextAsset script);
        ProjectAsyncOperation Compile();
    }

    [DefaultExecutionOrder(-1)]
    public class RuntimeScriptsManager : MonoBehaviour, IRuntimeScriptManager
    {
        public event Action Loading;
        public event Action Loaded;

        private static object m_syncRoot = new object();
        private const string RuntimeAssemblyName = "RuntimeAssembly";
        public string Ext
        {
            get { return ".cs"; }
        }

        private IProject m_project;
        private IEditorsMap m_map;
        private IRTE m_editor;
        private Assembly m_runtimeAssembly;

        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_project = IOC.Resolve<IProject>();
            m_map = IOC.Resolve<IEditorsMap>();
            IOC.RegisterFallback<IRuntimeScriptManager>(this);
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => m_project.IsOpened);
            yield return new WaitUntil(() => m_editor.IsBusy);

            if(Loading != null)
            {
                Loading();
            }

            ProjectAsyncOperation<RuntimeBinaryAsset> getValueAo = m_project.GetValue<RuntimeBinaryAsset>(RuntimeAssemblyName);
            yield return getValueAo;
            if (getValueAo.HasError)
            {
                if (getValueAo.Error.ErrorCode != Error.E_NotFound)
                {
                    Debug.LogError(getValueAo.Error);
                }
            }
            else
            {
                LoadAssembly(getValueAo.Result.Data);
            }
            
            if(Loaded != null)
            {
                Loaded();
            }
        }

        private void OnDestroy()
        {
            IOC.UnregisterFallback<IRuntimeScriptManager>(this);
            UnloadTypes(m_map);
        }

        public void CreateScript(ProjectItem folder)
        {
            string name = m_project.GetUniqueName("Script", Ext, folder, true);

            string nl = Environment.NewLine;
            RuntimeTextAsset csFile = ScriptableObject.CreateInstance<RuntimeTextAsset>();
            csFile.name = name;
            csFile.Ext = Ext;
            csFile.Text =
                "using System.Collections;" + nl +
                "using System.Collections.Generic;" + nl +
                "using UnityEngine;" + nl + nl +

                "public class " + name + " : MonoBehaviour" + nl +
                "{" + nl +
                "    // Start is called before the first frame update" + nl +
                "    void Start()" + nl +
                "    {" + nl +
                "    }" + nl + nl +

                "    // Update is called once per frame" + nl +
                "    void Update()" + nl +
                "    {" + nl +
                "    }" + nl +
                "}";

            IProjectFolder projectFolder = IOC.Resolve<IProjectFolder>();
            projectFolder.CreateAsset(csFile, folder);
        }

        public ProjectAsyncOperation<RuntimeTextAsset> LoadScript(AssetItem assetItem)
        {
            ProjectAsyncOperation<RuntimeTextAsset> ao = new ProjectAsyncOperation<RuntimeTextAsset>();
            StartCoroutine(CoLoadScript(assetItem, ao));
            return ao;
        }

        private IEnumerator CoLoadScript(AssetItem assetItem, ProjectAsyncOperation<RuntimeTextAsset> ao)
        {
            ProjectAsyncOperation<UnityObject[]> loadAo = m_project.Load(new[] { assetItem });
            yield return loadAo;

            ao.Error = loadAo.Error;
            if(!ao.HasError)
            {
                ao.Result = (RuntimeTextAsset)loadAo.Result[0];
            }
            ao.IsCompleted = true;
        }

        public ProjectAsyncOperation SaveScript(AssetItem assetItem, RuntimeTextAsset script)
        {
            return m_project.Save(new[] { assetItem }, new[] { script });
        }

        public ProjectAsyncOperation Compile()
        {
            ProjectAsyncOperation ao = new ProjectAsyncOperation();
            StartCoroutine(CoCompile(ao));
            return ao;
        }

        private IEnumerator CoCompile(ProjectAsyncOperation ao)
        {
            AssetItem[] assetItems = m_project.FindAssetItems(null, true, typeof(RuntimeTextAsset)).Where(assetItem => assetItem.Ext == Ext).ToArray();
            ProjectAsyncOperation<UnityObject[]> loadAo = m_project.Load(assetItems);
            yield return loadAo;
            if (loadAo.HasError)
            {
                ao.Error = loadAo.Error;
                ao.IsCompleted = true;
                yield break;
            }

            RunCompilerAsync(loadAo.Result.OfType<RuntimeTextAsset>().Select(s => s.Text).ToArray(), ao);
        }

        public async void RunCompilerAsync(string[] scripts, ProjectAsyncOperation ao)
        {
            ICompiler compiler = IOC.Resolve<ICompiler>();
            try
            {
                byte[] binData = await Task.Run(() => compiler.Compile(scripts));
                if(binData == null)
                {
                    ao.Error = new Error(Error.E_Failed) { ErrorText = "Compilation failed" };
                    ao.IsCompleted = true;
                }
                else
                {
                    StartCoroutine(CoSaveAssembly(binData, ao));
                }
            }
            catch(Exception e)
            {
                ao.Error = new Error(Error.E_Exception)
                {
                    ErrorText = e.ToString()
                };
                ao.IsCompleted = true;
            }
        }

        private IEnumerator CoSaveAssembly(byte[] binData, ProjectAsyncOperation ao)
        {
            RuntimeBinaryAsset asmBinaryData = ScriptableObject.CreateInstance<RuntimeBinaryAsset>();
            asmBinaryData.Data = binData;
            
            ProjectAsyncOperation setValueAo = m_project.SetValue(RuntimeAssemblyName, asmBinaryData);
            yield return setValueAo;
            if(setValueAo.HasError)
            {
                ao.Error = setValueAo.Error;
                ao.IsCompleted = true;
                yield break;
            }
            
            LoadAssembly(binData);
            ao.Error = Error.NoError;
            ao.IsCompleted = true;
        }

        private void LoadAssembly(byte[] binData)
        {
            UnloadTypes(m_map);

            m_runtimeAssembly = Assembly.Load(binData);
            Type[] loadedTypes = m_runtimeAssembly.GetTypes().Where(t => typeof(MonoBehaviour).IsAssignableFrom(typeof(MonoBehaviour))).ToArray();
            foreach (Type type in loadedTypes)
            {
                m_map.AddMapping(type, typeof(ComponentEditor), true, false);
            }
        }

        private void UnloadTypes(IEditorsMap map)
        {
            if (m_runtimeAssembly != null)
            {
                Type[] unloadedTypes = m_runtimeAssembly.GetTypes().Where(t => typeof(MonoBehaviour).IsAssignableFrom(typeof(MonoBehaviour))).ToArray();
                foreach (Type type in unloadedTypes)
                {
                    map.RemoveMapping(type);
                }
            }
        }
    }

}

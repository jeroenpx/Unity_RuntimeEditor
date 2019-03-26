using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public interface IFileImporter
    {
        string FileExt
        {
            get;
        }

        string IconPath
        {
            get;
        }

        IEnumerator Import(string filePath, string targetPath);
    }

    public class ImportFileDialog : RuntimeWindow
    {
        private Dialog m_parentDialog;
        private FileBrowser m_fileBrowser;

        private Dictionary<string, IFileImporter> m_extToFileImporter = new Dictionary<string, IFileImporter>();

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.ImportFile;
            base.AwakeOverride();
        }

        private void Start()
        {
            List<Assembly> assemblies = new List<Assembly>();
            foreach (string assemblyName in BHPath.RootAssemblies)
            {
                var asName = new AssemblyName();
                asName.Name = assemblyName;
                Assembly asm = Assembly.Load(asName);
                if (asm == null)
                {
                    Debug.LogWarning("Unable to load " + assemblyName);
                    continue;
                }
                assemblies.Add(asm);
            }

            m_fileBrowser = GetComponent<FileBrowser>();
            m_fileBrowser.DoubleClick += OnFileBrowserDoubleClick;
            List<string> allowedExts = new List<string>();
            List<FileIcon> icons = new List<FileIcon>();

            Type[] importerTypes = assemblies.SelectMany(asm => asm.GetTypes().Where(t => t != null && t.IsClass && typeof(IFileImporter).IsAssignableFrom(t))).ToArray();
            foreach (Type importerType in importerTypes)
            {
                try
                {
                    IFileImporter fileImporter = (IFileImporter)Activator.CreateInstance(importerType);

                    string ext = fileImporter.FileExt;
                    if(!ext.StartsWith("."))
                    {
                        ext = "." + ext;
                    }

                    if (m_extToFileImporter.ContainsKey(ext))
                    {
                        Debug.LogWarning("Importer for " + ext + " already exist");
                        continue;
                    }
                    m_extToFileImporter.Add(ext, fileImporter);

                    allowedExts.Add(ext);
                    icons.Add(new FileIcon { Ext = ext, Icon = Resources.Load<Sprite>(fileImporter.IconPath) });
                }
                catch (Exception e)
                {
                    Debug.LogError("Unable to instantiate File Importer " + e.ToString());
                }
            }

            m_fileBrowser.AllowedExt = allowedExts;
            m_fileBrowser.Icons = icons;

            m_parentDialog = GetComponentInParent<Dialog>();
            m_parentDialog.Ok += OnOk;
            m_parentDialog.OkText = "Open";
            m_parentDialog.IsOkVisible = true;
            m_parentDialog.CancelText = "Cancel";
            m_parentDialog.IsCancelVisible = true;
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
            }

            if(m_fileBrowser != null)
            {
                m_fileBrowser.DoubleClick -= OnFileBrowserDoubleClick;
            }
        }

        private void OnOk(Dialog sender, DialogCancelArgs args)
        {
            string path = m_fileBrowser.Open();
            if(string.IsNullOrEmpty(path))
            {
                args.Cancel = true;
                return;
            }

            if (!File.Exists(path))
            {
                args.Cancel = true;
                return;
            }

            TryImport(path);
        }

        private void OnFileBrowserDoubleClick(string path)
        {
            if(File.Exists(path))
            {
                m_parentDialog.Close();
                TryImport(path);
            }
        }

        private void TryImport(string path)
        {
            string ext = Path.GetExtension(path);

            IFileImporter importer;
            if(!m_extToFileImporter.TryGetValue(ext, out importer))
            {
                Debug.LogWarning("Importer for " + ext + " does not exists");
                return;
            }

            StartCoroutine(CoImport(importer, path));
        }

        private IEnumerator CoImport(IFileImporter importer, string path)
        {
            IRTE rte = IOC.Resolve<IRTE>();
            rte.IsBusy = true;

            IProjectTree projectTree = IOC.Resolve<IProjectTree>();
            string targetPath = Path.GetFileNameWithoutExtension(path);
            if (projectTree != null && projectTree.SelectedFolder != null)
            {
                ProjectItem folder = projectTree.SelectedFolder;

                targetPath = folder.RelativePath(false) + "/" + targetPath;
                targetPath = targetPath.TrimStart('/');

                IProject project = IOC.Resolve<IProject>();
               
                targetPath = project.GetUniquePath(targetPath, typeof(Texture2D), folder);    
            }

            yield return importer.Import(path, targetPath);
            rte.IsBusy = false;
        }

        
    }
}

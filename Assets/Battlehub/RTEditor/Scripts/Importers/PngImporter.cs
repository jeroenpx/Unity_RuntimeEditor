using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class PngImporter : IFileImporter
    {
        public string FileExt
        {
            get { return ".png"; }
        }

        public string IconPath
        {
            get { return "Importers/Png"; }
        }

        public IEnumerator Import(string filePath, string targetPath)
        {
            byte[] bytes = File.ReadAllBytes(filePath);

            Texture2D texture = new Texture2D(4, 4);
            if(texture.LoadImage(bytes, false))
            {
                IProject project = IOC.Resolve<IProject>();
                IResourcePreviewUtility previewUtility = IOC.Resolve<IResourcePreviewUtility>();
                byte[] preview = previewUtility.CreatePreviewData(texture); 
                yield return project.Save(targetPath, texture, preview);
            }
            else
            {
                Debug.LogError("Unable to load image " + filePath);
            }
            
            Object.Destroy(texture);
        }
    }
}

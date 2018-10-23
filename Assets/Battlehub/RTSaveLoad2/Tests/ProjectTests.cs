using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

using Battlehub.RTSaveLoad2.Interface;
namespace Battlehub.RTSaveLoad2
{
    public class ProjectTests
    {
        [TearDown]
        public void Cleanup()
        {
            Object.DestroyImmediate(Object.FindObjectOfType<RTSL2InternalDeps>().gameObject);
            Object.DestroyImmediate(Object.FindObjectOfType<Project>().gameObject);
        }
     
        [UnityTest]
        public IEnumerator OpenProjectTest()
        {
            GameObject projectGo = new GameObject();
            projectGo.AddComponent<Project>();
            
            IProject project = RTSL2InternalDeps.Get.Project;
            Assert.IsNull(project.Root);

            bool done = false;
            project.Open("TestProject", error =>
            {
                Assert.IsFalse(error.HasError);
                Assert.IsNotNull(project.Root);
                done = true;
            });

            while(!done)
            {
                yield return null;
            }

            
        }

        [Test]
        public void SaveTest1()
        {
            GameObject projectGo = new GameObject();
            projectGo.AddComponent<Project>();

            IProject project = RTSL2InternalDeps.Get.Project;
            Assert.Throws<System.InvalidOperationException>(() =>
            {
                project.Save(null, null, null);
            }, 
            "Project is not opened. Use OpenProject method");
        }

        [Test]
        public void SaveTest2()
        {
            GameObject projectGo = new GameObject();
            projectGo.AddComponent<Project>();

            IProject project = RTSL2InternalDeps.Get.Project;
            Assert.Throws<System.InvalidOperationException>(() =>
            {
                project.Save(null, null, null, null);
            },
            "Project is not opened. Use OpenProject method");
        }


        [UnityTest]
        public IEnumerator SaveLoadTest([Values(false, true)]bool unload)
        {
            GameObject projectGo = new GameObject();
            projectGo.AddComponent<Project>();

            byte[] dummyPreview = new byte[] { 0x1, 0x2, 0x3 };
            GameObject dummyGo = new GameObject();
            dummyGo.transform.position = new Vector3(1, 2, 3);
            dummyGo.name = "dummy";

            GameObject dummyChild = new GameObject();
            dummyChild.name = "dummyChild";
            dummyChild.transform.SetParent(dummyGo.transform, false);
            dummyChild.transform.position = new Vector3(2, 3, 4);
  
            IProject project = RTSL2InternalDeps.Get.Project;

            bool done = false;
            project.Open("TestProject", openError =>
            {
                project.Save(null, dummyPreview, dummyGo, (saveError, assetItem) =>
                {
                    Assert.IsFalse(saveError.HasError);
                    Assert.AreEqual(assetItem.Parent, project.Root);

                    Assert.IsNotNull(assetItem.Preview);
                    Assert.AreEqual(dummyPreview[2], assetItem.Preview.PreviewData[2]);
                    if(unload)
                    {
                        project.Unload(ao =>
                        {
                            done = Load(assetItem, dummyGo, dummyChild, project, done);
                        });
                    }
                    else
                    {
                        done = Load(assetItem, dummyGo, dummyChild, project, done);
                    }

                });
            });

            while (!done)
            {
                yield return null;
            }
        }


        private static bool Load(AssetItem assetItem, GameObject dummyGo, GameObject dummyChild, IProject project, bool done)
        {
            project.Load(assetItem, (loadError, obj) =>
            {
                Assert.IsFalse(loadError.HasError);
                GameObject loadedGo = (GameObject)obj;
                Assert.AreEqual(dummyGo.name, loadedGo.name);
                Assert.AreEqual(dummyGo.transform.position, loadedGo.transform.position);

                Assert.AreEqual(1, loadedGo.transform.childCount);
                foreach(Transform child in loadedGo.transform)
                {
                    Assert.AreEqual(dummyChild.name, child.gameObject.name);
                    Assert.AreEqual(dummyChild.transform.position, child.transform.position);
                }
                
                done = true;
            });
            return done;
        }

        [UnityTest]
        public IEnumerator SaveLoadTest2([Values(false, true)]bool unload)
        {
            GameObject projectGo = new GameObject();
            projectGo.AddComponent<Project>();

            byte[] dummyPreview = new byte[] { 0x1, 0x2, 0x3 };
            GameObject dummyGo = new GameObject();
            dummyGo.transform.position = new Vector3(1, 2, 3);
            dummyGo.name = "dummy";

            GameObject dummyChild = new GameObject();
            dummyChild.name = "dummyChild";
            dummyChild.transform.SetParent(dummyGo.transform, false);
            dummyChild.transform.position = new Vector3(2, 3, 4);

            IProject project = RTSL2InternalDeps.Get.Project;

            var openResult = project.Open("TestProject");
            yield return openResult;

            var saveResult = project.Save(null, dummyPreview, dummyGo);
            yield return saveResult;

            Assert.IsFalse(saveResult.Error.HasError);
            Assert.AreEqual(saveResult.Result.Parent, project.Root);
            Assert.IsNotNull(saveResult.Result.Preview);
            Assert.AreEqual(dummyPreview[2], saveResult.Result.Preview.PreviewData[2]);
            if (unload)
            {
                yield return project.Unload();
                yield return Load2(saveResult.Result, dummyGo, dummyChild, project);
            }
            else
            {
                yield return Load2(saveResult.Result, dummyGo, dummyChild, project);
            }
        }

        private static IEnumerator Load2(AssetItem assetItem, GameObject dummyGo, GameObject dummyChild, IProject project)
        {
            var loadResult = project.Load(assetItem);
            yield return loadResult;

            Assert.IsFalse(loadResult.Error.HasError);
            GameObject loadedGo = (GameObject)loadResult.Result;
            Assert.AreEqual(dummyGo.name, loadedGo.name);
            Assert.AreEqual(dummyGo.transform.position, loadedGo.transform.position);

            Assert.AreEqual(1, loadedGo.transform.childCount);
            foreach (Transform child in loadedGo.transform)
            {
                Assert.AreEqual(dummyChild.name, child.gameObject.name);
                Assert.AreEqual(dummyChild.transform.position, child.transform.position);
            }
        }
    }

}

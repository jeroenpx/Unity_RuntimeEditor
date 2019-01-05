using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

using Battlehub.RTSaveLoad2.Interface;
using Battlehub.RTCommon;

namespace Battlehub.RTSaveLoad2
{
    public class ProjectTests
    {
        [TearDown]
        public void Cleanup()
        {
            RTSL2Deps deps = Object.FindObjectOfType<RTSL2Deps>();
            if(deps != null)
            {
                Object.DestroyImmediate(deps.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator OpenProjectTest()
        {
            IProject project = IOC.Resolve<IProject>();
            Assert.IsNull(project.Root);

            bool done = false;
            project.OpenProject("TestProject", (error, projectInfo) =>
            {
                Assert.IsFalse(error.HasError);
                Assert.IsNotNull(project.Root);
                done = true;
            });

            while (!done)
            {
                yield return null;
            }


        }

        [Test]
        public void SaveTest1()
        {
            IProject project = IOC.Resolve<IProject>();
            Assert.Throws<System.InvalidOperationException>(() =>
            {
                project.Save(null, null, null);
            },
            "Project is not opened. Use OpenProject method");
        }

        [Test]
        public void SaveTest2()
        {
            IProject project = IOC.Resolve<IProject>();
            Assert.Throws<System.InvalidOperationException>(() =>
            {
                project.Create(null, null, null, null);
            },
            "Project is not opened. Use OpenProject method");
        }


        [UnityTest]
        public IEnumerator SaveLoadTest([Values(false, true)]bool unload)
        {
            byte[] dummyPreview = new byte[] { 0x1, 0x2, 0x3 };
            GameObject dummyGo = new GameObject();
            dummyGo.transform.position = new Vector3(1, 2, 3);
            dummyGo.name = "dummy";

            GameObject dummyChild = new GameObject();
            dummyChild.name = "dummyChild";
            dummyChild.transform.SetParent(dummyGo.transform, false);
            dummyChild.transform.position = new Vector3(2, 3, 4);

            IProject project = IOC.Resolve<IProject>();

            bool done = false;
            project.OpenProject("TestProject", (openError, projectInfo) =>
            {
                project.Create(null, new[] { dummyPreview }, new[] { dummyGo }, null, (saveError, assetItem) =>
                {
                    Assert.IsFalse(saveError.HasError);
                    Assert.AreEqual(assetItem[0].Parent, project.Root);

                    Assert.IsNotNull(assetItem[0].Preview);
                    Assert.AreEqual(dummyPreview, assetItem[0].Preview.PreviewData[2]);
                    if (unload)
                    {
                        project.Unload(ao =>
                        {
                            done = Load(assetItem[0], dummyGo, dummyChild, project, done);
                        });
                    }
                    else
                    {
                        done = Load(assetItem[0], dummyGo, dummyChild, project, done);
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
                foreach (Transform child in loadedGo.transform)
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
            byte[] dummyPreview = new byte[] { 0x1, 0x2, 0x3 };
            GameObject dummyGo = new GameObject();
            dummyGo.transform.position = new Vector3(1, 2, 3);
            dummyGo.name = "dummy";

            GameObject dummyChild = new GameObject();
            dummyChild.name = "dummyChild";
            dummyChild.transform.SetParent(dummyGo.transform, false);
            dummyChild.transform.position = new Vector3(2, 3, 4);

            IProject project = IOC.Resolve<IProject>();

            var openResult = project.OpenProject("TestProject");
            yield return openResult;

            var saveResult = project.Create(null, new[] { dummyPreview }, new[] { dummyGo }, null);
            yield return saveResult;

            Assert.IsFalse(saveResult.Error.HasError);
            Assert.AreEqual(saveResult.Result[0].Parent, project.Root);
            Assert.IsNotNull(saveResult.Result[0].Preview);
            Assert.AreEqual(dummyPreview[2], saveResult.Result[0].Preview.PreviewData[2]);
            if (unload)
            {
                yield return project.Unload();
                yield return Load2(saveResult.Result[0], dummyGo, dummyChild, project);
            }
            else
            {
                yield return Load2(saveResult.Result[0], dummyGo, dummyChild, project);
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

        [UnityTest]
        public IEnumerator AssetItemsSerializationTest()
        {
            AssetItem assetItem = new AssetItem();
            assetItem.ItemID = 123;
            assetItem.TypeGuid = System.Guid.NewGuid();

            ProtobufSerializer serizalizer = new ProtobufSerializer();

            AssetItem clone = serizalizer.DeepClone(assetItem);

            Assert.AreEqual(assetItem.ItemID, clone.ItemID);
            Assert.AreEqual(assetItem.TypeGuid, clone.TypeGuid);

            yield break;
        }
    }

}

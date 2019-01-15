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

   

      

        private static bool Load(AssetItem assetItem, GameObject dummyGo, GameObject dummyChild, IProject project, bool done)
        {
            project.Load(new[] { assetItem }, (loadError, obj) =>
            {
                Assert.IsFalse(loadError.HasError);
                GameObject loadedGo = (GameObject)obj[0];
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

 

        private static IEnumerator Load2(AssetItem assetItem, GameObject dummyGo, GameObject dummyChild, IProject project)
        {
            var loadResult = project.Load(new[] { assetItem });
            yield return loadResult;

            Assert.IsFalse(loadResult.Error.HasError);
            GameObject loadedGo = (GameObject)loadResult.Result[0];
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

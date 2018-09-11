using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;

namespace Battlehub.RTSaveLoad2
{
    public class PersistentSceneTests
    {
        [TearDown]
        public void Cleanup()
        {
            var rtsl2Deps = Object.FindObjectOfType<RTSL2Deps>();
            if(rtsl2Deps != null)
            {
                Object.DestroyImmediate(rtsl2Deps.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator PersistentTransformTest()
        {
            GameObject deps = new GameObject();
            deps.AddComponent<RTSL2Deps>();

            PersistentTransform tf = new PersistentTransform();
            tf.position = Vector3.one;
            tf.rotation = Quaternion.Euler(45, 45, 45);
            tf.localScale = Vector3.one * 2;

            ISerializer serializer = RTSL2Deps.Get.Serializer;
            PersistentTransform clone = serializer.Deserialize<PersistentTransform>(serializer.Serialize(tf));

            Assert.AreEqual(tf.position, clone.position);
            Assert.AreEqual(tf.rotation, clone.rotation);
            Assert.AreEqual(tf.localScale, clone.localScale);

            yield break;
        }
        [UnityTest]
        public IEnumerator PersistentSceneTest()
        {
            GameObject[] sceneGOs = SceneManager.GetActiveScene().GetRootGameObjects().ToArray();
            for (int i = 0; i < sceneGOs.Length; ++i)
            {
                sceneGOs[i].AddComponent<RTSL2Ignore>();
            }

            GameObject go = new GameObject();
            go.name = "root";
            go.transform.position = Vector3.one;
            go.transform.rotation = Quaternion.Euler(45, 45, 45);
            go.transform.localScale = Vector3.one * 2;

            GameObject child = new GameObject();
            child.name = "child";
            child.hideFlags = HideFlags.NotEditable;
            child.transform.position = Vector3.one * 2;
            child.transform.SetParent(go.transform, true);

            GameObject deps = new GameObject();
            deps.AddComponent<RTSL2Deps>();


            PersistentRuntimeScene scene = new PersistentRuntimeScene();
            scene.ReadFrom(SceneManager.GetActiveScene());

            ISerializer serializer = RTSL2Deps.Get.Serializer;
            PersistentRuntimeScene clone = serializer.DeepClone(scene);

            clone.WriteTo(SceneManager.GetActiveScene());
            yield return null;
            GameObject[] restoredGOs = SceneManager.GetActiveScene().GetRootGameObjects().ToArray().Where(g => g.name.Contains("root")).ToArray();
            Assert.AreEqual(1, restoredGOs.Length);

            go = restoredGOs[0];
            Assert.AreEqual("root", go.name);
            Assert.AreEqual(Vector3.one, go.transform.position);
            Assert.AreEqual(Quaternion.Euler(45, 45, 45), go.transform.rotation);
            Assert.AreEqual(Vector3.one * 2, go.transform.localScale);

            Assert.AreEqual(1, go.transform.childCount);
            child = go.transform.GetChild(0).gameObject;
            Assert.AreEqual("child", child.name);
            Assert.AreEqual(HideFlags.NotEditable, child.hideFlags);
            Assert.AreEqual(Vector3.one * 2, child.transform.position);
        }

        //// A UnityTest behaves like a coroutine in PlayMode
        //// and allows you to yield null to skip a frame in EditMode
        //[UnityTest]
        //public IEnumerator NewTestScriptWithEnumeratorPasses()
        //{
        //    // Use the Assert class to test conditions.
        //    // yield to skip a frame
        //    yield return null;
        //}
    }

}

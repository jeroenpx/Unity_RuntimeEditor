using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSaveLoad2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Battlehub.SL2;
using UnityEngine.SceneManagement;
using UnityEngine.SceneManagement.Battlehub.SL2;
using Battlehub.RTSaveLoad2.Battlehub.SL2;
using Battlehub.RTCommon;
using Battlehub.RTCommon.Battlehub.SL2;
using Battlehub.SL2;
using UnityEngine.Events;
using UnityEngine.Events.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2
{
    public partial class TypeMap
    {
        public TypeMap()
        {
            m_toPeristentType.Add(typeof(UnityObject), typeof(PersistentObject));
            m_toUnityType.Add(typeof(PersistentObject), typeof(UnityObject));
            m_toGuid.Add(typeof(PersistentObject), new System.Guid("b4abccaa-7fd8-4035-8c90-2d46ac7278ed"));
            m_toGuid.Add(typeof(UnityObject), new System.Guid("9cdd70ef-9948-4b85-8432-09eaf0faf4b7"));
            m_toType.Add(new System.Guid("b4abccaa-7fd8-4035-8c90-2d46ac7278ed"), typeof(PersistentObject));
            m_toType.Add(new System.Guid("9cdd70ef-9948-4b85-8432-09eaf0faf4b7"), typeof(UnityObject));
            m_toPeristentType.Add(typeof(GameObject), typeof(PersistentGameObject));
            m_toUnityType.Add(typeof(PersistentGameObject), typeof(GameObject));
            m_toGuid.Add(typeof(PersistentGameObject), new System.Guid("163584cb-bfc6-423c-ab02-e43c961af6d6"));
            m_toGuid.Add(typeof(GameObject), new System.Guid("2e76e2f0-289d-4c48-936f-56033397359c"));
            m_toType.Add(new System.Guid("163584cb-bfc6-423c-ab02-e43c961af6d6"), typeof(PersistentGameObject));
            m_toType.Add(new System.Guid("2e76e2f0-289d-4c48-936f-56033397359c"), typeof(GameObject));
            m_toPeristentType.Add(typeof(Renderer), typeof(PersistentRenderer));
            m_toUnityType.Add(typeof(PersistentRenderer), typeof(Renderer));
            m_toGuid.Add(typeof(PersistentRenderer), new System.Guid("dbdc3eb1-d638-4604-9b06-8d3898a70da6"));
            m_toGuid.Add(typeof(Renderer), new System.Guid("17bfdd8a-b397-4e52-8a97-4e97840d5703"));
            m_toType.Add(new System.Guid("dbdc3eb1-d638-4604-9b06-8d3898a70da6"), typeof(PersistentRenderer));
            m_toType.Add(new System.Guid("17bfdd8a-b397-4e52-8a97-4e97840d5703"), typeof(Renderer));
            m_toPeristentType.Add(typeof(MeshRenderer), typeof(PersistentMeshRenderer));
            m_toUnityType.Add(typeof(PersistentMeshRenderer), typeof(MeshRenderer));
            m_toGuid.Add(typeof(PersistentMeshRenderer), new System.Guid("ca2fc5bb-2928-4451-9031-6b0814fde1de"));
            m_toGuid.Add(typeof(MeshRenderer), new System.Guid("cc07f430-a866-4d47-b67a-6e110e1bbd77"));
            m_toType.Add(new System.Guid("ca2fc5bb-2928-4451-9031-6b0814fde1de"), typeof(PersistentMeshRenderer));
            m_toType.Add(new System.Guid("cc07f430-a866-4d47-b67a-6e110e1bbd77"), typeof(MeshRenderer));
            m_toPeristentType.Add(typeof(MeshFilter), typeof(PersistentMeshFilter));
            m_toUnityType.Add(typeof(PersistentMeshFilter), typeof(MeshFilter));
            m_toGuid.Add(typeof(PersistentMeshFilter), new System.Guid("7d851c06-d298-4cff-9634-265143b0ea3b"));
            m_toGuid.Add(typeof(MeshFilter), new System.Guid("49aa11a8-d1b4-4dd1-a211-a4424803439f"));
            m_toType.Add(new System.Guid("7d851c06-d298-4cff-9634-265143b0ea3b"), typeof(PersistentMeshFilter));
            m_toType.Add(new System.Guid("49aa11a8-d1b4-4dd1-a211-a4424803439f"), typeof(MeshFilter));
            m_toPeristentType.Add(typeof(SkinnedMeshRenderer), typeof(PersistentSkinnedMeshRenderer));
            m_toUnityType.Add(typeof(PersistentSkinnedMeshRenderer), typeof(SkinnedMeshRenderer));
            m_toGuid.Add(typeof(PersistentSkinnedMeshRenderer), new System.Guid("fef39919-9e1c-4b8a-903d-531bc443fb83"));
            m_toGuid.Add(typeof(SkinnedMeshRenderer), new System.Guid("b3e51931-8e07-4493-a8fe-c3146657ccb7"));
            m_toType.Add(new System.Guid("fef39919-9e1c-4b8a-903d-531bc443fb83"), typeof(PersistentSkinnedMeshRenderer));
            m_toType.Add(new System.Guid("b3e51931-8e07-4493-a8fe-c3146657ccb7"), typeof(SkinnedMeshRenderer));
            m_toPeristentType.Add(typeof(Mesh), typeof(PersistentMesh));
            m_toUnityType.Add(typeof(PersistentMesh), typeof(Mesh));
            m_toGuid.Add(typeof(PersistentMesh), new System.Guid("f05395cf-2ab3-458b-ad9c-2b0a12efa34f"));
            m_toGuid.Add(typeof(Mesh), new System.Guid("982272c4-0273-45ca-bfed-b5d0817a4e3a"));
            m_toType.Add(new System.Guid("f05395cf-2ab3-458b-ad9c-2b0a12efa34f"), typeof(PersistentMesh));
            m_toType.Add(new System.Guid("982272c4-0273-45ca-bfed-b5d0817a4e3a"), typeof(Mesh));
            m_toPeristentType.Add(typeof(Material), typeof(PersistentMaterial));
            m_toUnityType.Add(typeof(PersistentMaterial), typeof(Material));
            m_toGuid.Add(typeof(PersistentMaterial), new System.Guid("2cfed6cd-9c43-48e4-8982-ef8aa26207d2"));
            m_toGuid.Add(typeof(Material), new System.Guid("49a0281a-38ba-48ab-881f-32296c75dcd3"));
            m_toType.Add(new System.Guid("2cfed6cd-9c43-48e4-8982-ef8aa26207d2"), typeof(PersistentMaterial));
            m_toType.Add(new System.Guid("49a0281a-38ba-48ab-881f-32296c75dcd3"), typeof(Material));
            m_toPeristentType.Add(typeof(Rigidbody), typeof(PersistentRigidbody));
            m_toUnityType.Add(typeof(PersistentRigidbody), typeof(Rigidbody));
            m_toGuid.Add(typeof(PersistentRigidbody), new System.Guid("1219093d-34f0-4302-8a56-b98739e5023e"));
            m_toGuid.Add(typeof(Rigidbody), new System.Guid("94628cf4-c23b-402a-bca1-88c365470cf3"));
            m_toType.Add(new System.Guid("1219093d-34f0-4302-8a56-b98739e5023e"), typeof(PersistentRigidbody));
            m_toType.Add(new System.Guid("94628cf4-c23b-402a-bca1-88c365470cf3"), typeof(Rigidbody));
            m_toPeristentType.Add(typeof(BoxCollider), typeof(PersistentBoxCollider));
            m_toUnityType.Add(typeof(PersistentBoxCollider), typeof(BoxCollider));
            m_toGuid.Add(typeof(PersistentBoxCollider), new System.Guid("1803418e-463d-4eaf-bf2b-491876650268"));
            m_toGuid.Add(typeof(BoxCollider), new System.Guid("054b2788-cd8a-471c-a514-2c3e42fb8ddb"));
            m_toType.Add(new System.Guid("1803418e-463d-4eaf-bf2b-491876650268"), typeof(PersistentBoxCollider));
            m_toType.Add(new System.Guid("054b2788-cd8a-471c-a514-2c3e42fb8ddb"), typeof(BoxCollider));
            m_toPeristentType.Add(typeof(SphereCollider), typeof(PersistentSphereCollider));
            m_toUnityType.Add(typeof(PersistentSphereCollider), typeof(SphereCollider));
            m_toGuid.Add(typeof(PersistentSphereCollider), new System.Guid("22637c31-1c25-4a83-9cdb-95185b41cdb6"));
            m_toGuid.Add(typeof(SphereCollider), new System.Guid("1f886a21-589c-44e5-90af-06bb2b57a7fd"));
            m_toType.Add(new System.Guid("22637c31-1c25-4a83-9cdb-95185b41cdb6"), typeof(PersistentSphereCollider));
            m_toType.Add(new System.Guid("1f886a21-589c-44e5-90af-06bb2b57a7fd"), typeof(SphereCollider));
            m_toPeristentType.Add(typeof(CapsuleCollider), typeof(PersistentCapsuleCollider));
            m_toUnityType.Add(typeof(PersistentCapsuleCollider), typeof(CapsuleCollider));
            m_toGuid.Add(typeof(PersistentCapsuleCollider), new System.Guid("aece17a6-f390-43f6-bb30-f563edf77ee1"));
            m_toGuid.Add(typeof(CapsuleCollider), new System.Guid("749fd212-7099-404f-9223-ff68db990b53"));
            m_toType.Add(new System.Guid("aece17a6-f390-43f6-bb30-f563edf77ee1"), typeof(PersistentCapsuleCollider));
            m_toType.Add(new System.Guid("749fd212-7099-404f-9223-ff68db990b53"), typeof(CapsuleCollider));
            m_toPeristentType.Add(typeof(MeshCollider), typeof(PersistentMeshCollider));
            m_toUnityType.Add(typeof(PersistentMeshCollider), typeof(MeshCollider));
            m_toGuid.Add(typeof(PersistentMeshCollider), new System.Guid("7019a630-1e92-4996-a068-228fc3418794"));
            m_toGuid.Add(typeof(MeshCollider), new System.Guid("24ae57b8-5e5f-40b0-9c5c-ffd9a044611e"));
            m_toType.Add(new System.Guid("7019a630-1e92-4996-a068-228fc3418794"), typeof(PersistentMeshCollider));
            m_toType.Add(new System.Guid("24ae57b8-5e5f-40b0-9c5c-ffd9a044611e"), typeof(MeshCollider));
            m_toPeristentType.Add(typeof(Camera), typeof(PersistentCamera));
            m_toUnityType.Add(typeof(PersistentCamera), typeof(Camera));
            m_toGuid.Add(typeof(PersistentCamera), new System.Guid("7a9e30a7-9724-4d77-83c0-616dd8c2da4f"));
            m_toGuid.Add(typeof(Camera), new System.Guid("d9a87c76-a0f8-48af-bf09-c2650ee2f8cb"));
            m_toType.Add(new System.Guid("7a9e30a7-9724-4d77-83c0-616dd8c2da4f"), typeof(PersistentCamera));
            m_toType.Add(new System.Guid("d9a87c76-a0f8-48af-bf09-c2650ee2f8cb"), typeof(Camera));
            m_toPeristentType.Add(typeof(Light), typeof(PersistentLight));
            m_toUnityType.Add(typeof(PersistentLight), typeof(Light));
            m_toGuid.Add(typeof(PersistentLight), new System.Guid("309683e0-b63e-4c7d-a6a4-8e179be02c88"));
            m_toGuid.Add(typeof(Light), new System.Guid("668b4fb9-f4bd-44b3-87b1-8d3e9c0cbb7f"));
            m_toType.Add(new System.Guid("309683e0-b63e-4c7d-a6a4-8e179be02c88"), typeof(PersistentLight));
            m_toType.Add(new System.Guid("668b4fb9-f4bd-44b3-87b1-8d3e9c0cbb7f"), typeof(Light));
            m_toPeristentType.Add(typeof(Behaviour), typeof(PersistentBehaviour));
            m_toUnityType.Add(typeof(PersistentBehaviour), typeof(Behaviour));
            m_toGuid.Add(typeof(PersistentBehaviour), new System.Guid("6c6c639f-9914-42e6-91d8-1d22847590af"));
            m_toGuid.Add(typeof(Behaviour), new System.Guid("1afdd06b-9687-43f7-b118-15e7f5032ed9"));
            m_toType.Add(new System.Guid("6c6c639f-9914-42e6-91d8-1d22847590af"), typeof(PersistentBehaviour));
            m_toType.Add(new System.Guid("1afdd06b-9687-43f7-b118-15e7f5032ed9"), typeof(Behaviour));
            m_toPeristentType.Add(typeof(Collider), typeof(PersistentCollider));
            m_toUnityType.Add(typeof(PersistentCollider), typeof(Collider));
            m_toGuid.Add(typeof(PersistentCollider), new System.Guid("f2b2c22d-f1a9-488f-b628-d94e0878aa2b"));
            m_toGuid.Add(typeof(Collider), new System.Guid("2ef12768-4d72-4405-8674-289fa3c8dbc6"));
            m_toType.Add(new System.Guid("f2b2c22d-f1a9-488f-b628-d94e0878aa2b"), typeof(PersistentCollider));
            m_toType.Add(new System.Guid("2ef12768-4d72-4405-8674-289fa3c8dbc6"), typeof(Collider));
            m_toPeristentType.Add(typeof(Component), typeof(PersistentComponent));
            m_toUnityType.Add(typeof(PersistentComponent), typeof(Component));
            m_toGuid.Add(typeof(PersistentComponent), new System.Guid("f7be1b4c-1306-4074-8076-f8bef011ab72"));
            m_toGuid.Add(typeof(Component), new System.Guid("d19c5e1f-80d6-4294-9be4-713150ba5152"));
            m_toType.Add(new System.Guid("f7be1b4c-1306-4074-8076-f8bef011ab72"), typeof(PersistentComponent));
            m_toType.Add(new System.Guid("d19c5e1f-80d6-4294-9be4-713150ba5152"), typeof(Component));
            m_toPeristentType.Add(typeof(Flare), typeof(PersistentFlare));
            m_toUnityType.Add(typeof(PersistentFlare), typeof(Flare));
            m_toGuid.Add(typeof(PersistentFlare), new System.Guid("b2dfc5ab-7bde-41a1-b3a4-4d57de5f7390"));
            m_toGuid.Add(typeof(Flare), new System.Guid("c0274c2d-266b-408a-915b-1f427c9c180b"));
            m_toType.Add(new System.Guid("b2dfc5ab-7bde-41a1-b3a4-4d57de5f7390"), typeof(PersistentFlare));
            m_toType.Add(new System.Guid("c0274c2d-266b-408a-915b-1f427c9c180b"), typeof(Flare));
            m_toPeristentType.Add(typeof(MonoBehaviour), typeof(PersistentMonoBehaviour));
            m_toUnityType.Add(typeof(PersistentMonoBehaviour), typeof(MonoBehaviour));
            m_toGuid.Add(typeof(PersistentMonoBehaviour), new System.Guid("66a67c35-7782-4112-ae0e-0b50bb7ac23d"));
            m_toGuid.Add(typeof(MonoBehaviour), new System.Guid("8cf9be54-9c07-4a52-bc55-92a4f8bb1743"));
            m_toType.Add(new System.Guid("66a67c35-7782-4112-ae0e-0b50bb7ac23d"), typeof(PersistentMonoBehaviour));
            m_toType.Add(new System.Guid("8cf9be54-9c07-4a52-bc55-92a4f8bb1743"), typeof(MonoBehaviour));
            m_toPeristentType.Add(typeof(PhysicMaterial), typeof(PersistentPhysicMaterial));
            m_toUnityType.Add(typeof(PersistentPhysicMaterial), typeof(PhysicMaterial));
            m_toGuid.Add(typeof(PersistentPhysicMaterial), new System.Guid("a0c9f6c5-4fb9-4a3b-bf4c-d59323ef3569"));
            m_toGuid.Add(typeof(PhysicMaterial), new System.Guid("202d27b2-8dec-4034-9797-c0c1716f536f"));
            m_toType.Add(new System.Guid("a0c9f6c5-4fb9-4a3b-bf4c-d59323ef3569"), typeof(PersistentPhysicMaterial));
            m_toType.Add(new System.Guid("202d27b2-8dec-4034-9797-c0c1716f536f"), typeof(PhysicMaterial));
            m_toPeristentType.Add(typeof(RenderTexture), typeof(PersistentRenderTexture));
            m_toUnityType.Add(typeof(PersistentRenderTexture), typeof(RenderTexture));
            m_toGuid.Add(typeof(PersistentRenderTexture), new System.Guid("e4100056-e4fc-4f2b-9e06-7b96bebcad6e"));
            m_toGuid.Add(typeof(RenderTexture), new System.Guid("25ef3afd-4031-4189-8e64-1e84e2d59caa"));
            m_toType.Add(new System.Guid("e4100056-e4fc-4f2b-9e06-7b96bebcad6e"), typeof(PersistentRenderTexture));
            m_toType.Add(new System.Guid("25ef3afd-4031-4189-8e64-1e84e2d59caa"), typeof(RenderTexture));
            m_toPeristentType.Add(typeof(Shader), typeof(PersistentShader));
            m_toUnityType.Add(typeof(PersistentShader), typeof(Shader));
            m_toGuid.Add(typeof(PersistentShader), new System.Guid("211c5ea8-5e90-472a-a2f3-daa1c6e1ffff"));
            m_toGuid.Add(typeof(Shader), new System.Guid("aadd510d-d912-4060-8291-d2527fd5e46c"));
            m_toType.Add(new System.Guid("211c5ea8-5e90-472a-a2f3-daa1c6e1ffff"), typeof(PersistentShader));
            m_toType.Add(new System.Guid("aadd510d-d912-4060-8291-d2527fd5e46c"), typeof(Shader));
            m_toPeristentType.Add(typeof(Texture), typeof(PersistentTexture));
            m_toUnityType.Add(typeof(PersistentTexture), typeof(Texture));
            m_toGuid.Add(typeof(PersistentTexture), new System.Guid("a575e93e-35c6-437b-bb02-0b6f090a655d"));
            m_toGuid.Add(typeof(Texture), new System.Guid("dc6892fd-0a34-496e-8f4f-dc10554a2b21"));
            m_toType.Add(new System.Guid("a575e93e-35c6-437b-bb02-0b6f090a655d"), typeof(PersistentTexture));
            m_toType.Add(new System.Guid("dc6892fd-0a34-496e-8f4f-dc10554a2b21"), typeof(Texture));
            m_toPeristentType.Add(typeof(Texture2D), typeof(PersistentTexture2D));
            m_toUnityType.Add(typeof(PersistentTexture2D), typeof(Texture2D));
            m_toGuid.Add(typeof(PersistentTexture2D), new System.Guid("be6610d9-a548-446a-a1ba-2bf9dd4a35ed"));
            m_toGuid.Add(typeof(Texture2D), new System.Guid("f8262723-d973-479f-aa3a-0e5cfc9e3b4b"));
            m_toType.Add(new System.Guid("be6610d9-a548-446a-a1ba-2bf9dd4a35ed"), typeof(PersistentTexture2D));
            m_toType.Add(new System.Guid("f8262723-d973-479f-aa3a-0e5cfc9e3b4b"), typeof(Texture2D));
            m_toPeristentType.Add(typeof(Transform), typeof(PersistentTransform));
            m_toUnityType.Add(typeof(PersistentTransform), typeof(Transform));
            m_toGuid.Add(typeof(PersistentTransform), new System.Guid("3b5d0310-44df-44a1-8689-245048c8151a"));
            m_toGuid.Add(typeof(Transform), new System.Guid("b542d079-2da8-4468-80b4-ba4c6adaa225"));
            m_toType.Add(new System.Guid("3b5d0310-44df-44a1-8689-245048c8151a"), typeof(PersistentTransform));
            m_toType.Add(new System.Guid("b542d079-2da8-4468-80b4-ba4c6adaa225"), typeof(Transform));
            m_toPeristentType.Add(typeof(RuntimePrefab), typeof(PersistentRuntimePrefab));
            m_toUnityType.Add(typeof(PersistentRuntimePrefab), typeof(RuntimePrefab));
            m_toGuid.Add(typeof(PersistentRuntimePrefab), new System.Guid("342f1ac4-0726-44b2-a590-128d30733ed9"));
            m_toGuid.Add(typeof(RuntimePrefab), new System.Guid("f48033d5-1249-4a86-b56a-589888323932"));
            m_toType.Add(new System.Guid("342f1ac4-0726-44b2-a590-128d30733ed9"), typeof(PersistentRuntimePrefab));
            m_toType.Add(new System.Guid("f48033d5-1249-4a86-b56a-589888323932"), typeof(RuntimePrefab));
            m_toPeristentType.Add(typeof(RuntimeScene), typeof(PersistentRuntimeScene));
            m_toUnityType.Add(typeof(PersistentRuntimeScene), typeof(RuntimeScene));
            m_toGuid.Add(typeof(PersistentRuntimeScene), new System.Guid("e966174d-f3d0-46a7-b793-498cb3f78672"));
            m_toGuid.Add(typeof(RuntimeScene), new System.Guid("ecb2a50b-b00e-4a78-9065-c58eb67994be"));
            m_toType.Add(new System.Guid("e966174d-f3d0-46a7-b793-498cb3f78672"), typeof(PersistentRuntimeScene));
            m_toType.Add(new System.Guid("ecb2a50b-b00e-4a78-9065-c58eb67994be"), typeof(RuntimeScene));
            m_toPeristentType.Add(typeof(ExposeToEditor), typeof(PersistentExposeToEditor));
            m_toUnityType.Add(typeof(PersistentExposeToEditor), typeof(ExposeToEditor));
            m_toGuid.Add(typeof(PersistentExposeToEditor), new System.Guid("0b046b2c-5763-4525-bac0-cc07da016cdd"));
            m_toGuid.Add(typeof(ExposeToEditor), new System.Guid("34740418-83eb-4b3a-931a-07bee88d81db"));
            m_toType.Add(new System.Guid("0b046b2c-5763-4525-bac0-cc07da016cdd"), typeof(PersistentExposeToEditor));
            m_toType.Add(new System.Guid("34740418-83eb-4b3a-931a-07bee88d81db"), typeof(ExposeToEditor));
            m_toPeristentType.Add(typeof(TestBehavior), typeof(PersistentTestBehavior));
            m_toUnityType.Add(typeof(PersistentTestBehavior), typeof(TestBehavior));
            m_toGuid.Add(typeof(PersistentTestBehavior), new System.Guid("9c1990b4-3f97-4a73-9005-4220ffafc5b9"));
            m_toGuid.Add(typeof(TestBehavior), new System.Guid("9ac80b19-a75b-4a9c-9291-41cd41f53c4f"));
            m_toType.Add(new System.Guid("9c1990b4-3f97-4a73-9005-4220ffafc5b9"), typeof(PersistentTestBehavior));
            m_toType.Add(new System.Guid("9ac80b19-a75b-4a9c-9291-41cd41f53c4f"), typeof(TestBehavior));
            m_toPeristentType.Add(typeof(Vector2), typeof(PersistentVector2));
            m_toUnityType.Add(typeof(PersistentVector2), typeof(Vector2));
            m_toGuid.Add(typeof(PersistentVector2), new System.Guid("5ba3ff6b-8b6a-4184-8949-663c95d9c896"));
            m_toGuid.Add(typeof(Vector2), new System.Guid("1dc7a4b8-c9d9-459e-8391-2f64877c791f"));
            m_toType.Add(new System.Guid("5ba3ff6b-8b6a-4184-8949-663c95d9c896"), typeof(PersistentVector2));
            m_toType.Add(new System.Guid("1dc7a4b8-c9d9-459e-8391-2f64877c791f"), typeof(Vector2));
            m_toPeristentType.Add(typeof(Vector3), typeof(PersistentVector3));
            m_toUnityType.Add(typeof(PersistentVector3), typeof(Vector3));
            m_toGuid.Add(typeof(PersistentVector3), new System.Guid("d1b8fcf4-3b8a-48f1-836f-86666243ece8"));
            m_toGuid.Add(typeof(Vector3), new System.Guid("28d91efe-0de0-478d-9e7b-c76f1ba91807"));
            m_toType.Add(new System.Guid("d1b8fcf4-3b8a-48f1-836f-86666243ece8"), typeof(PersistentVector3));
            m_toType.Add(new System.Guid("28d91efe-0de0-478d-9e7b-c76f1ba91807"), typeof(Vector3));
            m_toPeristentType.Add(typeof(Vector4), typeof(PersistentVector4));
            m_toUnityType.Add(typeof(PersistentVector4), typeof(Vector4));
            m_toGuid.Add(typeof(PersistentVector4), new System.Guid("dce295a4-7b33-408c-b87e-6b85a3358dbf"));
            m_toGuid.Add(typeof(Vector4), new System.Guid("210fd541-e678-45dc-bb96-69333099ddf4"));
            m_toType.Add(new System.Guid("dce295a4-7b33-408c-b87e-6b85a3358dbf"), typeof(PersistentVector4));
            m_toType.Add(new System.Guid("210fd541-e678-45dc-bb96-69333099ddf4"), typeof(Vector4));
            m_toPeristentType.Add(typeof(Color), typeof(PersistentColor));
            m_toUnityType.Add(typeof(PersistentColor), typeof(Color));
            m_toGuid.Add(typeof(PersistentColor), new System.Guid("baea5d8e-ae9a-4eb7-bbf4-e6e80e0c85d3"));
            m_toGuid.Add(typeof(Color), new System.Guid("c5aaef8c-fce2-4ce9-8474-7094e24e7ea6"));
            m_toType.Add(new System.Guid("baea5d8e-ae9a-4eb7-bbf4-e6e80e0c85d3"), typeof(PersistentColor));
            m_toType.Add(new System.Guid("c5aaef8c-fce2-4ce9-8474-7094e24e7ea6"), typeof(Color));
            m_toPeristentType.Add(typeof(Matrix4x4), typeof(PersistentMatrix4x4));
            m_toUnityType.Add(typeof(PersistentMatrix4x4), typeof(Matrix4x4));
            m_toGuid.Add(typeof(PersistentMatrix4x4), new System.Guid("57141cea-ea73-435b-aa1f-f0f1085de05f"));
            m_toGuid.Add(typeof(Matrix4x4), new System.Guid("4b095697-2aac-48dd-9846-1749aaf8bd01"));
            m_toType.Add(new System.Guid("57141cea-ea73-435b-aa1f-f0f1085de05f"), typeof(PersistentMatrix4x4));
            m_toType.Add(new System.Guid("4b095697-2aac-48dd-9846-1749aaf8bd01"), typeof(Matrix4x4));
            m_toPeristentType.Add(typeof(BoneWeight), typeof(PersistentBoneWeight));
            m_toUnityType.Add(typeof(PersistentBoneWeight), typeof(BoneWeight));
            m_toGuid.Add(typeof(PersistentBoneWeight), new System.Guid("e51e91b4-1517-49ab-bcb4-794768876a17"));
            m_toGuid.Add(typeof(BoneWeight), new System.Guid("8232a75b-c3a4-43ef-90b2-4ea1160408cd"));
            m_toType.Add(new System.Guid("e51e91b4-1517-49ab-bcb4-794768876a17"), typeof(PersistentBoneWeight));
            m_toType.Add(new System.Guid("8232a75b-c3a4-43ef-90b2-4ea1160408cd"), typeof(BoneWeight));
            m_toPeristentType.Add(typeof(Bounds), typeof(PersistentBounds));
            m_toUnityType.Add(typeof(PersistentBounds), typeof(Bounds));
            m_toGuid.Add(typeof(PersistentBounds), new System.Guid("b673bce1-29c6-4e1f-b344-08e56f368319"));
            m_toGuid.Add(typeof(Bounds), new System.Guid("f2024152-94da-41ba-880c-b6647a791747"));
            m_toType.Add(new System.Guid("b673bce1-29c6-4e1f-b344-08e56f368319"), typeof(PersistentBounds));
            m_toType.Add(new System.Guid("f2024152-94da-41ba-880c-b6647a791747"), typeof(Bounds));
            m_toPeristentType.Add(typeof(LightBakingOutput), typeof(PersistentLightBakingOutput));
            m_toUnityType.Add(typeof(PersistentLightBakingOutput), typeof(LightBakingOutput));
            m_toGuid.Add(typeof(PersistentLightBakingOutput), new System.Guid("80666021-0475-4694-97a2-c6cccea90ef7"));
            m_toGuid.Add(typeof(LightBakingOutput), new System.Guid("9944cc93-bf5f-4b5f-9ef5-52d252cb1e76"));
            m_toType.Add(new System.Guid("80666021-0475-4694-97a2-c6cccea90ef7"), typeof(PersistentLightBakingOutput));
            m_toType.Add(new System.Guid("9944cc93-bf5f-4b5f-9ef5-52d252cb1e76"), typeof(LightBakingOutput));
            m_toPeristentType.Add(typeof(Quaternion), typeof(PersistentQuaternion));
            m_toUnityType.Add(typeof(PersistentQuaternion), typeof(Quaternion));
            m_toGuid.Add(typeof(PersistentQuaternion), new System.Guid("7e0cfd74-0fa9-4160-bdba-2498f6069b4b"));
            m_toGuid.Add(typeof(Quaternion), new System.Guid("a2c70442-63fd-4b71-a7ff-ce1749c513f3"));
            m_toType.Add(new System.Guid("7e0cfd74-0fa9-4160-bdba-2498f6069b4b"), typeof(PersistentQuaternion));
            m_toType.Add(new System.Guid("a2c70442-63fd-4b71-a7ff-ce1749c513f3"), typeof(Quaternion));
            m_toPeristentType.Add(typeof(Rect), typeof(PersistentRect));
            m_toUnityType.Add(typeof(PersistentRect), typeof(Rect));
            m_toGuid.Add(typeof(PersistentRect), new System.Guid("dec04e2b-8fbd-4ff2-bf40-8a3055417db8"));
            m_toGuid.Add(typeof(Rect), new System.Guid("ecb399a3-9a15-469f-a479-59fad334e046"));
            m_toType.Add(new System.Guid("dec04e2b-8fbd-4ff2-bf40-8a3055417db8"), typeof(PersistentRect));
            m_toType.Add(new System.Guid("ecb399a3-9a15-469f-a479-59fad334e046"), typeof(Rect));
            m_toPeristentType.Add(typeof(Scene), typeof(PersistentScene));
            m_toUnityType.Add(typeof(PersistentScene), typeof(Scene));
            m_toGuid.Add(typeof(PersistentScene), new System.Guid("471f41e7-c60a-458a-b0ff-d9ed0728e45d"));
            m_toGuid.Add(typeof(Scene), new System.Guid("ffa60660-cd3c-452f-8b3b-45f9e9d8024c"));
            m_toType.Add(new System.Guid("471f41e7-c60a-458a-b0ff-d9ed0728e45d"), typeof(PersistentScene));
            m_toType.Add(new System.Guid("ffa60660-cd3c-452f-8b3b-45f9e9d8024c"), typeof(Scene));
            m_toPeristentType.Add(typeof(ExposeToEditorUnityEvent), typeof(PersistentExposeToEditorUnityEvent));
            m_toUnityType.Add(typeof(PersistentExposeToEditorUnityEvent), typeof(ExposeToEditorUnityEvent));
            m_toGuid.Add(typeof(PersistentExposeToEditorUnityEvent), new System.Guid("7ba0181a-6bd2-4d98-be8c-096811e370c5"));
            m_toGuid.Add(typeof(ExposeToEditorUnityEvent), new System.Guid("e7635362-17e8-4c6d-88c6-cec4cdbf6701"));
            m_toType.Add(new System.Guid("7ba0181a-6bd2-4d98-be8c-096811e370c5"), typeof(PersistentExposeToEditorUnityEvent));
            m_toType.Add(new System.Guid("e7635362-17e8-4c6d-88c6-cec4cdbf6701"), typeof(ExposeToEditorUnityEvent));
            m_toPeristentType.Add(typeof(UnityEvent), typeof(PersistentUnityEvent));
            m_toUnityType.Add(typeof(PersistentUnityEvent), typeof(UnityEvent));
            m_toGuid.Add(typeof(PersistentUnityEvent), new System.Guid("9f432f32-23f3-432c-992d-6cc43b8edbad"));
            m_toGuid.Add(typeof(UnityEvent), new System.Guid("3ed54cee-4405-475c-8954-a5eaed086ad7"));
            m_toType.Add(new System.Guid("9f432f32-23f3-432c-992d-6cc43b8edbad"), typeof(PersistentUnityEvent));
            m_toType.Add(new System.Guid("3ed54cee-4405-475c-8954-a5eaed086ad7"), typeof(UnityEvent));
            m_toPeristentType.Add(typeof(UnityEventBase), typeof(PersistentUnityEventBase));
            m_toUnityType.Add(typeof(PersistentUnityEventBase), typeof(UnityEventBase));
            m_toGuid.Add(typeof(PersistentUnityEventBase), new System.Guid("1f0e42fc-2817-49d9-af67-1fd15dd6f9ed"));
            m_toGuid.Add(typeof(UnityEventBase), new System.Guid("9e9f6774-aeb8-4cc5-b149-597a62077a89"));
            m_toType.Add(new System.Guid("1f0e42fc-2817-49d9-af67-1fd15dd6f9ed"), typeof(PersistentUnityEventBase));
            m_toType.Add(new System.Guid("9e9f6774-aeb8-4cc5-b149-597a62077a89"), typeof(UnityEventBase));
            m_toPeristentType.Add(typeof(Test), typeof(PersistentTest));
            m_toUnityType.Add(typeof(PersistentTest), typeof(Test));
            m_toGuid.Add(typeof(PersistentTest), new System.Guid("7d350e73-72d0-4659-9145-cd4ade149e0b"));
            m_toGuid.Add(typeof(Test), new System.Guid("c04aa8f4-8a9d-44fc-a485-121d2923ecdc"));
            m_toType.Add(new System.Guid("7d350e73-72d0-4659-9145-cd4ade149e0b"), typeof(PersistentTest));
            m_toType.Add(new System.Guid("c04aa8f4-8a9d-44fc-a485-121d2923ecdc"), typeof(Test));
            
            OnConstructed();
        }
    }
}


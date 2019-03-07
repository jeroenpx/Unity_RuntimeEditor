using Battlehub.RTCommon;
using GoogleARCore;
using UnityEngine;

namespace Battlehub.RTHandles.ARCore
{
    public class PrefabSpawnPointAR : PrefabSpawnPoint
    {
        private TrackableHit m_hit;

        protected override Plane GetDragPlane(Camera camera, Pointer pointer, Vector3 scenePivot)
        {
            scenePivot = camera.transform.forward * 2;
            return base.GetDragPlane(camera, pointer, scenePivot);
        }

        protected override bool GetPointOnDragPlane(Camera camera, Pointer pointer, out Vector3 point, out Quaternion rotation)
        {
            // Raycast against the location the player touched to search for planes.
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;

            if (Frame.Raycast(pointer.ScreenPoint.x, pointer.ScreenPoint.y, raycastFilter, out m_hit))
            {
                rotation = m_hit.Pose.rotation;
                point = m_hit.Pose.position;
                return true;
            }
            return base.GetPointOnDragPlane(camera, pointer, out point, out rotation);
        }

        protected override GameObject InstantiatePrefab(GameObject prefab, Vector3 point, Quaternion rotation)
        {
            if(m_hit.Trackable != null)
            {
                Anchor anchor = m_hit.Trackable.CreateAnchor(m_hit.Pose);
               
                GameObject instance = Instantiate(prefab, m_hit.Pose.position, m_hit.Pose.rotation);
                instance.transform.parent = anchor.transform;
                instance.transform.localScale = Vector3.Scale(instance.transform.localScale, PrefabScale);
                return anchor.gameObject;
            }
            return base.InstantiatePrefab(prefab, point, rotation);
        }

        protected override ExposeToEditor ExposeToEditor(GameObject prefabInstance)
        {
            ExposeToEditor exposedToEditor = base.ExposeToEditor(prefabInstance);
            Anchor anchor = prefabInstance.GetComponent<Anchor>();
            if(anchor != null)
            {
                exposedToEditor.BoundsObject = anchor.transform.GetChild(0).gameObject;
            }
            return exposedToEditor;
        }
    }

}

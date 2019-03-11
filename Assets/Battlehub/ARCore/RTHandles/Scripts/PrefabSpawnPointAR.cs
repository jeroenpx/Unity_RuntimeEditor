using Battlehub.RTCommon;
using GoogleARCore;
using UnityEngine;
using UnityEngine.EventSystems;

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
                rotation = Quaternion.identity; //m_hit.Pose.rotation;
                point = m_hit.Pose.position;
                return true;
            }
            return base.GetPointOnDragPlane(camera, pointer, out point, out rotation);
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

        public override void OnEndDrag(PointerEventData eventData)
        {
            bool anchorCreated = false;
            Vector3 point;
            Quaternion rotation;
            if (GetPointOnDragPlane(Scene.Camera, Scene.Pointer, out point, out rotation))
            {
                if (m_hit.Trackable != null && PrefabInstance != null)
                {
                    Anchor anchor = m_hit.Trackable.CreateAnchor(m_hit.Pose);
                    PrefabInstance.transform.parent = anchor.transform;
                    PrefabInstance.transform.localScale = Vector3.Scale(PrefabInstance.transform.localScale, PrefabScale);
                    anchorCreated = true;
                }
            }

            if(!anchorCreated)
            {
                if(PrefabInstance != null)
                {
                    Destroy(PrefabInstance);
                    PrefabInstance = null;
                }
            }

            base.OnEndDrag(eventData);

        }


    }

}

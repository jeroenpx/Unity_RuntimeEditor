using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Battlehub.ProBuilderIntegration
{
    public struct MeshAndFace
    {
        public ProBuilderMesh mesh;
        public Face face;
    }

    public static class PBUtility
    {
        public static GameObject PickObject(Camera camera, Vector2 mousePosition)
        {
            var ray = camera.ScreenPointToRay(mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
                return hit.collider.gameObject;

            return null;
        }

        public static MeshAndFace PickFace(Camera camera, Vector3 mousePosition)
        {
            var res = new MeshAndFace();
            var go = PickObject(camera, mousePosition);

            if (go == null || !(res.mesh = go.GetComponent<ProBuilderMesh>()))
                return res;

            res.face = SelectionPicker.PickFace(camera, mousePosition, res.mesh);
            return res;
        }

        public static Dictionary<ProBuilderMesh, HashSet<Face>> PickFaces(Camera camera, Rect rect, GameObject[] gameObjects)
        {
            try
            {
                return SelectionPicker.PickFacesInRect(camera, rect, gameObjects.Select(g => g.GetComponent<ProBuilderMesh>()).Where(pbm => pbm != null).ToArray(), new PickerOptions { rectSelectMode = RectSelectMode.Partial });
            }
            catch(System.Exception e)
            {
                Debug.LogError(e);
                return new Dictionary<ProBuilderMesh, HashSet<Face>>();
            }
            
        }
    }
}


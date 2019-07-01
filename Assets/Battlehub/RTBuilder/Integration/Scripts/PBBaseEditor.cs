using UnityEngine;

namespace Battlehub.ProBuilderIntegration
{
    public abstract class PBBaseEditor : MonoBehaviour, IMeshEditor
    {
        public abstract bool HasSelection
        {
            get;
        }

        public virtual bool CenterMode
        {
            get;
            set;
        }

        public virtual bool GlobalMode
        {
            get;
            set;
        }

        public virtual bool UVEditingMode
        {
            get;
            set;
        }

        public abstract Vector3 Position
        {
            get;
            set;
        }

        public abstract Vector3 Normal
        {
            get;
        }

        public virtual Quaternion Rotation
        {
            get { return Quaternion.identity; }
        }
        
        public abstract GameObject Target
        {
            get;
        }

        public virtual void ApplySelection(MeshSelection selection)
        {
        }

        public virtual void RollbackSelection(MeshSelection selection)
        {
        }

        public virtual MeshSelection ClearSelection()
        {
            return null;
        }

        public virtual void Delete()
        {
        }

        public virtual void Extrude(float distance = 0)
        {
        }

        public virtual MeshSelection SelectHoles()
        {
            return null;
        }

        public virtual void FillHoles()
        {
        }

        public virtual MeshSelection GetSelection()
        {
            return null;
        }

        public virtual MeshEditorState GetState()
        {
            return null;
        }

        public virtual void Hover(Camera camera, Vector3 pointer)
        {
            
        }

        public virtual void BeginRotate(Quaternion initialRotation)
        {
        }

        public virtual void BeginScale()
        {

        }

        public virtual void EndRotate()
        {

        }

        public virtual void EndScale()
        {

        }

        public virtual void Rotate(Quaternion rotation)
        {
        }

        public virtual void Scale(Vector3 scale, Quaternion rotation)
        {
        }

        public virtual MeshSelection Select(Camera camera, Vector3 pointer, bool shift)
        {
            return null;
        }

        public virtual MeshSelection Select(Camera camera, Rect rect, GameObject[] gameObjects, MeshEditorSelectionMode mode)
        {
            return null;
        }

        public virtual MeshSelection Select(Material material)
        {
            return null;
        }

        public virtual MeshSelection Unselect(Material material)
        {
            return null;
        }

        public virtual void SetState(MeshEditorState state)
        {
        }

        
    }
}


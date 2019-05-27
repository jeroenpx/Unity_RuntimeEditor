using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Battlehub.ProBuilderIntegration
{
    public enum MeshEditorSelectionMode
    {
        Add,
        Substract,
        Difference
    }

    public class MeshEditorState
    {
        internal readonly Dictionary<ProBuilderMesh, MeshState> State = new Dictionary<ProBuilderMesh, MeshState>();
    }

    internal class MeshState
    {
        public readonly IList<Vector3> Positions;
        public readonly IList<Face> Faces;

        public MeshState(IList<Vector3> positions, IList<Face> faces)
        {
            Positions = positions;
            Faces = faces;
        }
    }

    public class MeshSelection
    {
        internal readonly Dictionary<ProBuilderMesh, IList<Face>> Selected = new Dictionary<ProBuilderMesh, IList<Face>>();
        internal readonly Dictionary<ProBuilderMesh, IList<Face>> Unselected = new Dictionary<ProBuilderMesh, IList<Face>>();

    }

    public interface IMeshEditor 
    {
        bool HasSelection
        {
            get;
        }

        bool CenterMode
        {
            get;
            set;
        }

        Vector3 Position
        {
            get;
            set;
        }

        Vector3 Normal
        {
            get;
        }

        GameObject Target
        {
            get;
        }

        void Extrude(float distance = 0.0f);
        MeshSelection Select(Camera camera, Vector3 pointer, bool shift);
        MeshSelection Select(Camera camera, Rect rect, GameObject[] gameObjects, MeshEditorSelectionMode mode);
        
        void RedoSelection(MeshSelection selection);
        void UndoSelection(MeshSelection selection);

        MeshSelection ClearSelection();

        MeshEditorState GetState();
        void SetState(MeshEditorState state);
    }
}



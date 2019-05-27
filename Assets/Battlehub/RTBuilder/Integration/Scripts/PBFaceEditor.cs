using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace Battlehub.ProBuilderIntegration
{
    public class PBFaceEditor : MonoBehaviour, IMeshEditor
    {
        private PBFaceSelection m_faceSelection;

        public bool HasSelection
        {
            get { return m_faceSelection.FacesCount > 0; }
        }

        public bool CenterMode
        {
            get;
            set;
        }

        public bool Global
        {
            get;
            set;
        }

        public Vector3 Position
        {
            get { return CenterMode ? m_faceSelection.CenterOfMass : m_faceSelection.LastPosition; }
            set { MoveTo(value); }
        }

        public Vector3 Normal
        {
            get { return CenterMode ? Vector3.forward : m_faceSelection.LastNormal; }
        }

        public GameObject Target
        {
            get { return m_faceSelection.LastMesh != null ? m_faceSelection.LastMesh.gameObject : null; }
        }

        private void Awake()
        {
            m_faceSelection = gameObject.AddComponent<PBFaceSelection>();
        }

        private void OnDestroy()
        {
            if(m_faceSelection != null)
            {
                Destroy(m_faceSelection);
            }
        }

        public void Extrude(float distance)
        {
            m_faceSelection.BeginChange();

            ProBuilderMesh[] meshes = m_faceSelection.Meshes.OrderBy(m => m == m_faceSelection.LastMesh).ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Face> faces = m_faceSelection.GetFaces(mesh).ToArray();
                for(int i = 0; i < faces.Count; ++i)
                {
                    m_faceSelection.Remove(faces[i]);
                }

                mesh.Extrude(faces, ExtrudeMethod.FaceNormal, 0.0f);
                
                for(int i = 0; i < faces.Count; ++i)
                {
                    m_faceSelection.Add(mesh, faces[i]);
                }

                mesh.ToMesh();
                mesh.Refresh();
            }

            m_faceSelection.EndChange();
        }

        private void MoveTo(Vector3 to)
        {
            Vector3 from = Position;
            Vector3 offset = to - from;

            IEnumerable<ProBuilderMesh> meshes = m_faceSelection.Meshes;
            foreach(ProBuilderMesh mesh in meshes)
            {
                Vector3 localOffset = mesh.transform.InverseTransformVector(offset);
                IList<Face> faces = m_faceSelection.GetFaces(mesh);
                mesh.TranslateVertices(faces, localOffset);

                mesh.ToMesh();
                mesh.Refresh();
            }
            m_faceSelection.Synchronize(
                m_faceSelection.CenterOfMass + offset,
                m_faceSelection.LastPosition + offset,
                m_faceSelection.LastNormal);
        }

        //private void Rotate(Quaternion to)
        //{
        //    Quaternion rotation = to * Quaternion.Inverse(Quaternion.LookRotation(PivotNormal));

        //    Vector3 center = PivotPosition;
            
        //    IEnumerable<ProBuilderMesh> meshes = m_faceSelection.Meshes;
        //    foreach (ProBuilderMesh mesh in meshes)
        //    {
        //        int[] indexes = m_faceSelection.GetIndexes(mesh).ToArray();
                
        //        Vertex[] vertices = mesh.GetVertices(indexes);

        //        int[] index = new int[1];
        //        for(int i = 0; i < vertices.Length; ++i)
        //        {
        //            Vertex vertex = vertices[i];
        //            Vector3 newPosition = mesh.transform.TransformPoint(vertex.position);
        //            newPosition = center + rotation * (newPosition - center);
        //            newPosition = mesh.transform.InverseTransformPoint(newPosition);

        //            index[0] = indexes[i];
        //            mesh.TranslateVertices(index, newPosition - vertex.position);
        //        }
                
        //        mesh.Refresh();
        //        mesh.ToMesh();
        //    }
        //    m_faceSelection.Synchronize(
        //        m_faceSelection.CenterOfMass,
        //        center + rotation * (m_faceSelection.LastPosition - center),
        //        rotation * m_faceSelection.LastNormal);
       // }

        //private void Scale(Vector3 to)
        //{

        //}

        public MeshSelection Select(Camera camera, Vector3 pointer, bool shift)
        {
            MeshSelection selection = null;
            MeshAndFace result = PBUtility.PickFace(camera, pointer);
            if(result.face != null)
            {
                if(m_faceSelection.IsSelected(result.face))
                {
                    if(shift)
                    {
                        m_faceSelection.Remove(result.face);
                        selection = new MeshSelection();
                        selection.Unselected.Add(result.mesh, new[] { result.face });
                    }
                    else
                    {
                        selection = ReadSelection();
                        selection.Unselected[result.mesh] = selection.Unselected[result.mesh].Where(f => f != result.face).ToArray();
                        selection.Selected.Add(result.mesh, new[] { result.face });
                        m_faceSelection.Clear();
                        m_faceSelection.Add(result.mesh, result.face);
                    }
                }
                else
                {
                    if(shift)
                    {
                        selection = new MeshSelection();
                    }
                    else
                    {
                        selection = ReadSelection();
                        m_faceSelection.Clear();
                    }
                    
                    m_faceSelection.Add(result.mesh, result.face);
                    selection.Selected.Add(result.mesh, new[] { result.face });
                }
            }
            else
            {
                if (!shift)
                {
                    selection = ReadSelection();
                    if (selection.Unselected.Count == 0)
                    {
                        selection = null;
                    }
                    m_faceSelection.Clear(); 
                }
            }
            return selection;
        }

        private MeshSelection ReadSelection()
        {
            MeshSelection selection = new MeshSelection();
            ProBuilderMesh[] meshes = m_faceSelection.Meshes.OrderBy(m => m == m_faceSelection.LastMesh).ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Face> faces = m_faceSelection.GetFaces(mesh);
                if (faces != null && faces.Count > 0)
                {
                    selection.Unselected.Add(mesh, faces.ToArray());
                }
            }
            return selection;
        }

        public MeshSelection Select(Camera camera, Rect rect, GameObject[] gameObjects, MeshEditorSelectionMode mode)
        {
            MeshSelection selection = new MeshSelection();
            m_faceSelection.BeginChange();

            Dictionary<ProBuilderMesh, HashSet<Face>> result = PBUtility.PickFaces(camera, rect, gameObjects);
            if (mode == MeshEditorSelectionMode.Add)
            {
                foreach (KeyValuePair<ProBuilderMesh, HashSet<Face>> kvp in result)
                {
                    ProBuilderMesh mesh = kvp.Key;
                    IList<Face> notSelected = kvp.Value.Where(f => !m_faceSelection.IsSelected(f)).ToArray();
                    foreach (Face face in notSelected)
                    {
                        m_faceSelection.Add(mesh, face);
                    }

                    selection.Selected.Add(mesh, notSelected);
                }
            }
            else if(mode == MeshEditorSelectionMode.Substract)
            {
                foreach (KeyValuePair<ProBuilderMesh, HashSet<Face>> kvp in result)
                {
                    ProBuilderMesh mesh = kvp.Key;
                    IList<Face> selected = kvp.Value.Where(f => m_faceSelection.IsSelected(f)).ToArray();
                    foreach (Face face in selected)
                    {
                        m_faceSelection.Remove(face);
                    }
                    selection.Unselected.Add(mesh, selected);
                }
            }
            else if(mode == MeshEditorSelectionMode.Difference)
            {
                foreach (KeyValuePair<ProBuilderMesh, HashSet<Face>> kvp in result)
                {
                    ProBuilderMesh mesh = kvp.Key;

                    IList<Face> selected = kvp.Value.Where(f => m_faceSelection.IsSelected(f)).ToArray();
                    IList<Face> notSelected = kvp.Value.Where(f => !m_faceSelection.IsSelected(f)).ToArray();

                    foreach (Face face in selected)
                    {
                        m_faceSelection.Remove(face);
                    }

                    foreach(Face face in notSelected)
                    {
                        m_faceSelection.Add(mesh, face);
                    }

                    selection.Unselected.Add(mesh, selected);
                    selection.Selected.Add(mesh, notSelected);
                }
            }

            m_faceSelection.EndChange();

            if(selection.Selected.Count == 0 && selection.Unselected.Count == 0)
            {
                selection = null;
            }

            return selection;
        }

        public MeshSelection ClearSelection()
        {
            MeshSelection meshSelection = null;
            if (m_faceSelection != null)
            {
                meshSelection = ReadSelection();
                m_faceSelection.Clear();
            }
            return meshSelection;
        }

        public void UndoSelection(MeshSelection selection)
        {
            m_faceSelection.BeginChange();

            foreach (KeyValuePair<ProBuilderMesh, IList<Face>> kvp in selection.Selected)
            {
                foreach (Face face in kvp.Value)
                {
                    m_faceSelection.Remove(face);
                }
            }

            foreach (KeyValuePair<ProBuilderMesh, IList<Face>> kvp in selection.Unselected)
            {
                ProBuilderMesh mesh = kvp.Key;
                foreach (Face face in kvp.Value)
                {
                    m_faceSelection.Add(mesh, face);
                }
            }

            m_faceSelection.EndChange();
        }

        public void RedoSelection(MeshSelection selection)
        {
            m_faceSelection.BeginChange();

            foreach (KeyValuePair<ProBuilderMesh, IList<Face>> kvp in selection.Unselected)
            {
                foreach (Face face in kvp.Value)
                {
                    m_faceSelection.Remove(face);
                }
            }

            foreach (KeyValuePair<ProBuilderMesh, IList<Face>> kvp in selection.Selected)
            {
                ProBuilderMesh mesh = kvp.Key;
                foreach (Face face in kvp.Value)
                {
                    m_faceSelection.Add(mesh, face);
                }
            }

            m_faceSelection.EndChange();
        }

        public MeshEditorState GetState()
        {
            MeshEditorState state = new MeshEditorState();
            ProBuilderMesh[] meshes = m_faceSelection.Meshes.OrderBy(m => m == m_faceSelection.LastMesh).ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                state.State.Add(mesh, new MeshState(mesh.positions.ToArray(), mesh.faces.ToArray()));
            }
            return state;
        }

        public void SetState(MeshEditorState state)
        {
            m_faceSelection.BeginChange();

            ProBuilderMesh[] meshes = m_faceSelection.Meshes.OrderBy(m => m == m_faceSelection.LastMesh).ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Face> faces = m_faceSelection.GetFaces(mesh).ToArray();
                for (int i = 0; i < faces.Count; ++i)
                {
                    m_faceSelection.Remove(faces[i]);
                }

                MeshState meshState = state.State[mesh];
                mesh.RebuildWithPositionsAndFaces(meshState.Positions, meshState.Faces);

                mesh.ToMesh();
                mesh.Refresh();

                for (int i = 0; i < faces.Count; ++i)
                {
                    m_faceSelection.Add(mesh, faces[i]);
                }
            }

            m_faceSelection.EndChange();
        }
    }
}


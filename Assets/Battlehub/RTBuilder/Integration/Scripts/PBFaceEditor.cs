using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace Battlehub.ProBuilderIntegration
{
    public class PBFaceEditor :  PBBaseEditor
    {
        private PBFaceSelection m_faceSelection;

        private int[][] m_initialIndexes;
        private Vector3[][] m_initialPositions;
        private Vector3 m_initialPostion;
        private Quaternion m_initialRotation;
        
        public override bool HasSelection
        {
            get { return m_faceSelection.FacesCount > 0; }
        }

        public override Vector3 Position
        {
            get { return CenterMode ? m_faceSelection.CenterOfMass : m_faceSelection.LastPosition; }
            set { MoveTo(value); }
        }
               
        public override Vector3 Normal
        {
            get { return (CenterMode && m_faceSelection.FacesCount > 1) ? Vector3.forward : m_faceSelection.LastNormal; }
        }

        public override GameObject Target
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

        public override void Hover(Camera camera, Vector3 pointer)
        {

        }

        public override void Extrude(float distance)
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

                mesh.Extrude(faces, ExtrudeMethod.FaceNormal, distance);
                
                for(int i = 0; i < faces.Count; ++i)
                {
                    m_faceSelection.Add(mesh, faces[i]);
                }

                mesh.ToMesh();
                mesh.Refresh();
            }

            m_faceSelection.EndChange();

            if(distance != 0.0f)
            {
                m_faceSelection.Synchronize(
                    m_faceSelection.GetCenterOfMass(),
                    m_faceSelection.LastPosition + m_faceSelection.LastNormal * distance);
            }
        }

        public override void Delete()
        {
            MeshSelection selection = new MeshSelection();
            ProBuilderMesh[] meshes = m_faceSelection.Meshes.OrderBy(m => m == m_faceSelection.LastMesh).ToArray();
            m_faceSelection.BeginChange();

            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Face> faces = m_faceSelection.GetFaces(mesh).ToArray();
                mesh.DeleteFaces(faces);
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
                m_faceSelection.LastPosition + offset);
        }

        public override void BeginRotate(Quaternion initialRotation)
        {
            m_initialPostion = m_faceSelection.LastPosition;
            m_initialPositions = new Vector3[m_faceSelection.MeshesCount][];
            m_initialIndexes = new int[m_faceSelection.MeshesCount][];
            m_initialRotation = Quaternion.Inverse(initialRotation);

            int meshIndex = 0;
            IEnumerable<ProBuilderMesh> meshes = m_faceSelection.Meshes;
            foreach (ProBuilderMesh mesh in meshes)
            {
                int[] indexes = m_faceSelection.GetIndexes(mesh).ToArray();
                m_initialIndexes[meshIndex] = mesh.GetCoincidentVertices(m_faceSelection.GetIndexes(mesh)).ToArray();
                m_initialPositions[meshIndex] = mesh.GetVertices(m_initialIndexes[meshIndex]).Select(v => mesh.transform.TransformPoint(v.position)).ToArray();
                meshIndex++;
            }
        }

        public override void EndRotate()
        {
            m_initialPositions = null;
            m_initialIndexes = null;
        }

        public override void Rotate(Quaternion rotation)
        {
            Vector3 center = Position;
            int meshIndex = 0;
            IEnumerable<ProBuilderMesh> meshes = m_faceSelection.Meshes;
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Vector3> positions = mesh.positions.ToArray();
                Vector3[] initialPositions = m_initialPositions[meshIndex];
                int[] indexes = m_initialIndexes[meshIndex];

                for (int i = 0; i < initialPositions.Length; ++i)
                {
                    Vector3 position = initialPositions[i];
                    position = center + rotation * m_initialRotation * (position - center);
                    position = mesh.transform.InverseTransformPoint(position);
                    positions[indexes[i]] = position;
                }

                mesh.positions = positions;
                mesh.Refresh();
                mesh.ToMesh();
                meshIndex++;
            }

            m_faceSelection.Synchronize(
                m_faceSelection.CenterOfMass,
                center + rotation * m_initialRotation * (m_initialPostion - center));
        }


        public override void BeginScale()
        {
            m_initialPostion = m_faceSelection.LastPosition;

            m_initialPositions = new Vector3[m_faceSelection.MeshesCount][];
            m_initialIndexes = new int[m_faceSelection.MeshesCount][];

            int meshIndex = 0;
            IEnumerable<ProBuilderMesh> meshes = m_faceSelection.Meshes;
            foreach (ProBuilderMesh mesh in meshes)
            {
                m_initialIndexes[meshIndex] = mesh.GetCoincidentVertices(m_faceSelection.GetIndexes(mesh)).ToArray();
                m_initialPositions[meshIndex] = mesh.GetVertices(m_initialIndexes[meshIndex]).Select(v => mesh.transform.TransformPoint(v.position)).ToArray();
                meshIndex++;
            }
        }

        public override void EndScale()
        {
            m_initialPositions = null;
            m_initialIndexes = null;
        }

        public override void Scale(Vector3 scale, Quaternion rotation)
        {
            Vector3 center = Position;
            int meshIndex = 0;
            IEnumerable<ProBuilderMesh> meshes = m_faceSelection.Meshes;
            
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Vector3> positions = mesh.positions.ToArray();
                Vector3[] initialPositions = m_initialPositions[meshIndex];
                int[] indexes = m_initialIndexes[meshIndex];

                for (int i = 0; i < initialPositions.Length; ++i)
                {
                    Vector3 position = initialPositions[i];
                    position = center + rotation * Vector3.Scale(Quaternion.Inverse(rotation) * (position - center), scale);
                    position = mesh.transform.InverseTransformPoint(position);
                    positions[indexes[i]] = position;
                }

                mesh.positions = positions;
                mesh.Refresh();
                mesh.ToMesh();
                meshIndex++;
            }

            m_faceSelection.Synchronize(
                m_faceSelection.CenterOfMass,
                center + rotation * Vector3.Scale(Quaternion.Inverse(rotation) * (m_initialPostion - center), scale));
        }

        public override MeshSelection Select(Camera camera, Vector3 pointer, bool shift)
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
                        selection.UnselectedFaces.Add(result.mesh, new[] { result.face });
                    }
                    else
                    {
                        selection = ReadSelection();
                        selection.UnselectedFaces[result.mesh] = selection.UnselectedFaces[result.mesh].Where(f => f != result.face).ToArray();
                        selection.SelectedFaces.Add(result.mesh, new[] { result.face });
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
                    selection.SelectedFaces.Add(result.mesh, new[] { result.face });
                }
            }
            else
            {
                if (!shift)
                {
                    selection = ReadSelection();
                    if (selection.UnselectedFaces.Count == 0)
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
                    selection.UnselectedFaces.Add(mesh, faces.ToArray());
                }
            }
            return selection;
        }

        public override MeshSelection Select(Camera camera, Rect rect, GameObject[] gameObjects, MeshEditorSelectionMode mode)
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

                    selection.SelectedFaces.Add(mesh, notSelected);
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
                    selection.UnselectedFaces.Add(mesh, selected);
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

                    selection.UnselectedFaces.Add(mesh, selected);
                    selection.SelectedFaces.Add(mesh, notSelected);
                }
            }

            m_faceSelection.EndChange();

            if(selection.SelectedFaces.Count == 0 && selection.UnselectedFaces.Count == 0)
            {
                selection = null;
            }

            return selection;
        }

        public override MeshSelection Select(Material material)
        {
            MeshSelection selection = IMeshEditorExt.Select(material);

            m_faceSelection.BeginChange();
            foreach (KeyValuePair<ProBuilderMesh, IList<Face>> kvp in selection.SelectedFaces.ToArray())
            {
                IList<Face> faces = kvp.Value;
                for(int i = faces.Count - 1; i >= 0; i--)
                {
                    Face face = faces[i];
                    if(m_faceSelection.IsSelected(face))
                    {
                        faces.Remove(face);
                    }
                    else
                    {
                        m_faceSelection.Add(kvp.Key, face);
                    }
                }

                if(faces.Count == 0)
                {
                    selection.SelectedFaces.Remove(kvp.Key);
                }
            }
            m_faceSelection.EndChange();
            if(selection.SelectedFaces.Count == 0)
            {
                return null;
            }
            return selection;
        }

        public override MeshSelection Unselect(Material material)
        {
            MeshSelection selection = IMeshEditorExt.Select(material);
            selection.Invert();

            m_faceSelection.BeginChange();
            foreach (KeyValuePair<ProBuilderMesh, IList<Face>> kvp in selection.UnselectedFaces.ToArray())
            {
                IList<Face> faces = kvp.Value;
                for (int i = faces.Count - 1; i >= 0; i--)
                {
                    Face face = faces[i];
                    if (!m_faceSelection.IsSelected(face))
                    {
                        faces.Remove(face);
                    }
                    else
                    {
                        m_faceSelection.Remove(face);
                    }
                }

                if (faces.Count == 0)
                {
                    selection.UnselectedFaces.Remove(kvp.Key);
                }
            }
            m_faceSelection.EndChange();
            if (selection.UnselectedFaces.Count == 0)
            {
                return null;
            }
            return selection;
        }

        public override MeshSelection GetSelection()
        {
            MeshSelection selection = new MeshSelection();

            foreach (ProBuilderMesh mesh in m_faceSelection.Meshes)
            {
                selection.SelectedFaces.Add(mesh, m_faceSelection.GetFaces(mesh).ToArray());
            }
            if (selection.SelectedFaces.Count > 0)
            {
                return selection;
            }
            return null;
        }

        public override MeshSelection ClearSelection()
        {
            MeshSelection meshSelection = null;
            if (m_faceSelection != null)
            {
                meshSelection = ReadSelection();
                m_faceSelection.Clear();
            }
            return meshSelection;
        }

        public override void RollbackSelection(MeshSelection selection)
        {
            m_faceSelection.BeginChange();

            foreach (KeyValuePair<ProBuilderMesh, IList<Face>> kvp in selection.SelectedFaces)
            {
                foreach (Face face in kvp.Value)
                {
                    m_faceSelection.Remove(face);
                }
            }

            foreach (KeyValuePair<ProBuilderMesh, IList<Face>> kvp in selection.UnselectedFaces)
            {
                ProBuilderMesh mesh = kvp.Key;
                foreach (Face face in kvp.Value)
                {
                    m_faceSelection.Add(mesh, face);
                }
            }

            m_faceSelection.EndChange();
        }

        public override void ApplySelection(MeshSelection selection)
        {
            m_faceSelection.BeginChange();

            foreach (KeyValuePair<ProBuilderMesh, IList<Face>> kvp in selection.UnselectedFaces)
            {
                foreach (Face face in kvp.Value)
                {
                    m_faceSelection.Remove(face);
                }
            }

            foreach (KeyValuePair<ProBuilderMesh, IList<Face>> kvp in selection.SelectedFaces)
            {
                ProBuilderMesh mesh = kvp.Key;
                foreach (Face face in kvp.Value)
                {
                    m_faceSelection.Add(mesh, face);
                }
            }

            m_faceSelection.EndChange();
        }

        public override MeshEditorState GetState()
        {
            MeshEditorState state = new MeshEditorState();
            ProBuilderMesh[] meshes = m_faceSelection.Meshes.OrderBy(m => m == m_faceSelection.LastMesh).ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                state.State.Add(mesh, new MeshState(mesh.positions.ToArray(), mesh.faces.ToArray()));
            }
            return state;
        }

        public override void SetState(MeshEditorState state)
        {
            m_faceSelection.BeginChange();

            //ProBuilderMesh[] meshes = m_faceSelection.Meshes.OrderBy(m => m == m_faceSelection.LastMesh).ToArray();
            ProBuilderMesh[] meshes = state.State.Keys.ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Face> faces = m_faceSelection.GetFaces(mesh).ToArray();
                for (int i = 0; i < faces.Count; ++i)
                {
                    m_faceSelection.Remove(faces[i]);
                }

                MeshState meshState = state.State[mesh];
                mesh.RebuildWithPositionsAndFaces(meshState.Positions, meshState.Faces.Select(f => f.ToFace()));

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


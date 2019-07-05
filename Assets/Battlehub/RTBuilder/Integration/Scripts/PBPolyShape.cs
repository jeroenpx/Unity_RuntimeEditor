﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace Battlehub.ProBuilderIntegration
{
    public class PBPolyShape : MonoBehaviour
    {
        private PBPolyShapeSelection m_selection;
        private ProBuilderMesh m_targetMesh;
        private PBMesh m_target;
        private bool m_isEditing;
        public bool IsEditing
        {
            get { return m_isEditing; }
            set
            {
                if (m_isEditing != value)
                {
                    m_isEditing = value;
                    if (m_isEditing)
                    {
                        BeginEdit();
                    }
                    else
                    {
                        EndEdit();
                    }
                }
            }
        }

        public int VertexCount
        {
            get { return m_selection.Positions.Count; }
        }

        private int m_stage;
        public int Stage
        {
            get { return m_stage; }
            set { m_stage = value; }
        }

        public IList<Vector3> Positions
        {
            get { return m_selection.Positions; }
            set
            {
                m_selection.Clear();
                if (value != null)
                {
                    for (int i = 0; i < value.Count; ++i)
                    {
                        m_selection.Add(value[i]);
                    }
                }
            }
        }

        public int SelectedIndex
        {
            get { return m_selection.SelectedIndex; }
            set
            {
                m_selection.Unselect();
                if (value >= 0)
                {
                    m_selection.Select(value);
                }
            }
        }

        public Vector3 SelectedPosition
        {
            get { return m_selection.Positions[SelectedIndex]; }
            set
            {
                IList<Vector3> positions = m_selection.Positions;
                positions[m_selection.SelectedIndex] = value;
                Refresh();
            }
        }

        private void Awake()
        {
            m_target = GetComponent<PBMesh>();
            if(!m_target)
            {
                m_target = gameObject.AddComponent<PBMesh>();
            }

            m_targetMesh = m_target.GetComponent<ProBuilderMesh>();
        }

        private void OnDestroy()
        {
            EndEdit();
        }

        private void BeginEdit()
        {
            m_selection = gameObject.GetComponent<PBPolyShapeSelection>();
            if(m_selection == null)
            {
                m_selection = gameObject.AddComponent<PBPolyShapeSelection>();
            }
            m_selection.enabled = true;

            if(m_selection.Positions.Count == 0)
            {
                m_selection.Add(Vector3.zero);
            }
            
            m_isEditing = true;
        }

        private void EndEdit()
        {
            if(m_selection != null)
            {
                m_selection.enabled = false;
            }
            m_isEditing = false;
        }

        public void AddVertexWorld(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);
            AddVertex(position);
        }

        private void AddVertex(Vector3 position)
        {
            if(!m_isEditing)
            {
                BeginEdit();
            }

            m_selection.Add(position);
        }

        public bool Click(Camera camera, Vector3 pointer)
        {
            if(!m_isEditing)
            {
                return false;
            }

            SceneSelection selection = new SceneSelection();
            float result = PBUtility.PickVertex(camera, pointer, 20, m_selection.Transform, m_selection.Positions,  ref selection);

            if (result != Mathf.Infinity)
            {
                if(m_selection.Positions.Count >= 3)
                {
                    m_selection.Unselect();
                    m_selection.Select(selection.vertex);
                    return true;
                }
            }
            else
            {
                if(Stage == 0)
                {
                    Ray ray = camera.ScreenPointToRay(pointer);
                    float enter;

                    Plane plane = new Plane(m_selection.transform.up, m_selection.transform.position);
                    if (plane.Raycast(ray, out enter))
                    {
                        Vector3 position = ray.GetPoint(enter);
                        position = m_selection.Transform.InverseTransformPoint(position);
                        m_selection.Add(position);
                    }

                    m_targetMesh.CreateShapeFromPolygon(m_selection.Positions, 0.001f, false);
                }
            }

            return false;
        }

        public void Refresh()
        {
            m_targetMesh.CreateShapeFromPolygon(m_selection.Positions, 0.001f, false);
            m_selection.Refersh();
        }

        public MeshEditorState GetState()
        {
            MeshEditorState state = new MeshEditorState();
            state.State.Add(m_targetMesh, new MeshState(m_targetMesh.positions, m_targetMesh.faces));
            return state;
        }

        public void SetState(MeshEditorState state)
        {
            ProBuilderMesh[] meshes = state.State.Keys.ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                MeshState meshState = state.State[mesh];
                mesh.RebuildWithPositionsAndFaces(meshState.Positions, meshState.Faces.Select(f => f.ToFace()));

                mesh.ToMesh();
                mesh.Refresh();
            }
        }
    }
}


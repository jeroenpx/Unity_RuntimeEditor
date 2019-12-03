using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Battlehub.RTCommon
{
    public class VRTracker : MonoBehaviour
    {
        private GameObject m_leftHandModel;
        private GameObject m_rightHandModel;
        private List<XRNodeState> nodeStates = new List<XRNodeState>();
        private Vector3 m_headPos;

        private void Start()
        {
            InputTracking.nodeAdded += OnNodeAdded;
            InputTracking.nodeRemoved += OnNodeRemoved;

            InputTracking.trackingAcquired += OnTrackingAquired;
            InputTracking.trackingLost += OnTrackingLost;
        }

        private void OnDestroy()
        {
            InputTracking.nodeAdded -= OnNodeAdded;
            InputTracking.nodeRemoved -= OnNodeRemoved;

            InputTracking.trackingAcquired -= OnTrackingAquired;
            InputTracking.trackingLost -= OnTrackingLost;
        }

        private GameObject CreateModel(Color color, string name)
        {
            GameObject root = new GameObject();
            root.name = name;

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.transform.SetParent(root.transform, false);
            go.transform.localScale = Vector3.one * 0.05f;
            go.transform.localRotation = Quaternion.AngleAxis(90, Vector3.right);
            go.transform.localPosition = Vector3.forward * -0.09f;
            go.name = "model";

            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            Material m = renderer.material;
            m.color = color;
            renderer.sharedMaterial = m;

            GameObject lineRendererGo = new GameObject("Line Renderer");
            lineRendererGo.transform.SetParent(root.transform, false);
            lineRendererGo.transform.localRotation = Quaternion.AngleAxis(45, Vector3.right);
            lineRendererGo.transform.localPosition = Vector3.forward * -0.04f;
            LineRenderer lineRenderer = lineRendererGo.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.forward * 10 });
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            lineRenderer.startWidth = 0.004f;
            lineRenderer.endWidth = 0.004f;

            return root;
        }

        private void OnNodeAdded(XRNodeState state)
        {
            Debug.Log("Node added: " + state.nodeType);
        }

        private void OnNodeRemoved(XRNodeState state)
        {
            Debug.Log("Node removed " + state.nodeType);
        }

        private void OnTrackingLost(XRNodeState state)
        {
            Debug.Log("Tracking Lost " + state.nodeType);

            if (state.nodeType == XRNode.LeftHand)
            {
                Destroy(m_leftHandModel);
            }
            else if (state.nodeType == XRNode.RightHand)
            {
                Destroy(m_rightHandModel);
            }
        }

        private void OnTrackingAquired(XRNodeState state)
        {
            Debug.Log("Tracking Aquired " + state.nodeType);

            if (state.nodeType == XRNode.LeftHand)
            {
                m_leftHandModel = CreateModel(Color.red, "Left Hand");
                CreateVRInputDevice(m_leftHandModel, state);
            }
            else if (state.nodeType == XRNode.RightHand)
            {
                m_rightHandModel = CreateModel(Color.yellow, "Right Hand");
                CreateVRInputDevice(m_rightHandModel, state);
            }
        }

        private static void CreateVRInputDevice(GameObject go, XRNodeState state)
        {
            if (go != null)
            {
                InputDevice device = InputDevices.GetDeviceAtXRNode(state.nodeType);
                if (device != null)
                {
                    VRInputDevice vrInput = go.AddComponent<VRInputDevice>();
                    vrInput.Device = device;
                }
            }
        }

        private void Update()
        {
            nodeStates.Clear();

            Transform ht = Camera.main.transform;

            InputTracking.GetNodeStates(nodeStates);

            foreach (XRNodeState state in nodeStates)
            {
                Vector3 pos;
                Quaternion rot;
                if (state.TryGetPosition(out pos) && state.TryGetRotation(out rot))
                {
                    if (state.nodeType == XRNode.Head)
                    {
                        m_headPos = pos;
                    }
                    if (state.nodeType == XRNode.LeftHand)
                    {
                        if(m_leftHandModel != null)
                        {
                            m_leftHandModel.transform.position = ht.position - m_headPos + pos;
                            m_leftHandModel.transform.rotation = rot;
                        }
                        
                    }
                    else if (state.nodeType == XRNode.RightHand)
                    {
                        if(m_rightHandModel != null)
                        {
                            m_rightHandModel.transform.position = ht.position - m_headPos + pos;
                            m_rightHandModel.transform.rotation = rot;
                        }
                    }
                }
            }
        }
    }
}



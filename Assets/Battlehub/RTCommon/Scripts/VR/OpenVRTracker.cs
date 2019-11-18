using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class OpenVRTracker : MonoBehaviour
{
    private XRNodeState m_leftHand;
    private XRNodeState m_rightHand;

    private GameObject m_leftHandModel;
    private GameObject m_rightHandModel;

    // Start is called before the first frame update
    void Start()
    {
        InputTracking.nodeAdded += OnNodeAdded;
        InputTracking.nodeRemoved += OnNodeRemoved;

        InputTracking.trackingAcquired += OnTrackingAquired;
        InputTracking.trackingLost += OnTrackingLost;
        m_leftHandModel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        m_leftHandModel.transform.localScale = Vector3.one * 0.25f;
        m_rightHandModel = GameObject.CreatePrimitive(PrimitiveType.Cube);

    }

    private void OnNodeRemoved(XRNodeState obj)
    {
        Debug.Log("Node removed " + obj.nodeType);
    }

    private void OnTrackingLost(XRNodeState obj)
    {
        Debug.Log("Tracking Lost " + obj.nodeType);
    }

    private void OnTrackingAquired(XRNodeState obj)
    {
        Debug.Log("Tracking Aquired " + obj.nodeType);
    }

   
    private Vector3 m_headPos;
    private List<XRNodeState> nodeStates = new List<XRNodeState>();

    private void Update()
    {
        nodeStates.Clear();
        InputTracking.GetNodeStates(nodeStates);

        foreach (XRNodeState state in nodeStates)
        {

            if (state.nodeType == XRNode.LeftHand)
            {
                Vector3 pos;
                if (state.TryGetPosition(out pos))
                {
                    m_leftHandModel.transform.position = Camera.main.transform.position + Vector3.down + Vector3.forward + pos;
                }
            }
        }

    }

    private void OnNodeAdded(XRNodeState obj)
    {
        if (obj.nodeType == XRNode.LeftHand)
        {
            m_leftHand = obj;
            Debug.Log(obj.nodeType);
        }

        else if (obj.nodeType == XRNode.RightHand)
        {
            m_rightHand = obj;
            Debug.Log(obj.nodeType);
        }


    }
}

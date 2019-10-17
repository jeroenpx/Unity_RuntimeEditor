using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderQuads : MonoBehaviour
{
    [SerializeField]
    private Mesh m_quad = null;

    [SerializeField]
    private Material m_material = null;

    private int m_rows = 1000;
    private int m_cols = 100;
    private int m_batchSize = 512;

    private Matrix4x4[] m_matrices;

    [SerializeField]
    private Camera m_camera = null;

    private CommandBuffer m_commandBuffer;

    [SerializeField]
    private AnimationClip clip = null;

    private void Awake()
    {
        //clip.legacy = true;

        // create a curve to move the GameObject and assign to the clip
        //Keyframe[] keys;
        //keys = new Keyframe[3];
        //keys[0] = new Keyframe(0.0f, 0.0f);
        //keys[1] = new Keyframe(1.0f, 1.5f);
        //keys[2] = new Keyframe(2.0f, 0.0f);
        //AnimationCurve curve = new AnimationCurve(keys);
        //clip.SetCurve("", typeof(Transform), "localPosition.x", curve);

       // return;

        m_matrices = new Matrix4x4[m_batchSize];

        m_camera.enabled = false;

        m_commandBuffer = new CommandBuffer();
        m_camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, m_commandBuffer);
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.T))
        {
            m_commandBuffer.Clear();

            int index = 0;
            for (int i = 0; i < m_rows; ++i)
            {
                for (int j = 0; j < m_cols; ++j)
                {
                    m_matrices[index] = Matrix4x4.TRS(
                        new Vector3(i / 10.0f, j / 10.0f, 0 ),
                        Quaternion.Euler(0, 0, 45),
                        Vector3.one * 0.05f);

                    index++;
                    if(index == m_batchSize)
                    {    
                        index = 0;
                        m_commandBuffer.DrawMeshInstanced(m_quad, 0, m_material, 0, m_matrices, m_batchSize);
                        
                    }
                }
            }

            if(0 < index && index < m_batchSize)
            {
                m_commandBuffer.DrawMeshInstanced(m_quad, 0, m_material, 0, m_matrices, index);
            }

            m_camera.enabled = true;
            m_camera.Render();
            m_camera.enabled = false;
        }
    }

  
}

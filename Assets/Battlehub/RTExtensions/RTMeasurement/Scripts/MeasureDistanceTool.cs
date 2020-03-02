using Battlehub.RTCommon;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTMeasurement
{
    public class MeasureDistanceTool : MeasureTool
    {
        [SerializeField]
        private bool m_metric = true;

        protected List<Vector3> m_points = new List<Vector3>();
        
        protected override void UpdateOverride()
        {
            base.UpdateOverride();

            RaycastHit hit;
            if (Physics.Raycast(Window.Pointer, out hit))
            {
                Vector3 point = SnapToVertex(hit.point);

                if (Editor.Input.GetPointerDown(0))
                {
                    if (m_points.Count == 2)
                    {
                        m_points.Clear();
                    }
                    else
                    {
                        m_points.Add(point);
                        m_points.Add(point);
                    }

                    Renderer.Vertices = m_points.ToArray();
                    Renderer.Refresh();
                }
                else
                {
                    PointerRenderer.transform.position = point;

                    if(m_points.Count == 2)
                    {
                        m_points[1] = point;
                        Renderer.Vertices = m_points.ToArray();
                        Renderer.Refresh(true);
                    }

                    if (Output != null && Canvas != null)
                    {
                        Camera worldCamera = Canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Canvas.worldCamera;

                        Vector2 position;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)Output.transform.parent, Editor.Input.GetPointerXY(0), worldCamera, out position);
                        Output.transform.localPosition = position;
                        if(m_points.Count == 2)
                        {
                            float mag = (m_points[1] - m_points[0]).magnitude;
                            Output.text = m_metric ? mag.ToString("F2") : UnitsConverter.MetersToFeetInches(mag);
                        }
                        else
                        {
                            Output.text = "";
                        }
                    }
                }
            }
           
        }

      
    }
}


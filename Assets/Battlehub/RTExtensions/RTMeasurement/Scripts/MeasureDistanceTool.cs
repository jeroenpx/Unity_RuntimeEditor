using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTMeasurement
{
    public class MeasureDistanceTool : MeasureTool
    {
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
                        Vector2 position;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)Output.transform.parent, Editor.Input.GetPointerXY(0), Canvas.worldCamera, out position);
                        Output.transform.localPosition = position;
                        if(m_points.Count == 2)
                        {
                            Output.text = (m_points[1] - m_points[0]).magnitude.ToString("F2");
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


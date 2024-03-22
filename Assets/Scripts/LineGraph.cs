using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LineGraph : Graphic
{
    public List<Vector2> dataPoints;

    public float xOffset = 50f;
    public float yOffset = 50f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (dataPoints == null || dataPoints.Count == 0)
            return;

        Rect graphRect = GetPixelAdjustedRect();

        float graphWidth = graphRect.width;
        float graphHeight = graphRect.height;

        float xStep = graphWidth / (dataPoints.Count - 1);

        for (int i = 0; i < dataPoints.Count; i++)
        {
            Vector2 dataPoint = dataPoints[i];

            float xPos = graphRect.x + i * xStep;
            float yPos = graphRect.y + dataPoint.y / 100f * graphHeight;

            DrawPoint(vh, new Vector2(xPos, yPos));

            if (i > 0)
            {
                Vector2 prevDataPoint = dataPoints[i - 1];
                float prevXPos = graphRect.x + (i - 1) * xStep;
                float prevYPos = graphRect.y + prevDataPoint.y / 100f * graphHeight;

                DrawLine(vh, new Vector2(prevXPos, prevYPos), new Vector2(xPos, yPos));
            }
        }
    }
    
    public float lineThickness = 20f;
    public float pointSize = 20f;

    private void DrawPoint(VertexHelper vh, Vector2 position)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        float halfSize = pointSize * 0.5f;

        UIVertex[] vertices = new UIVertex[4];
        vertices[0] = vertex;
        vertices[0].position = position + new Vector2(-halfSize, -halfSize);
        vertices[1] = vertex;
        vertices[1].position = position + new Vector2(-halfSize, halfSize);
        vertices[2] = vertex;
        vertices[2].position = position + new Vector2(halfSize, halfSize);
        vertices[3] = vertex;
        vertices[3].position = position + new Vector2(halfSize, -halfSize);

        vh.AddUIVertexQuad(vertices);
    }
    
    private void DrawLine(VertexHelper vh, Vector2 startPos, Vector2 endPos)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        // Calculate the direction and length of the line
        Vector2 direction = (endPos - startPos).normalized;

        // Calculate the perpendicular direction for line width
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        // Calculate the vertices for the quad to represent the line
        UIVertex[] vertices = new UIVertex[4];
        vertices[0] = vertex;
        vertices[0].position = startPos - perpendicular * (lineThickness * 0.5f);
        vertices[1] = vertex;
        vertices[1].position = endPos - perpendicular * (lineThickness * 0.5f);
        vertices[2] = vertex;
        vertices[2].position = endPos + perpendicular * (lineThickness * 0.5f);
        vertices[3] = vertex;
        vertices[3].position = startPos + perpendicular * (lineThickness * 0.5f);

        // Add the quad to the VertexHelper
        vh.AddUIVertexQuad(vertices);
    }

}
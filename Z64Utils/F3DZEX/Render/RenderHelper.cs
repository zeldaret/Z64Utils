using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace F3DZEX.Render
{
    public static class RenderHelper
    {
        public static void DrawGrid(SimpleVertexDrawer drawer)
        {
            float oldLineWidth = GL.GetFloat(GetPName.LineWidth);
            GL.LineWidth(1);

            drawer.SendColor(Color.FromArgb(0x7F, 0, 0, 0));
            drawer.Draw(PrimitiveType.Lines);

            GL.LineWidth(oldLineWidth);
        }
        public static float[] GenerateGridVertices(float gridScale, int lineCount, bool cube)
        {
            int stride = 3;
            int times = lineCount * lineCount;
            if (cube)
                times *= lineCount;

            float[] vertices = new float[(lineCount * 2 * stride) * times];

            int i = 0;
            if (cube)
            {
                // todo: finish
                for (float z = -gridScale; z < gridScale + 1; z += gridScale / lineCount)
                for (float x = -gridScale; x < gridScale + 1; x += gridScale / lineCount)
                {
                    vertices[i++] = x;
                    vertices[i++] = -gridScale;
                    vertices[i++] = z;

                    vertices[i++] = x;
                    vertices[i++] = gridScale;
                    vertices[i++] = z;
                }
            }
            else
            {
                // X
                for (float x = -gridScale; x < gridScale + 1; x += gridScale / lineCount)
                {
                    vertices[i++] = x;
                    i++;
                    vertices[i++] = -gridScale;

                    vertices[i++] = x;
                    i++;
                    vertices[i++] = gridScale;
                }
                // Z
                for (float z = -gridScale; z < gridScale + 1; z += gridScale / lineCount)
                {
                    vertices[i++] = -gridScale;
                    i++;
                    vertices[i++] = z;

                    vertices[i++] = gridScale;
                    i++;
                    vertices[i++] = z;
                }
            }

            return vertices; ;

        }
        
        public static void DrawAxis(ColoredVertexDrawer drawer)
        {
            float oldLineWidth = GL.GetFloat(GetPName.LineWidth);
            GL.LineWidth(2);

            drawer.Draw(PrimitiveType.Lines);

            GL.LineWidth(oldLineWidth);
        }
        public static float[] GenerateAxisvertices(float gridScale)
        {
            return new float[] { 
                0, 0, 0,                1, 0, 0, 1,
                gridScale/10, 0, 0,     1, 0, 0, 1,

                0, 0, 0,                0, 1, 0, 1,
                0, gridScale/10, 0,     0, 1, 0, 1,

                0, 0, 0,                0, 0, 1, 1,
                0, 0, gridScale/10,     0, 0, 1, 1,
            };
        }
    }
}

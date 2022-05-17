using System;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace OpenGLRayTracing
{
    class MyObject
    {
        int VertexBufferObject;
        int VertexArrayObject;

        Vector3 Ambient;
        Vector3 Diffuse;
        Vector3 Specular;
        float Shine;

        private float[] vertices;

        public MyObject(float[] vert, Vector3 ambient, Vector3 diffuse, Vector3 specular, float shine)
        {
            vertices = vert;
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
            Shine = shine;

            VertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);//vertices
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));//normals
            GL.EnableVertexAttribArray(1);

        }
        public void Move_and_render(ref Matrix4 view, ref Matrix4 projection, ref Matrix4 model)
        {
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);
        }
        public void Unload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteVertexArray(VertexArrayObject);
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using OpenTK.Input;

namespace OpenGLRayTracing
{
    public class Shader
    {
        int handle;
        private bool disposed = false;
        public int Handle { get => handle; set => handle = value; }
        public void SetMatrix4(string name, Matrix4 data)
        {
            int location = GL.GetUniformLocation(Handle, name);

            GL.UniformMatrix4(location, true, ref data);
        }
        public Shader(string vertexPath, string fragmentPath)
        {
            disposed = false;
            int VertexShader;
            int FragmentShader;

            string VertexShaderSource;

            using (StreamReader reader = new StreamReader(vertexPath, Encoding.UTF8))
            {
                VertexShaderSource = reader.ReadToEnd();
            }
            string FragmentShaderSource;

            using (StreamReader reader = new StreamReader(fragmentPath, Encoding.UTF8))
            {
                FragmentShaderSource = reader.ReadToEnd();
            }

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            GL.CompileShader(VertexShader);

            string infoLogVert = GL.GetShaderInfoLog(VertexShader);
            if (infoLogVert != System.String.Empty)
                System.Console.WriteLine(infoLogVert);

            GL.CompileShader(FragmentShader);

            string infoLogFrag = GL.GetShaderInfoLog(FragmentShader);

            if (infoLogFrag != System.String.Empty)
                System.Console.WriteLine(infoLogFrag);

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);
        }
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void Dispose()
        {
            GL.DeleteProgram(Handle);
            disposed = true;
        }

        ~Shader()
        {
            if (!disposed)
                GL.DeleteProgram(Handle);
        }
    }
}

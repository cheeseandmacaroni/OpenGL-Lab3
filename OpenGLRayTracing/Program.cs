using System;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;


namespace OpenGLRayTracing
{
    public class MyScene : GameWindow
    {
        public Shader shader;

        public float[] vertices = {
            -1.0f, -1.0f, 0.0f, //Bottom-left vertex
            1.0f, -1.0f, 0.0f, //Bottom-right vertex
            1.0f, 1.0f, 0.0f,  //Top-right vertex

            -1.0f, -1.0f, 0.0f, //Bottom-left vertex
            -1.0f, 1.0f, 0.0f, //top-left vertex
            1.0f, 1.0f, 0.0f  //Top-right vertex
        };

        public float[] sceneInfo;

        public Cylinder firstCylinder;
        public Cylinder secondCylinder;
        public Cube box;

        int VertexBufferObject;
        int VertexArrayObject;

        struct Camera
        {
            public static Vector3 position = new Vector3(0.0f, 0.2f, -0.9f);
            public static Vector3 view = new Vector3(0.0f, 0.0f, 1.0f);
            public static Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
            public static Vector3 side = new Vector3(1.0f, 0.0f, 0.0f);
            public static Vector2 scale = new Vector2(1.0f);
        };

        int cameraPosLoc;
        int cameraViewLoc;
        int cameraUpLoc;
        int cameraSideLoc;
        int cameraScaleLoc;
        int primitiveCountLoc;
        int sceneInfoLoc;

        public MyScene(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        protected void MoveVertices(int offset, int size, Matrix4 model)
        {
            for(int i = offset; i < offset + size; ++i)
            {
                Vector4 res = new Vector4(sceneInfo[3 * i], sceneInfo[3 * i + 1], sceneInfo[3 * i + 2], 1.0f);
                res *= model;
                sceneInfo[3 * i] = res.X;
                sceneInfo[3 * i + 1] = res.Y;
                sceneInfo[3 * i + 2] = res.Z;
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                Close();
            }
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            shader.Use();

            Matrix4 model = Matrix4.CreateRotationY(MathHelper.DegreesToRadians((float)e.Time * 10));
            //MoveVertices(0, firstCylinder.vertices.Length / 3, model);

            GL.Uniform3(cameraPosLoc, Camera.position);
            GL.Uniform3(cameraViewLoc, Camera.view);
            GL.Uniform3(cameraUpLoc, Camera.up);
            GL.Uniform3(cameraSideLoc, Camera.side);
            GL.Uniform2(cameraScaleLoc, Camera.scale);
            GL.Uniform1(primitiveCountLoc, sceneInfo.Length / 9);
            GL.Uniform1(sceneInfoLoc, sceneInfo.Length, sceneInfo);

            GL.BindVertexArray(VertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);

            SwapBuffers();
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            shader = new Shader("../../Shader.vert", "../../Shader.frag");

            cameraPosLoc = GL.GetUniformLocation(shader.Handle, "uniformCamera.position");
            cameraViewLoc = GL.GetUniformLocation(shader.Handle, "uniformCamera.view");
            cameraUpLoc = GL.GetUniformLocation(shader.Handle, "uniformCamera.up");
            cameraSideLoc = GL.GetUniformLocation(shader.Handle, "uniformCamera.side");
            cameraScaleLoc = GL.GetUniformLocation(shader.Handle, "uniformCamera.scale");
            primitiveCountLoc = GL.GetUniformLocation(shader.Handle, "primitiveCount");
            sceneInfoLoc = GL.GetUniformLocation(shader.Handle, "sceneInfo");


            box = new Cube(1.0f);
            secondCylinder = firstCylinder = new Cylinder(0.2f, 0.6f, 0.0f, 10);

            sceneInfo = new float[firstCylinder.vertices.Length + secondCylinder.vertices.Length + box.vertices.Length];

            firstCylinder.vertices.CopyTo(sceneInfo, 0);
            secondCylinder.vertices.CopyTo(sceneInfo, firstCylinder.vertices.Length);
            box.vertices.CopyTo(sceneInfo, secondCylinder.vertices.Length + firstCylinder.vertices.Length);

            Matrix4 model1 = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(-40f)) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-40f)) * Matrix4.CreateTranslation(-0.3f, 0.0f, 0.4f);
            Matrix4 model2 = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(20f)) * Matrix4.CreateTranslation(new Vector3(0.4f, 0.0f, 0.5f));
            Matrix4 model3 = Matrix4.CreateTranslation(new Vector3(0.0f, 0.0f, 0.0f));

            MoveVertices(0, firstCylinder.vertices.Length / 3, model1);
            MoveVertices(firstCylinder.vertices.Length / 3, secondCylinder.vertices.Length / 3, model2);
            MoveVertices((secondCylinder.vertices.Length + firstCylinder.vertices.Length) / 3, box.vertices.Length / 3, model3);

            VertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);//vertices
            GL.EnableVertexAttribArray(0);

            shader.Use();

            GL.ClearColor(0.0f, 0f, 0f, 0f);
            GL.Enable(EnableCap.DepthTest);
        }
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
        }


        //////////////////////////////////////////////////////////////////////////////////

        static void Main()
        {
            MyScene scene = new MyScene(650, 650, "Window");
            scene.Run();
        }
    }
}


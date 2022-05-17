using System;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System.Collections.Generic;

namespace OpenGLRayTracing
{
    public class Cylinder
    {
        float[] _2Dsircle;
        public float[] vertices;
        float radius;
        float height;
        float offset;
        int sectorCount;

        public float[] Sircle { get => _2Dsircle; set => _2Dsircle = value; }

        protected void GenerateCircleVertices()
        {
            const float PI = 3.1415926f;
            float sectorStep = 2 * PI / sectorCount;
            float sectorAngle;  // radian
            List<float> result = new List<float>();
            for (int i = 0; i < sectorCount; ++i)
            {
                sectorAngle = i * sectorStep;
                result.Add((float)Math.Cos(sectorAngle)); //x
                result.Add((float)Math.Sin(sectorAngle)); //y
                result.Add(0); //z
            }
            Sircle = result.ToArray();
        }

        public Cylinder(float _radius, float _height, float _offset, int _sectorCount)
        {
            radius = _radius;
            height = _height;
            offset = _offset;
            sectorCount = _sectorCount;
            List<float> verticesList = new List<float>();
            List<float> normalsList = new List<float>();
            GenerateCircleVertices();
            for (int i = 0; i < 2; ++i)
            {
                float h = i * height + offset;
                float nz = -1 + i * 2;


                for (int j = 0, k = 0; j < sectorCount; ++j, k += 3)
                {
                    
                    float ux = Sircle[k];
                    float uy = Sircle[k + 1];
                    float uz = Sircle[k + 2];
                    float ux2 = Sircle[(k + 3) % Sircle.Length];
                    float uy2 = Sircle[(k + 4) % Sircle.Length];
                    float uz2 = Sircle[(k + 5) % Sircle.Length];
                    
                    if (h == offset)
                    {
                        //top and bottom
                        verticesList.Add(0);
                        verticesList.Add(0);
                        verticesList.Add(h);
                        verticesList.Add(ux * radius);
                        verticesList.Add(uy * radius);
                        verticesList.Add(h);
                        verticesList.Add(ux2 * radius);
                        verticesList.Add(uy2 * radius);
                        verticesList.Add(h);
                        //side
                        verticesList.Add(ux2 * radius);
                        verticesList.Add(uy2 * radius);
                        verticesList.Add(h);
                        verticesList.Add(ux * radius);
                        verticesList.Add(uy * radius);
                        verticesList.Add(h);
                        verticesList.Add(ux2 * radius); 
                        verticesList.Add(uy2 * radius);
                        verticesList.Add(height + offset);
                    }
                    else
                    {//top and bottom
                        verticesList.Add(ux * radius);
                        verticesList.Add(uy * radius);
                        verticesList.Add(h);
                        verticesList.Add(0);
                        verticesList.Add(0);
                        verticesList.Add(h);
                        verticesList.Add(ux2 * radius);
                        verticesList.Add(uy2 * radius);
                        verticesList.Add(h);
                        //side
                        verticesList.Add(ux * radius);
                        verticesList.Add(uy * radius);
                        verticesList.Add(h);
                        verticesList.Add(ux2 * radius);
                        verticesList.Add(uy2 * radius);
                        verticesList.Add(h);
                        verticesList.Add(ux * radius); 
                        verticesList.Add(uy * radius);
                        verticesList.Add(offset);
                    }
                }
            }
            vertices = verticesList.ToArray();
        }
    };
}

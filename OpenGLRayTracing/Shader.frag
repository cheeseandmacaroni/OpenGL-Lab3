#version 330 core

const int DIFFUSE = 1;
const int REFLECTION = 2;
const int REFRACTION = 3;

out vec4 FragColor;
in vec3 glPosition;

struct Camera
{
	vec3 position;
	vec3 view;
	vec3 up;
	vec3 side;
	vec2 scale;
};

struct Ray
{
	vec3 origin;
	vec3 direction;
};

struct Light
{
	vec3 position;
};

struct TracingRay
{
	Ray ray;
	float contribution;
	int depth;
};

struct Material
{
	vec3 color;
	vec4 lightCoef;
	float reflectionCoef;
	float refractionCoef;
	int materialType;
};


struct Intersection
{
	float time;
	vec3 point;
	vec3 normal;
	vec3 color;
	// ambient, diffuse and specular coeffs
	vec4 lightCoeffs;
	// 0 - non-reflection, 1 - mirror
	float reflectionCoef;
	float refractionCoef;
	int materialType;
};

uniform Camera uniformCamera;
uniform int primitiveCount;
uniform float sceneInfo[1000];

Light light;
Material materials[8];

void initializeDefaultLightMaterials()
{
	//** LIGHT **//
	light.position = vec3(-0.6f, 0.0f, -0.3f);
	/** MATERIALS **/
	vec4 lightCoefs = vec4(0.3,0.6,0.3,200.0);

	materials[0].color = vec3(0.0, 1.0, 0.0);
	materials[0].lightCoef = vec4(lightCoefs);
	materials[0].reflectionCoef = 0.6;
	materials[0].refractionCoef = 1.05;
	materials[0].materialType = REFRACTION;

	materials[1].color = vec3(1.0, 0.0, 1.0);
	materials[1].lightCoef = vec4(lightCoefs);
	materials[1].reflectionCoef = 0.6;
	materials[1].refractionCoef = 1.0;
	materials[1].materialType = REFLECTION;

	materials[2].color = vec3(1.0, 0.0, 0.0);
	materials[2].lightCoef = vec4(lightCoefs);
	materials[2].reflectionCoef = 0.5;
	materials[2].refractionCoef = 1.0;
	materials[2].materialType = DIFFUSE;
	
	materials[3].color = vec3(0.0, 1.0, 0.0);
	materials[3].lightCoef = vec4(lightCoefs);
	materials[3].reflectionCoef = 0.5;
	materials[3].refractionCoef = 1.0;
	materials[3].materialType = DIFFUSE;

	materials[4].color = vec3(0.0, 0.0, 1.0);
	materials[4].lightCoef = vec4(lightCoefs);
	materials[4].reflectionCoef = 0.5;
	materials[4].refractionCoef = 1.0;
	materials[4].materialType = REFLECTION;
			  
	materials[5].color = vec3(1.0, 1.0, 0.0);
	materials[5].lightCoef = vec4(lightCoefs);
	materials[5].reflectionCoef = 0.5;
	materials[5].refractionCoef = 1.0;
	materials[5].materialType = REFLECTION;

	materials[6].color = vec3(0.0, 1.0, 1.0);
	materials[6].lightCoef = vec4(lightCoefs);
	materials[6].reflectionCoef = 0.5;
	materials[6].refractionCoef = 1.0;
	materials[6].materialType = DIFFUSE;

	materials[7].color = vec3(1.0, 1.0, 1.0);
	materials[7].lightCoef = vec4(lightCoefs);
	materials[7].reflectionCoef = 0.5;
	materials[7].refractionCoef = 1.0;
	materials[7].materialType = DIFFUSE;
}

vec3 Phong (Intersection intersect, Light currLight, float shadowing)
{
	vec3 light = normalize ( currLight.position - intersect.point );
	float diff = max(dot(light, intersect.normal), 0.0);
	vec3 view = normalize(uniformCamera.position - intersect.point);
	vec3 reflected = reflect( -view, intersect.normal );
	float spec = pow(max(dot(reflected, light), 0.0), intersect.lightCoeffs.w);

	vec3 ambient = intersect.lightCoeffs.x * intersect.color;
	vec3 diffuse = intersect.lightCoeffs.y * diff * intersect.color * shadowing;
	vec3 specular = intersect.lightCoeffs.z * spec * intersect.color;
	//return diffuse;
	//return ambient;
	//return specular;
	//return ambient + diffuse + specular;
	return intersect.lightCoeffs.x * intersect.color + intersect.lightCoeffs.y * diff * intersect.color * shadowing +
 intersect.lightCoeffs.z * spec;

}

bool IntersectTriangle (Ray ray, vec3 v1, vec3 v2, vec3 v3, out float time )
{
	time = -1;
	vec3 A = v2 - v1;
	vec3 B = v3 - v1;
	vec3 N = cross(A, B);
	float NdotRayDirection = dot(N, ray.direction);
	if (abs(NdotRayDirection) < 0.001)
	return false;
	float d = dot(N, v1);
	float t = -(dot(N, ray.origin) - d) / NdotRayDirection;
	if (t < 0)
	return false;
	vec3 P = ray.origin + t * ray.direction;
	vec3 C;
	vec3 edge1 = v2 - v1;
	vec3 VP1 = P - v1;
	C = cross(edge1, VP1);
	if (dot(N, C) < 0)
	return false;
	vec3 edge2 = v3 - v2;
	vec3 VP2 = P - v2;
	C = cross(edge2, VP2);
	if (dot(N, C) < 0)
	return false;
	vec3 edge3 = v1 - v3;
	vec3 VP3 = P - v3;
	C = cross(edge3, VP3);
	if (dot(N, C) < 0)
	return false;
	time = t;
	return true;
}

bool Raytrace ( Ray ray, float start, float final, inout Intersection intersect )
{
	bool result = false;
	float test = start;
	intersect.time = final;
	vec3 v1;
	vec3 v2;
	vec3 v3;
	int cylinderPrimitiveCount = (primitiveCount - 12) / 2;
	//calculate intersect with triangles
	for(int i = 0; i < primitiveCount; i++)
	{
	v1 = vec3(sceneInfo[9 * i], sceneInfo[9 * i + 1], sceneInfo[9 * i + 2]);
	v2 = vec3(sceneInfo[9 * i + 3], sceneInfo[9 * i + 4], sceneInfo[9 * i + 5]);
	v3 = vec3(sceneInfo[9 * i + 6], sceneInfo[9 * i + 7], sceneInfo[9 * i + 8]);
		if(IntersectTriangle(ray, v1, v2, v3, test) && test < intersect.time)	
		{
			Material material;
			if(i >= 0 && i < cylinderPrimitiveCount) //first cylinder
				material = materials[0];
			else if(i >= cylinderPrimitiveCount && i < 2 * cylinderPrimitiveCount) //second cylinder
				material = materials[1]; 
			else if(i >= primitiveCount - 12 && i < primitiveCount - 10)
				material = materials[2];
			else if(i >= primitiveCount - 10 && i < primitiveCount - 8)
				material = materials[3];
			else if(i >= primitiveCount - 8 && i < primitiveCount - 6)
				material = materials[4];
			else if(i >= primitiveCount - 6 && i < primitiveCount - 4)
				material = materials[5];
			else if(i >= primitiveCount - 4 && i < primitiveCount - 2)
				material = materials[6];
			else
				material = materials[7];	
			intersect.time = test;
			intersect.point = ray.origin + ray.direction * test;
			intersect.normal = normalize(cross(v1 - v2, v3 - v2));
			intersect.color = material.color;
			intersect.lightCoeffs = material.lightCoef;
			intersect.reflectionCoef = material.reflectionCoef;
			intersect.refractionCoef = material.refractionCoef;
			intersect.materialType = material.materialType;
			result = true;
		}
	}
	return result;
}

float Shadow(Light currLight, Intersection intersect)
{
	// Point is lighted
	float shadowing = 1.0;
	// Vector to the light source
	vec3 direction = normalize(currLight.position - intersect.point);
	// Distance to the light source
	float distanceLight = distance(currLight.position, intersect.point);
	// Generation shadow ray for this light source
	Ray shadowRay = Ray(intersect.point + direction * 0.001f, direction);
	// ...test intersection this ray with each scene object
	Intersection shadowIntersect;
	shadowIntersect.time = 1000000.0f;
	// trace ray from shadow ray begining to light source position
	if(Raytrace(shadowRay, 0, distanceLight, shadowIntersect))
	{
	// this light source is invisible in the intercection point
		shadowing = 0.0;
	}
	return shadowing;
}

Ray GenerateRay(Camera camera)
{
	vec2 coords = glPosition.xy * camera.scale;
	vec3 dir = camera.view + camera.side * coords.x + camera.up * coords.y;
	return Ray(camera.position, normalize(dir));
}

struct Stack
{
	TracingRay rays[100];
	int i;
	void push(TracingRay ray)
	{
		rays[++i] = ray;
	}
	TracingRay pop()
	{
		i--;
		return rays[i + 1];
	}
	bool is_empty()
	{
		return i == -1 ? true : false;
	}
};

Stack stack;

void main ( void )
{
	stack.i = -1;
	float start = 0;
	float final = 1000000.0f;
	initializeDefaultLightMaterials();

	Ray ray = GenerateRay( uniformCamera );
	stack.push(TracingRay(ray, 1, 0));

	vec3 resultColor = vec3(0,0,0);
	while(!stack.is_empty())
	{
		TracingRay tRay = stack.pop();
		ray = tRay.ray;
		if(tRay.depth >= 10)
			break;
		Intersection intersect;
		intersect.time = 1000000.0f;
		if (Raytrace(ray, start, final, intersect))
		{
			if(intersect.materialType == DIFFUSE)
			{
				float shadowing = Shadow(light, intersect);
				resultColor += tRay.contribution * Phong ( intersect, light, shadowing );
			}
			else if(intersect.materialType == REFLECTION)
			{
				if(intersect.reflectionCoef < 1)
				{
					float contribution = tRay.contribution * (1 - intersect.reflectionCoef);
					//float shadowing = Shadow(light, intersect);
					resultColor += contribution * Phong(intersect, light, 1);
				}
				vec3 reflectDirection = reflect(ray.direction, intersect.normal);
				// create reflection ray
				
				float contribution = tRay.contribution * intersect.reflectionCoef;
				TracingRay reflectRay = TracingRay( Ray(intersect.point + reflectDirection * 0.001, reflectDirection), contribution, tRay.depth + 1);
				stack.push(reflectRay);
			}
			else if(intersect.materialType == REFRACTION)
			{
                vec3 refrDir;
                if (dot(ray.direction, intersect.normal) < 0)
                {
                    refrDir = normalize(refract(ray.direction, intersect.normal, intersect.refractionCoef));
					if(intersect.reflectionCoef < 1)
					{
						float contribution = tRay.contribution * (1 - intersect.reflectionCoef);
						//float shadowing = Shadow(light, intersect);
						resultColor += contribution * Phong(intersect, light, 1);
					}
					vec3 reflectDirection = reflect(ray.direction, intersect.normal);
				
					float contribution = tRay.contribution * intersect.reflectionCoef;
					TracingRay reflectRay = TracingRay( Ray(intersect.point + reflectDirection * 0.001, reflectDirection), contribution, tRay.depth + 1);
					stack.push(reflectRay);
                }
                else
                {
                    refrDir = normalize(refract(ray.direction, -intersect.normal, 1 / intersect.refractionCoef));
                }
                TracingRay refractRay = TracingRay(Ray(intersect.point + 0.001 * refrDir, refrDir), tRay.contribution, tRay.depth + 1);
                stack.push(refractRay);
			}
		}
	}
	
	FragColor = vec4 (resultColor, 1.0);
}

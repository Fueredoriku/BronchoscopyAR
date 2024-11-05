//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED
//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
//#include "UnityCG.cginc"
//#include "UnityShaderVariables.hlsl"

float iTime;

//uniform sampler2D depth_texture : source_color, hint_depth_texture;
//uniform bool isDebug = false;

struct Material
{
    float ambient;
    float diffuse;
    float specular;
    float alpha;
};

Material MaterialInit_float(float ambient, float diffuse, float specular, float alpha)
{
    Material newM;
    newM.ambient = ambient;
    newM.diffuse = diffuse;
    newM.specular = specular;
    newM.alpha = alpha;
    return newM;
}

struct SDF_Surface
{
    float signed_distance;
    float3 color;
    Material material;
};

SDF_Surface SDF_SurfaceInit_float(float signedDistance, float3 color, Material mat)
{
    SDF_Surface newS;
    newS.signed_distance = signedDistance;
    newS.color = color;
    newS.material = mat;
    return newS;
}

// You can add materials like this:
Material material1;  
Material material2;

// Or as a custom shader texture:
float3 CustomMaterialExample_float(float2 uv)
{
    return float3(0.5, 0.5, 0.5);
}
// Or as a color return value from a SDF method in the map method.




float3x3 rotateX_float(float rotationDegrees)
{
    return float3x3(
        float3(1., 0., 0.),
        float3(0., cos(rotationDegrees), -sin(rotationDegrees)),
        float3(0., sin(rotationDegrees), cos(rotationDegrees))
    );
}

float3x3 rotateY_float(float rotationDegrees)
{
    return float3x3(
        float3(cos(rotationDegrees), 0., sin(rotationDegrees)),
        float3(0., 1., 0.),
        float3(-sin(rotationDegrees), 0., cos(rotationDegrees))
    );
}

float3x3 rotateZ_float(float rotationDegrees)
{
    return float3x3(
        float3(cos(rotationDegrees), -sin(rotationDegrees), 0.),
        float3(sin(rotationDegrees), cos(rotationDegrees), 0.),
        float3(0., 0., 1.)
    );
}

SDF_Surface min_sdf_float(SDF_Surface surface1, SDF_Surface surface2)
{
    if (surface1.signed_distance < surface2.signed_distance)
    {
        return surface1;
    }
    return surface2;
}

SDF_Surface smin_sdf_float(SDF_Surface surface1, SDF_Surface surface2, float blend_factor)
{
    float d1 = surface1.signed_distance;
    float d2 = surface2.signed_distance;
    float h = clamp(0.5 + 0.5 * (d2 - d1) / blend_factor, 0.0, 1.0);
    float one_minus_h = 1. - h;
    return SDF_SurfaceInit_float(
		lerp(d2, d1, h) - blend_factor * h * (one_minus_h),
		lerp(surface1.color, surface2.color, one_minus_h),
		MaterialInit_float(
			lerp(surface1.material.ambient, surface2.material.ambient, one_minus_h),
			lerp(surface1.material.diffuse, surface2.material.diffuse, one_minus_h),
			lerp(surface1.material.specular, surface2.material.specular, one_minus_h),
			lerp(surface1.material.alpha, surface2.material.alpha, one_minus_h)
		));
}

// Slightly more optimized material selection that minimizes branching
Material closest_material_float(float distance1, float distance2, Material mt1, Material mt2)
{
    float step1 = step(distance1, distance2);
    float step2 = 1. - step1;
    return MaterialInit_float(
		step1 * mt1.ambient + step2 * mt2.ambient,
		step1 * mt1.diffuse + step2 * mt2.diffuse,
		step1 * mt1.specular + step2 * mt2.specular,
		step1 * mt1.alpha + step2 * mt2.alpha);
}
/*
float signedDistanceSponge(float3 position)
{
    float scale = 1.;
    float3 size = float3(1., 1., 1.);
    position += float3(-1., 1., -1.);

    // How many times to fold the sponge
    int folds = 4;


    position /= 4.;

    // Repeat pattern along z axis
    position.z = 1. - mod(position.z, 2.);


    for (int i = 0; i < folds; i++)
    {
        scale *= 3.8;
        position *= 4.0;

        //position *= rotateX(1.);
        //position *= rotateY(1.5);
        //position *= rotateZ(1.);


        float dist = dot(position + 1., normalize(vec3(1., 0., 0)));
        position -= 2. * normalize(vec3(1., 0.05, 0.)) * min(0., dist);

        dist = dot(position + 1., normalize(vec3(0.05, -1., 0))) + 2.;
        position -= 2. * normalize(vec3(0., -1., 0.)) * min(0., dist);

        //dist = dot(position+1., normalize(vec3(0., 0.2+sin(iTime/2.)*0.2, 1.))) + 0.;
        //position -= 2.*normalize(vec3(0.1+cos(iTime)*0.1, 0.2+sin(iTime/2.)*0.2,1.))*min(0., dist);
        //dist = dot(position+1., normalize(vec3(0., 0.2+sin(iTime)*0.2, 1.))) + 0.;
        //position -= 2.*normalize(vec3(0.1+cos(iTime)*+.2, 0.,1.))*min(0., dist);
        dist = dot(position + 1., normalize(vec3(0., 0., 1.))) + 0.;
        position -= 2. * normalize(vec3(0., 0., 1.)) * min(0., dist);


        dist = dot(position, normalize(vec3(1, 1, 0)));
        position -= 2. * normalize(vec3(1., 1., 0.)) * min(0., dist);

        dist = dot(position, normalize(vec3(0, 1, 1)));
        position -= 2. * normalize(vec3(0., 1.1, 1.)) * min(0., dist);

        dist = dot(position, normalize(vec3(0.15, -1., 0))) + 0.5;
        position -= 2. * normalize(vec3(0., -1., 0.)) * min(0., dist);

        position *= rotateY(TIME);
        //position *= rotateX(iTime);

    }

    float d = length(max(abs(position) - size, 0.));

    return d / scale;
}
*/
// Define your SDF scene
// xyz = color, w = distance
SDF_Surface map_float(float3 position)
{
	// Notes:
	// - All the lines below can be isloated in their own method that returns custom position-warping/colors
	// - Setting this material on a small object in the scene with similar coordinate and scale as the SDF
	//	 acts as an implicit bounding volume!
	// - Defining different marching materials with seperate maps on them makes the marcher evaluate less SDFs
	//	 at the cost of 1 more draw call and inability to combine the shapes from different materials.
	//
	// Reminder:
	// Optimization for SDFs themselves worth looking at include bounding spheres and domain repetition!

	// Edit position to warp scene/SDF surfaces

	// Add distance functions to add/mix shapes
	//float distance_torus = length(vec2(length(position.xz) - .5, position.y)) - .1;
    position = float3(fmod(position.x, 2.0) + sin(position.y * 10. + iTime) * 0.1, position.y - 2.5, fmod(position.z, 1.));
    position = float3(position.x + 1., position.y, position.z + 0.5);
	//position *= rotateY(TIME);
    material1 = MaterialInit_float(1.0, 0.4, 0.1, 1.0);
    material2 = MaterialInit_float(1.0, 1.0, 0.8, 1.0);
 
    float distance_eyeball = length(position) - 0.2;
    float distance_lens = length(position * 1.1 - float3(0., 0., 0.08)) - 0.15;
    float distance_pupil = length(position * float3(1.25, 1.25, 1.3) - float3(0., 0., 0.22)) - 0.05 * (1.2 + abs(sin(iTime) * 0.1));
	//return SDF_Surface(signedDistanceSponge(position), vec3(0., 0., 0.894), material2);
    return min_sdf_float(min_sdf_float(
		SDF_SurfaceInit_float(distance_eyeball, float3(1., 1., 1.), material2),
		SDF_SurfaceInit_float(distance_lens, float3(0., 0., 0.894), material2)),
		SDF_SurfaceInit_float(distance_pupil, float3(0., 0., 0.), material1));
}

float3 map_normal_float(float3 position)
{
    float2 e = float2(1e-2, 0);
    float3 n = map_float(position).signed_distance - float3(
		map_float(position - e.xyy).signed_distance,
		map_float(position - e.yxy).signed_distance,
		map_float(position - e.yyx).signed_distance
	);
    return normalize(n);
}

SDF_Surface march_float(float3 cam_pos, float3 cam_rot, float depth)
{
	// TODO: relaxation/overstepping optimization
    float dist = 0.;
    float step_distance = 0.;
    float step_distance_previous;
    float3 position = cam_pos;
    float min_distance = 0.001;
    float max_distance = depth;
    SDF_Surface result;

    bool relaxed = true;

    for (int steps = 0; steps < 256; steps++)
    {
        float3 new_position = cam_pos + dist * cam_rot;
        result = map_float(new_position);
        step_distance = result.signed_distance;
        dist += step_distance;

        step_distance_previous = step_distance;
        if (step_distance < min_distance || dist > max_distance)
        {
            break;
        }
    }
    return SDF_SurfaceInit_float(dist, result.color, result.material);
}
// TODO: pass in these instead
/*
void vertex()
{
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.)).xyz;
    world_camera = (inverse(MODELVIEW_MATRIX) * vec4(0, 0, 0, 1)).xyz;
}
*/

float3 phong_float(float3 light_direction, float3 normal, float3 camera_direction, SDF_Surface surface)
{
	// Diffuse
    float dotLN = clamp(dot(light_direction, normal), 0., 1.);
    float3 diffuse = (surface.material.diffuse * dotLN * surface.color); // / (surface.signed_distance * surface.signed_distance);

    // specular
    float dotRV = clamp(dot(reflect(light_direction, normal), camera_direction), 0., 1.);
    float3 reflection_color = float3(1.,1.,1.);
    float3 specular = (surface.material.specular * pow(dotRV, surface.material.alpha) * reflection_color); /// (surface.signed_distance * surface.signed_distance);

    return diffuse + specular;
}

float LinearEyeDepth_float(float depth, float nearClip, float farClip)
{
    // Convert the non-linear depth to linear depth
    float linearDepth = (2.0 * nearClip * farClip) / (farClip + nearClip - depth * (farClip - nearClip));
    return linearDepth;
}
float LinearToDepth_float(float linearDepth)
{
    return (1.0 - _ZBufferParams.w * linearDepth) / (linearDepth * _ZBufferParams.z);
}
void fragmentOutput_float(float depth, float2 UV, float3 world_position, float3 world_camera, float3 camera_direction, float itime, bool isDebug, out float4 fragOut)
{
    iTime = itime;
	// Read the scene depth for proper culling with rasterized scene
    //float depth = texture(depth_texture, SCREEN_UV).x;
    //float3 ndc = float3(UV * 2.0 - 1.0, depth);
    //float4 view = mul(float4(ndc, 1.0), unity_CameraInvProjection);
    //view.xyz /= view.w;
    float linear_depth = depth;//LinearToDepth_float(depth); //-view.z;
    float3 position = (float4(world_position, 1.)).xyz;
    SDF_Surface result;
    camera_direction = normalize(world_position - world_camera); //mul(float3(0, 0, -1), (float3x3) UNITY_MATRIX_V); //(float4(normalize(-world_camera), 1.)).xyz;
    
    if (isDebug)
    {
        result = map_float(position);
        result.material.specular = 0.;
        result.material.ambient = 1.;
        result.material.diffuse = 1.;
        result.material.alpha = 0.5;
        result.color = lerp(float3(.5, 0, .1), float3(1., 1., 1.), smoothstep(.02, 0., result.signed_distance)) * (.5 + .8 * cos(60. * result.signed_distance));
        fragOut = float4(result.color, 0.5);
    }
    else
    {
        float max_distance = min(256, depth);
        result = march_float(world_camera, camera_direction, max_distance);
        if (result.signed_distance > max_distance)
        {
            fragOut = float4(0, 0., 0., 0.);
        }
        else
        {
            float3 n = map_normal_float(world_camera + camera_direction * result.signed_distance);
            fragOut = float4(phong_float(float3(0.1, 0.2, 0.4), n, camera_direction, result), result.material.alpha);
        }
    }
}
#endif

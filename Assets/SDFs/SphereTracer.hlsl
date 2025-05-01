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
    float shinyness;
};

Material MaterialInit_float(float ambient, float diffuse, float specular, float shinyness)
{
    Material newM;
    newM.ambient = ambient;
    newM.diffuse = diffuse;
    newM.specular = specular;
    newM.shinyness = shinyness;
    return newM;
}

struct SDF_Surface
{
    float signed_distance;
    float4 color;
    Material material;
};

SDF_Surface SDF_SurfaceInit_float(float signedDistance, float4 color, Material mat)
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
			lerp(surface1.material.shinyness, surface2.material.shinyness, one_minus_h)
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
		step1 * mt1.shinyness + step2 * mt2.shinyness);
}

float scene_float(float3 p)
{
    //sdf is undefined outside the unit sphere, uncomment to witness the abominations
    if (length(p) > 1.)
    {
        return length(p) - .8;
    }
    //neural networks can be really compact... when they want to be
    float4 f00 = sin(p.y * float4(-3.02, 1.95, -3.42, -.60) + p.z * float4(3.08, .85, -2.25, -.24) - p.x * float4(-.29, 1.16, -3.74, 2.89) + float4(-.71, 4.50, -3.24, -3.50));
    float4 f01 = sin(p.y * float4(-.40, -3.61, 3.23, -.14) + p.z * float4(-.36, 3.64, -3.91, 2.66) - p.x * float4(2.90, -.54, -2.75, 2.71) + float4(7.02, -5.41, -1.12, -7.41));
    float4 f02 = sin(p.y * float4(-1.77, -1.28, -4.29, -3.20) + p.z * float4(-3.49, -2.81, -.64, 2.79) - p.x * float4(3.15, 2.14, -3.85, 1.83) + float4(-2.07, 4.49, 5.33, -2.17));
    float4 f03 = sin(p.y * float4(-.49, .68, 3.05, .42) + p.z * float4(-2.87, .78, 3.78, -3.41) - p.x * float4(-2.65, .33, .07, -.64) + float4(-3.24, -5.90, 1.14, -4.71));
    float4 f10 = sin(
    mul(float4x4(
        -.34, .10, .64, -.16,
         .06, -.19, -.02, .21,
        -.59, -.12, -.26, .91,
        -.76, .44, .15, .15
    ), f00)
    +
    mul(float4x4(
         .01, .06, -.18, .33,
         .54, -.14, .08, -.49,
        -.77, .43, .39, -.10,
         .11, .51, .20, .19
    ), f01)
    +
    mul(float4x4(
         .27, .18, -.14, -.13,
         .22, -.17, .02, -.06,
         .43, .23, -.10, -.04,
         .53, -.64, .16, -.36
    ), f02)
    +
    mul(float4x4(
        -.13, 1.13, -.32, .14,
         .29, .02, .04, -.03,
        -.29, -.83, -.31, -.20,
         .08, .32, -.16, .39
    ), f03)
    +
    float4(.73, -4.28, -1.56, -1.80)
) / 1.0 + f00;


    float4 f11 = sin(
    mul(float4x4(
        -1.11, .16, -.01, -.29,
         .55, .15, .01, .38,
        -.12, -.30, .31, -.04,
        -1.00, .31, -.42, .71
    ), f00)
    +
    mul(float4x4(
         .96, -.14, .02, -.06,
        -.02, .60, -.15, -.25,
         .86, .44, -.49, -.03,
         .52, .43, -.05, -.22
    ), f01)
    +
    mul(float4x4(
         .52, -.56, -.04, -.02,
         .44, -.10, .55, .28,
        -.05, -.61, .32, .26,
        -.11, -.40, -.07, -.49
    ), f02)
    +
    mul(float4x4(
         .02, -.59, -.06, -.12,
        -.32, .00, .13, -.14,
         .06, -.24, -.21, .58,
        -.17, .60, -.27, -.55
    ), f03)
    +
    float4(-2.24, -3.48, -.80, 1.41)
) / 1.0 + f01;


    float4 f12 = sin(
    mul(float4x4(
         .44, .05, .35, .40,
        -.06, -.60, .12, -.26,
        -.79, .30, .02, .63,
        -.46, .36, .12, -.21
    ), f00)
    +
    mul(float4x4(
        -.48, .11, -.25, .32,
         .43, -.01, .25, -.02,
        -.73, .71, -.28, -.84,
        -.40, .05, -.20, .16
    ), f01)
    +
    mul(float4x4(
         .39, -.38, .48, -.00,
        -.07, -.27, -.20, -.21,
         .90, -1.86, -.05, .29,
         .36, -.39, .10, .63
    ), f02)
    +
    mul(float4x4(
         .46, .72, .90, -.16,
        -.32, -.47, .02, .22,
         .06, .81, -.21, .32,
         .09, .78, .08, -.13
    ), f03)
    +
    float4(3.38, 1.20, .84, 1.41)
) / 1.0 + f02;


    float4 f13 = sin(
    mul(float4x4(
        -.41, -.24, -.27, -.01,
        -.24, -.75, -.42, .51,
        -.71, -.09, .02, -.12,
        -.25, .02, .03, -1.24
    ), f00)
    +
    mul(float4x4(
         .64, -.34, .22, .02,
         .31, .11, -.16, -.37,
        -1.36, .14, -.29, .49,
         .61, .79, -.70, .39
    ), f01)
    +
    mul(float4x4(
         .79, -1.13, -.67, -.07,
         .47, -.35, -.26, -.73,
         .54, -1.03, .10, -.11,
        -.47, -.22, .21, .72
    ), f02)
    +
    mul(float4x4(
         .43, 1.38, .39, -.57,
        -.23, -.63, -.14, -.08,
         .13, 1.57, .42, -.21,
         .09, -.20, .13, .21
    ), f03)
    +
    float4(-.34, -3.28, .43, -.52)
) / 1.0 + f03;


    f00 = sin(
    mul(float4x4(
        -.72, .38, .26, .29,
         .23, .19, -.37, -.72,
        -.89, -.16, .09, .30,
         .52, -.88, .63, -.95
    ), f10)
    +
    mul(float4x4(
        -.22, -.32, -.20, -.41,
        -.51, .00, -.03, .09,
        -.42, -1.03, -.13, .36,
        -.73, 1.17, -.16, -.84
    ), f11)
    +
    mul(float4x4(
        -.21, .05, .13, .01,
         .01, .20, .12, -.34,
         .33, -.44, -.13, .41,
         .47, -1.04, .31, -.34
    ), f12)
    +
    mul(float4x4(
        -.13, .48, -.34, -.44,
        -.06, .25, .14, .05,
        -.39, .24, .42, .09,
        -.22, -.97, -.00, -.95
    ), f13)
    +
    float4(.48, .87, -.87, -2.06)
) / 1.4 + f10;

    f01 = sin(
    mul(float4x4(
        -.27, .34, -1.15, -.12,
         .29, -.23, -.24, -.73,
        -.21, .85, -.05, -.17,
         .15, -.09, -.25, -.37
    ), f10)
    +
    mul(float4x4(
        -1.11, -.79, .60, -.03,
         .35, -.03, -.37, -.21,
        -.93, -.46, -.14, .02,
        -.06, -.37, .45, .59
    ), f11)
    +
    mul(float4x4(
        -.92, .58, -.80, .08,
        -.17, .60, -.16, .16,
        -.58, .83, .23, .76,
        -.18, -1.04, -.11, .61
    ), f12)
    +
    mul(float4x4(
         .29, -.91, .21, 1.10,
         .45, .66, .16, -.38,
         .30, -.35, -.54, .20,
         .39, -.35, -.63, .15
    ), f13)
    +
    float4(-1.72, -.14, 1.92, 2.08)
) / 1.4 + f11;

    f02 = sin(
    mul(float4x4(
        1.00, .88, -.68, .46,
         .66, .25, -.08, 1.15,
        1.30, -.67, -.12, .38,
        -.51, .03, -.14, -.10
    ), f10)
    +
    mul(float4x4(
         .51, .68, .20, -.09,
        -.57, -.50, .44, -.37,
         .41, -.04, -.60, -1.30,
        -.09, -1.01, .46, .04
    ), f11)
    +
    mul(float4x4(
         .14, -.65, .71, -1.68,
         .29, .33, -.07, -.20,
        -.45, -.37, 1.00, -.00,
        -.06, -.95, -.60, -.70
    ), f12)
    +
    mul(float4x4(
        -.31, .95, -.63, 1.23,
         .69, .36, .52, .72,
         .56, .56, -.30, .95,
         .13, .59, .17, .75
    ), f13)
    +
    float4(-.90, -3.26, -.44, -3.11)
) / 1.4 + f12;

    f03 = sin(
    mul(float4x4(
         .51, -.22, .70, .78,
        -.98, -.17, -.15, .67,
        -.28, -1.03, .12, -.85,
         .16, .22, .43, -.25
    ), f10)
    +
    mul(float4x4(
         .81, -1.03, -.06, .73,
         .60, -.33, .01, .69,
        -.89, .60, -.02, 1.02,
         .61, -.11, -.44, .62
    ), f11)
    +
    mul(float4x4(
        -.10, .40, .03, -.03,
         .52, -.75, .05, .22,
         .80, .47, .08, -1.63,
        -.65, 1.56, .31, .07
    ), f12)
    +
    mul(float4x4(
        -.18, -.01, .24, .23,
        -.07, .56, .25, -.08,
       -1.22, .07, -.09, .20,
         .48, .15, -.54, .36
    ), f13)
    +
    float4(-1.11, -4.28, 1.02, -.23)
) / 1.4 + f13;


    return dot(f00, float4(.09, .12, -.07, -.03)) + dot(f01, float4(-.04, .07, -.08, .05)) +
        dot(f02, float4(-.01, .06, -.02, .07)) + dot(f03, float4(-.05, .07, .03, .04)) - 0.16;
}

float scene2_float (float3 p)
{
    //sdf is undefined outside the unit sphere, uncomment to witness the abominations
    if (length(p) > 1.)
    {
        return length(p) - .8;
    }
    p.x *= -1.;
    //neural networks can be really compact... when they want to be
    float4 f0_0 = sin(p.y * float4(3.451, .171, -.551, -1.848) + p.z * float4(-2.399, .017, 5.274, 4.896) + p.x * float4(-2.966, -2.308, 10.432, -5.567) + float4(12.886, -.910, -6.792, 11.684));
    float4 f0_1 = sin(p.y * float4(-.137, -.490, -4.093, 2.032) + p.z * float4(-3.355, -3.784, 3.027, -.309) + p.x * float4(-6.207, -4.424, 1.419, -1.185) + float4(3.072, 3.820, 14.790, 11.643));
    float4 f0_2 = sin(p.y * float4(3.946, 4.274, -.307, 2.540) + p.z * float4(-2.232, .659, -6.993, -5.860) + p.x * float4(5.025, 1.084, -5.392, .221) + float4(-3.610, 14.136, 5.845, -.843));
    float4 f0_3 = sin(p.y * float4(4.826, 2.762, 1.606, -6.086) + p.z * float4(5.305, -7.457, 8.015, 6.262) + p.x * float4(1.832, -7.432, -6.068, .865) + float4(11.432, 1.207, -12.846, -2.363));
    float4 f0_4 = sin(p.y * float4(-2.188, 5.530, 1.226, -2.565) + p.z * float4(-3.773, -6.355, 6.850, 2.333) + p.x * float4(4.341, 5.505, 5.511, .678) + float4(-10.405, 6.693, -9.094, 2.018));
    float4 f0_5 = sin(p.y * float4(-.273, -1.481, -3.523, 4.763) + p.z * float4(.965, .201, -6.538, 1.235) + p.x * float4(2.613, -2.177, .442, 5.917) + float4(-12.510, -10.641, 14.820, -3.122));
    float4 f0_6 = sin(p.y * float4(2.865, -6.743, 1.124, 5.784) + p.z * float4(1.636, -.561, 2.778, 1.156) + p.x * float4(5.048, -3.575, -1.568, 6.485) + float4(-15.848, 6.715, -.820, -9.125));
    float4 f0_7 = sin(p.y * float4(-3.059, -2.427, -3.940, -3.792) + p.z * float4(-7.302, -.829, -6.473, -7.030) + p.x * float4(5.225, .523, -1.138, 5.196) + float4(3.608, -14.064, -4.991, -15.086));
    float4 f1_0 = sin(mul(float4x4(
    0.56, 0.67, 0.26, -0.40,
    -1.56, 0.78, -0.79, 0.65,
    -0.99, -0.71, -0.53, 0.23,
    0.16, 0.58, -0.01, -0.48
), f0_0) +
    mul(float4x4(
    1.93, -1.05, -0.47, 0.81,
    -0.88, 0.20, 1.61, 1.22,
    -1.85, 0.77, 0.15, 1.23,
    -0.02, 0.07, -0.42, -0.98
), f0_1) +
    mul(float4x4(
    0.26, -0.85, -0.94, 0.50,
    -0.53, 0.85, -0.40, -0.22,
    -0.24, 0.68, 0.30, -0.35,
    0.05, -0.16, -0.52, -0.60
), f0_2) +
    mul(float4x4(
    0.10, -0.17, -0.12, -0.19,
    -0.79, -0.27, -0.81, -0.17,
    -0.88, 0.17, -0.76, 0.15,
    -0.50, 0.06, -0.66, 0.01
), f0_3) +
    mul(float4x4(
    -0.77, 0.02, 0.47, 0.41,
    0.76, 0.06, 0.85, -0.97,
    0.90, 0.06, 0.13, 0.46,
    -0.91, 0.08, 0.82, 1.32
), f0_4) +
    mul(float4x4(
    3.13, 0.97, -0.54, -1.16,
    -0.78, 0.68, 0.13, 0.75,
    -2.34, 0.67, 1.01, 1.36,
    -0.67, -0.36, 0.21, -0.43
), f0_5) +
    mul(float4x4(
    1.16, -0.10, 0.17, 0.52,
    -1.47, -0.10, -0.06, -0.11,
    -0.90, -0.06, 0.29, -0.60,
    0.83, -0.28, -0.22, 0.10
), f0_6) +
    mul(float4x4(
    0.37, -1.51, 0.28, -0.29,
    -0.43, 1.19, -1.42, 1.15,
    0.48, 0.66, -1.24, 0.22,
    0.25, 0.02, -1.18, 0.66
), f0_7) +
    float4(-2.367, 4.515, -.165, -1.002)) / 1.0 + f0_0;
    float4 f1_1 = sin(mul(float4x4(
    0.05, -0.17, 0.06, -0.17,
    -1.24, 0.41, -0.17, 0.26,
    -1.50, -0.54, -1.12, 1.08,
    -0.18, -0.54, 0.05, -0.24
), f0_0) +
    mul(float4x4(
    0.52, 0.31, -0.47, 0.79,
    -1.03, 0.07, 0.55, -0.39,
    -1.19, -0.23, 0.25, -0.45,
    0.47, -0.21, 0.01, 0.01
), f0_1) +
    mul(float4x4(
    0.30, 0.35, 0.66, 0.06,
    0.11, 0.46, 0.13, -0.41,
    0.04, 0.82, 1.43, -0.22,
    0.35, -0.52, -0.65, 0.07
), f0_2) +
    mul(float4x4(
    0.08, -0.26, 0.49, -0.39,
    -1.00, 0.20, -0.79, 0.14,
    0.14, -0.87, 0.24, -0.19,
    0.06, 0.10, 0.22, -0.23
), f0_3) +
    mul(float4x4(
    0.48, -0.08, -0.88, 0.70,
    0.11, -0.13, 0.25, 0.55,
    1.13, 0.26, -0.25, -0.12,
    -0.49, -0.21, 0.46, 0.60
), f0_4) +
    mul(float4x4(
    0.84, -0.11, -0.45, -0.05,
    -1.29, -0.07, 0.62, -1.09,
    -2.81, -0.41, 0.40, 0.90,
    0.09, 0.34, -0.16, -0.65
), f0_5) +
    mul(float4x4(
    -0.01, 0.11, 0.35, 0.20,
    0.25, 0.03, -0.12, 0.69,
    -1.92, 0.23, 0.26, -0.41,
    -0.16, -0.04, -0.28, 0.43
), f0_6) +
    mul(float4x4(
    -0.74, -0.21, 0.11, 0.04,
    -0.05, 0.62, -1.00, 0.43,
    -1.81, 0.66, -0.01, 1.14,
    -0.61, -0.59, 0.04, 0.23
), f0_7) +
    float4(-3.414, .075, 2.821, 2.914)) / 1.0 + f0_1;
    float4 f1_2 = sin(mul(float4x4(
    1.33, -0.54, 0.39, 0.14,
    -1.99, -0.74, -0.67, 0.58,
    0.49, 0.28, -0.00, -0.52,
    -1.11, -0.01, -0.73, 1.01
), f0_0) +
    mul(float4x4(
    -0.35, 1.07, -0.01, -0.38,
    -2.11, 0.64, 1.07, 1.00,
    0.59, -0.27, -0.05, -0.35,
    -0.70, -0.43, 0.55, 0.77
), f0_1) +
    mul(float4x4(
    0.12, 0.43, 0.99, 0.34,
    -0.20, 0.54, 0.48, -0.46,
    -0.13, -0.14, -1.24, -0.36,
    -0.26, 0.18, -0.80, 0.22
), f0_2) +
    mul(float4x4(
    0.28, 0.53, 1.48, -0.01,
    -0.79, 0.01, -0.86, 0.11,
    0.38, 0.09, -0.18, 0.07,
    0.53, -0.30, 0.43, 0.06
), f0_3) +
    mul(float4x4(
    -0.20, -0.17, -1.42, -0.74,
    1.43, 0.07, 0.19, -0.54,
    -0.62, -0.23, 1.15, -0.98,
    1.30, 0.09, 1.07, -1.42
), f0_4) +
    mul(float4x4(
    0.89, -0.54, -0.25, -0.15,
    -4.12, -0.05, 0.85, 1.63,
    -0.75, -0.07, -0.70, -0.19,
    -2.43, -0.43, 0.33, 0.39
), f0_5) +
    mul(float4x4(
    0.65, 0.45, -0.48, 0.03,
    -1.98, -0.22, 0.21, -0.64,
    0.41, -0.28, 0.79, -0.09,
    -0.82, -0.05, 0.09, 0.06
), f0_6) +
    mul(float4x4(
    -1.04, -0.41, 1.17, -0.29,
    -0.35, 0.96, -1.28, 0.93,
    -0.06, -1.85, 0.36, 0.54,
    -1.70, 0.40, 0.29, 1.15
), f0_7) +
    float4(2.790, -4.990, .909, -.284)) / 1.0 + f0_2;
    float4 f1_3 = sin(mul(float4x4(
    -0.12, 0.74, -0.04, 0.13,
    -0.24, -1.14, 0.36, 0.12,
    -0.22, -0.06, -0.16, -0.66,
    -1.24, -0.34, -0.44, 0.79
), f0_0) +
    mul(float4x4(
    0.06, -0.06, 0.40, -0.24,
    0.08, 0.17, 0.17, 0.16,
    0.20, 0.49, 0.33, 0.09,
    -1.16, -0.13, 0.75, -0.69
), f0_1) +
    mul(float4x4(
    -0.49, 0.33, 0.12, -0.25,
    0.29, 0.28, -0.04, -0.62,
    0.31, 1.34, -1.99, -0.84,
    -0.07, 0.46, 0.13, -0.36
), f0_2) +
    mul(float4x4(
    0.36, 0.12, -0.34, -0.08,
    -0.06, 0.17, -0.53, 0.05,
    -1.06, 0.04, -0.84, -0.05,
    -0.52, 0.19, -0.33, -0.01
), f0_3) +
    mul(float4x4(
    -0.17, -0.04, -0.17, -0.13,
    0.14, -0.17, -0.35, 0.59,
    -0.94, 0.13, 1.86, 0.72,
    0.56, 0.18, 0.38, -0.09
), f0_4) +
    mul(float4x4(
    0.08, 0.66, -0.40, 0.18,
    0.78, -0.74, 0.17, 0.74,
    0.79, 2.04, 0.47, 0.21,
    -2.18, -1.66, 0.35, -0.32
), f0_5) +
    mul(float4x4(
    0.03, -0.29, -0.11, -0.16,
    -0.46, -0.37, 0.03, -0.36,
    0.57, -0.39, -0.35, -0.16,
    -0.80, 0.17, -0.82, 0.43
), f0_6) +
    mul(float4x4(
    -0.50, -0.17, 0.30, 0.75,
    1.75, -0.72, 0.14, -1.42,
    0.59, -0.16, -1.99, 0.51,
    -1.59, 0.48, -0.67, 1.49
), f0_7) +
    float4(-3.211, -4.360, -.842, 4.125)) / 1.0 + f0_3;
    float4 f1_4 = sin(mul(float4x4(
    0.85, 0.97, 0.18, -0.50,
    0.50, -1.02, 0.31, -0.49,
    -0.69, 0.43, -0.14, -0.46,
    -0.31, 1.02, -0.18, 0.23
), f0_0) +
    mul(float4x4(
    0.03, 0.47, -0.47, -0.10,
    0.10, 0.71, -0.41, -0.21,
    0.91, -0.45, 0.01, 0.29,
    -1.61, 0.78, 0.35, -1.03
), f0_1) +
    mul(float4x4(
    -0.18, 0.14, 0.88, -0.42,
    0.07, -0.17, -0.32, -0.10,
    0.08, 0.60, -1.71, -0.10,
    -0.26, 0.72, 0.91, -0.58
), f0_2) +
    mul(float4x4(
    -0.57, 0.18, -0.38, 0.08,
    0.24, 0.17, -0.10, 0.24,
    -0.58, -0.55, -0.83, -0.21,
    -0.52, 0.12, -0.26, 0.19
), f0_3) +
    mul(float4x4(
    -0.92, -0.12, -0.85, 0.36,
    -0.19, 0.22, -0.03, 0.10,
    0.01, 0.13, 2.02, 0.45,
    0.70, 0.06, -0.68, -0.16
), f0_4) +
    mul(float4x4(
    0.51, -0.42, -0.54, -0.06,
    1.69, -0.14, 0.17, -1.40,
    0.94, 2.31, 0.01, 0.13,
    -1.38, 0.90, 0.68, 0.52
), f0_5) +
    mul(float4x4(
    0.61, -0.10, 0.71, -0.24,
    1.95, 0.30, 0.23, 0.28,
    0.32, -0.04, 0.19, -0.03,
    -0.33, 0.06, 0.19, -0.32
), f0_6) +
    mul(float4x4(
    1.33, -0.93, -0.49, -0.76,
    1.75, -0.39, 0.63, -1.84,
    0.43, 0.42, -1.36, -0.02,
    0.80, 0.62, -0.46, -0.51
), f0_7) +
    float4(-1.785, 1.722, 3.434, 4.328)) / 1.0 + f0_4;
    float4 f1_5 = sin(mul(float4x4(
    -0.04, 0.57, 0.03, 0.56,
    1.47, 0.22, 1.10, -0.92,
    -0.82, 0.09, -0.40, 1.01,
    -0.27, -1.32, -0.09, 0.20
), f0_0) +
    mul(float4x4(
    0.62, -0.07, 0.15, 0.65,
    1.28, 0.01, -0.37, 0.61,
    -0.38, -0.83, 0.45, 0.49,
    0.20, 0.04, 0.64, -0.02
), f0_1) +
    mul(float4x4(
    0.15, 0.41, -0.39, 0.38,
    -0.03, -0.73, -1.22, 0.19,
    -0.08, 0.39, 0.66, 0.50,
    -0.25, 0.42, -0.06, 0.38
), f0_2) +
    mul(float4x4(
    0.51, -0.25, 0.50, -0.17,
    0.10, 0.79, -0.19, 0.15,
    0.90, -0.12, 1.04, -0.09,
    -0.70, 0.11, -0.17, -0.10
), f0_3) +
    mul(float4x4(
    0.15, -0.01, 0.26, 0.33,
    -1.20, -0.18, 0.15, 0.16,
    0.86, -0.07, -0.55, -0.75,
    0.05, 0.08, -0.05, -0.53
), f0_4) +
    mul(float4x4(
    -0.17, -0.02, 0.08, 0.23,
    2.81, -0.10, -0.52, -0.80,
    -1.16, -1.16, 0.29, 1.29,
    0.12, -0.71, 0.12, 0.33
), f0_5) +
    mul(float4x4(
    -0.07, 0.07, 0.39, -0.07,
    1.73, -0.20, -0.47, 0.38,
    -1.71, 0.16, 0.34, -0.65,
    0.21, 0.21, 0.73, -0.21
), f0_6) +
    mul(float4x4(
    -0.33, -1.36, 0.80, -0.06,
    1.77, -0.64, 0.26, -1.19,
    -1.68, 0.01, 1.01, 0.83,
    0.57, 0.44, -0.65, -0.45
), f0_7) +
    float4(-4.482, .526, 3.954, -2.233)) / 1.0 + f0_5;
    float4 f1_6 = sin(mul(float4x4(
    -0.90, 0.62, -0.68, 0.96,
    -0.71, 0.83, -0.53, 1.12,
    -0.64, -0.03, 0.10, -0.70,
    0.62, 0.93, 0.20, -0.34
), f0_0) +
    mul(float4x4(
    0.30, -1.74, -0.24, -0.37,
    -0.77, -0.53, 0.37, -0.51,
    0.54, -0.42, -0.24, 0.30,
    0.07, 0.09, 0.16, -0.51
), f0_1) +
    mul(float4x4(
    0.21, 0.75, 1.43, -0.17,
    -0.20, 0.29, 0.09, 0.23,
    0.27, 0.72, -1.14, -0.30,
    0.10, 0.07, -0.82, 0.33
), f0_2) +
    mul(float4x4(
    0.62, -0.65, 0.03, -0.23,
    0.24, -0.30, 0.44, 0.04,
    -0.58, 0.09, -0.33, -0.16,
    -0.31, 0.32, -0.11, -0.24
), f0_3) +
    mul(float4x4(
    0.18, 0.25, -0.56, -0.48,
    1.16, -0.10, 0.05, -0.92,
    0.28, 0.01, 1.54, 0.61,
    -0.33, -0.21, 0.47, -0.75
), f0_4) +
    mul(float4x4(
    -1.38, -0.28, -0.49, 0.92,
    -1.03, -0.76, 0.29, 0.61,
    1.18, 1.48, 0.10, 0.45,
    0.19, 0.28, -0.18, -1.10
), f0_5) +
    mul(float4x4(
    -2.02, 0.00, -1.49, -0.20,
    -0.93, -0.06, 0.52, -0.11,
    0.55, -0.03, 0.63, -0.50,
    0.83, -0.10, -0.28, 0.57
), f0_6) +
    mul(float4x4(
    -0.92, 1.46, 1.07, 0.46,
    -0.76, 0.69, 0.43, 0.42,
    1.01, 0.60, -0.68, -0.60,
    -0.54, 0.13, -0.14, 0.64
), f0_7) +
    float4(4.227, -4.459, 1.163, 2.772)) / 1.0 + f0_6;
    float4 f1_7 = sin(mul(float4x4(
    -0.35, 0.47, -0.13, -0.06,
    -1.49, 0.51, -0.61, 0.33,
    0.09, -0.02, 0.20, -0.46,
    -0.04, 0.04, 0.18, -0.38
), f0_0) +
    mul(float4x4(
    -0.67, 0.81, 0.10, -0.24,
    -0.11, -0.69, 1.37, 0.74,
    -0.17, 0.69, -0.22, 0.80,
    -0.10, -0.27, 0.50, 0.29
), f0_1) +
    mul(float4x4(
    0.02, 0.32, 0.46, 0.09,
    -0.16, 0.41, -0.96, -0.29,
    -0.20, 0.08, 0.04, -0.30,
    0.32, -0.09, 1.16, 0.04
), f0_2) +
    mul(float4x4(
    -0.32, 0.08, 0.42, 0.04,
    -0.35, -0.48, -1.17, 0.05,
    0.18, 0.10, 0.55, 0.12,
    0.25, 0.25, 0.27, -0.11
), f0_3) +
    mul(float4x4(
    -0.10, -0.01, 0.03, -0.44,
    0.56, 0.28, 1.63, -1.22,
    -0.17, -0.05, 0.08, -0.07,
    -0.27, 0.01, -1.55, 0.07
), f0_4) +
    mul(float4x4(
    -0.19, 0.37, 0.13, 0.15,
    -0.51, 0.91, 0.00, 1.17,
    1.02, 1.22, -0.40, -0.06,
    -0.39, 0.13, 0.13, -0.00
), f0_5) +
    mul(float4x4(
    0.04, 0.11, 0.30, -0.16,
    -1.31, -0.32, -0.07, -0.46,
    0.12, -0.03, -0.82, -0.01,
    -0.60, -0.09, -0.06, 0.11
), f0_6) +
    mul(float4x4(
    -0.10, 0.59, -0.25, -0.11,
    0.29, 1.25, -1.03, 0.58,
    -1.48, 0.14, 0.24, 1.08,
    -0.27, -0.02, 0.34, -0.10
), f0_7) +
    float4(-2.363, -1.533, -4.280, -3.239)) / 1.0 + f0_7;
    return dot(f1_0, float4(.092, -.045, .069, -.079)) +
    dot(f1_1, float4(.078, -.077, .110, .128)) +
    dot(f1_2, float4(.054, -.060, -.066, -.073)) +
    dot(f1_3, float4(.125, .072, -.030, -.060)) +
    dot(f1_4, float4(-.087, -.059, -.057, -.103)) +
    dot(f1_5, float4(.111, -.113, -.100, -.116)) +
    dot(f1_6, float4(-.039, -.092, -.051, .098)) +
    dot(f1_7, float4(-.158, .061, .120, .131)) +
    0.218;
}


float signedDistanceSponge_float(float3 position)
{
    float scale = 1.;
    float3 size = float3(1., 1., 1.);
    position += float3(-1., 1., -1.);

    // How many times to fold the sponge
    int folds = 4;


    position /= 4.;

    // Repeat pattern along z axis
    //position.z = 1. - fmod(position.z, 2.);


    for (int i = 0; i < folds; i++)
    {
        scale *= 3.8;
        position *= 4.0;

        //position *= rotateX(1.);
        //position *= rotateY(1.5);
        //position *= rotateZ(1.);


        float dist = dot(position + 1., normalize(float3(1., 0., 0)));
        position -= 2. * normalize(float3(1., 0.05, 0.)) * min(0., dist);

        dist = dot(position + 1., normalize(float3(0.05, -1., 0))) + 2.;
        position -= 2. * normalize(float3(0., -1., 0.)) * min(0., dist);

        //dist = dot(position+1., normalize(vec3(0., 0.2+sin(iTime/2.)*0.2, 1.))) + 0.;
        //position -= 2.*normalize(vec3(0.1+cos(iTime)*0.1, 0.2+sin(iTime/2.)*0.2,1.))*min(0., dist);
        //dist = dot(position+1., normalize(vec3(0., 0.2+sin(iTime)*0.2, 1.))) + 0.;
        //position -= 2.*normalize(vec3(0.1+cos(iTime)*+.2, 0.,1.))*min(0., dist);
        dist = dot(position + 1., normalize(float3(0., 0., 1.))) + 0.;
        position -= 2. * normalize(float3(0., 0., 1.)) * min(0., dist);


        dist = dot(position, normalize(float3(1, 1, 0)));
        position -= 2. * normalize(float3(1., 1., 0.)) * min(0., dist);

        dist = dot(position, normalize(float3(0, 1, 1)));
        position -= 2. * normalize(float3(0., 1.1, 1.)) * min(0., dist);

        dist = dot(position, normalize(float3(0.15, -1., 0))) + 0.5;
        position -= 2. * normalize(float3(0., -1., 0.)) * min(0., dist);

        position = mul(rotateY_float(iTime), position);
        //position *= rotateX(iTime);

    }

    float d = length(max(abs(position) - size, 0.));

    return d / scale;
}
float sdBox_float(float3 p, float3 b)
{
    float3 q = abs(p) - b;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
}
float4 repeated_mirrored_float(float2 p, float s)
{
    float2 id = round(p / s);
    float2 r = p - s * id;
    float2 m = float2(((int(id.x) & 1) == 0) ? r.x : -r.x,
                   ((int(id.y) & 1) == 0) ? r.y : -r.y);
    return float4(m.x, m.y, id.x, id.y);
}
float3 pal(in float t, in float3 a, in float3 b, in float3 c, in float3 d)
{
    return a + b * cos(6.28318 * (c * t + d));
}
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
    Material chosen;
    /*
    float distance_fractal = abs(position.y-0.5) - 2.5;
    //position = float3(fmod(position.x , 6.0), position.y - 2.5, fmod(position.z +3., 6.));
    //position = float3(position.x + 1., position.y, position.z + 0.5);
	//position *= rotateY(TIME);
    //material1 = MaterialInit_float(1.0, 0.4, 0.1, 1.0);
 
    //float distance_eyeball = length(position) - 0.2;
    //float distance_lens = length(position * 1.1 - float3(0., 0., 0.08)) - 0.15;
    //float distance_pupil = length(position * float3(1.25, 1.25, 1.3) - float3(0., 0., 0.22)) - 0.05 * (1.2 + abs(sin(iTime) * 0.1));
    
    float4 color = float4(0.,0.,0., 0.);
    if (distance_fractal < 0.1)
    {
        float4 repeated = repeated_mirrored_float(float2(position.x, position.z), 5.);
        float3 new_pos = float3(repeated.x, position.y, repeated.y) * 1.5
        + float3(0., sin(repeated.z) + sin(repeated.w), 0.);
        distance_fractal = sdBox_float(new_pos, float3(2.5, 2.5, 2.5));
        if (distance_fractal < 0.1)
        {
            distance_fractal = signedDistanceSponge_float(new_pos);
            chosen = MaterialInit_float(0.01, 0.4, 0.2, 0.1);;
            float3 color_spectre = pal(sin(repeated.z) + sin(repeated.w), float3(0.5, 0.5, 0.5), float3(0.5, 0.5, 0.5), float3(1.0, 1.0, 1.0), float3(0.0, 0.33, 0.67));
            color = float4(color_spectre.x, color_spectre.y, color_spectre.z, 1.);
        }
        else
        {
            chosen = MaterialInit_float(0., 0., 0., 0.);
            //color = float4(0.3, 0.7, 0.5, 1.);
            
        }

    }
    else
    {
        chosen = MaterialInit_float(0.,0.,0.,0.);
    }
    */
    chosen = MaterialInit_float(0.01, 0.4, 0.2, 0.1);
    float4 color = float4(0.3, 0.7, 0.5, 1.);
    return SDF_SurfaceInit_float(scene2_float(position), color, chosen);
    /*
    return min_sdf_float(min_sdf_float(
		SDF_SurfaceInit_float(distance_eyeball, float3(1., 1., 1.), material2),
		SDF_SurfaceInit_float(distance_lens, float3(0., 0., 0.894), material2)),
		SDF_SurfaceInit_float(distance_pupil, float3(0., 0., 0.), material1));*/
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
    float epsilon = 0.001;
    float min_distance = 0.001;
    float max_distance = depth;
    SDF_Surface result;

    bool relaxed = true;

    do
    {
        result = map_float(position);
        step_distance = result.signed_distance;
        dist += step_distance;
        position = cam_pos + dist * cam_rot;
    } while (min(max_distance - dist, step_distance - epsilon * dist) > 0.);
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
float4 shade_float(float3 lightDir, float3 normal, float3 viewDir, SDF_Surface surface)
{
    float3 lightColor = float3(0.1, 0.1, 0.1);
    // Lambertian diffuse
    float diffuse = max(dot(normal, lightDir), 0.0);
    
    float3 reflectDir = reflect(lightDir, normal);
    // Phong specular 
    // Schlick's Fresnel approximation
    float cosTheta = max(dot(normal, viewDir), 0.0);
    float F0 = smoothstep(0.05, 0.5, surface.material.shinyness);
    float fresnel = F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
    
    float specular = pow(max(dot(viewDir, reflectDir), 0.0), surface.material.shinyness);
    specular *= fresnel;
    float3 color = surface.color.xyz * (surface.material.diffuse + diffuse) + surface.material.specular * specular + surface.material.ambient;
    return float4(color.r, color.g, color.b, surface.color.a);

}
float3 phong_float(float3 light_direction, float3 normal, float3 camera_direction, SDF_Surface surface)
{
	// Diffuse
    float dotLN = clamp(dot(light_direction, normal), 0., 1.);
    float3 diffuse = (surface.material.diffuse * dotLN * surface.color); // / (surface.signed_distance * surface.signed_distance);

    // specular
    float dotRV = clamp(dot(reflect(light_direction, normal), camera_direction), 0., 1.);
    float3 reflection_color = float3(1.,1.,1.);
    float3 specular = (surface.material.specular * pow(dotRV, surface.material.shinyness) * reflection_color); /// (surface.signed_distance * surface.signed_distance);

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
        result.material.shinyness = 0.5;
        result.color = lerp(float4(.5, 0, .1, 0.5), float4(1., 1., 1., 0.5), smoothstep(.02, 0., result.signed_distance)) * (.5 + .8 * cos(60. * result.signed_distance));
        fragOut = result.signed_distance < 0 ? float4(result.color.x, result.color.y, result.color.z, 1.) : float4(0., 0., 0., 0.5);
    }
    else
    {
        float max_distance = min(256, depth);
        result = march_float(world_camera, camera_direction, max_distance);
        if (result.signed_distance > max_distance)
        {
            fragOut = float4(0., 0., 0., 0.);
        }
        else
        {
            float3 n = map_normal_float(world_camera + camera_direction * result.signed_distance);
            fragOut = float4(shade_float(float3(0.1, 0.2, 0.4), n, camera_direction, result));
        }
    }
}
#endif

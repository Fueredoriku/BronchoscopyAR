Shader "Unlit/SphereTracer"
{
    Properties
    {
        _DepthTex ("Depth Texture", 2D) = "white" {}
        _IsDebug ("Debug Mode", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Uniforms
            sampler2D _DepthTex;
            float _IsDebug;
            float4x4 _ModelMatrix;
            float4x4 _InverseModelMatrix;
            float4x4 _ProjectionMatrix;
            float4x4 _InverseProjectionMatrix;

            // Constants
            const float maxDistanceBase = 100.0;
            const int maxSteps = 256;

            // Material struct
            struct Material {
                float ambient;
                float diffuse;
                float specular;
                float alpha;
            };

            // SDF Surface struct
            struct SDF_Surface {
                float signed_distance;
                float3 color;
                float4 material;
            };

            // Example materials
            //Material material1 = {1.0, 0.4, 0.1, 1.0};
            //Material material2 = {1.0, 1.0, 0.8, 1.0};
            float4 material1 = float4(1.0, 0.4, 0.1, 1.0);
            float4 material2 = float4(1.0, 1.0, 0.8, 1.0);

            // Rotation matrices
            float3x3 rotateX(float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return float3x3(
                    1, 0, 0,
                    0, c, -s,
                    0, s, c
                );
            }

            float3x3 rotateY(float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return float3x3(
                    c, 0, s,
                    0, 1, 0,
                    -s, 0, c
                );
            }

            float3x3 rotateZ(float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return float3x3(
                    c, -s, 0,
                    s, c, 0,
                    0, 0, 1
                );
            }

            // SDF operations
            SDF_Surface min_sdf(SDF_Surface s1, SDF_Surface s2) {
                if (s1.signed_distance < s2.signed_distance) 
                { return s1; }
                else { return s2; }
            }

            // Define scene's SDF function
            SDF_Surface map(float3 position)
            {
                float distance_eyeball = length(position) - 0.2;
	            float distance_lens = length(position * 1.1 - float3(0.,0.,0.08)) -0.15;
	            float distance_pupil = length(position * float3(1.25,1.25,1.3) - float3(0.,0.,0.22)) -0.05 * (1.2+abs(_SinTime*0.1));
                SDF_Surface eyeball;
                eyeball.signed_distance = distance_eyeball;
                eyeball.color = float3(1., 1., 1.);
                eyeball.material = material2;
                SDF_Surface lens;
                lens.signed_distance = distance_lens;
                lens.color = float3(0., 0., 0.894);
                lens.material = material2;
                SDF_Surface pupil;
                pupil.signed_distance = distance_pupil;
                pupil.color = float3(0., 0., 0.);
                pupil.material = material1;
                return min_sdf(
                    min_sdf(eyeball, lens),
		            pupil);
            }

            // Ray marching function
            SDF_Surface march(float3 camPos, float3 camDir, float maxDist)
            {
                float dist = 0.0;
                SDF_Surface result;
                for (int i = 0; i < maxSteps; i++)
                {
                    float3 position = camPos + dist * camDir;
                    result = map(position);
                    if (result.signed_distance < 0.001 || dist > maxDist)
                        break;
                    dist += result.signed_distance;
                }
                return result;
            }

            // Phong shading
            float3 phong(float3 lightDir, float3 normal, float3 camDir, SDF_Surface surface)
            {
                float3 diffuse = surface.material.y * max(dot(lightDir, normal), 0.0) * surface.color;
                float3 specular = surface.material.z * pow(max(dot(reflect(-lightDir, normal), camDir), 0.0), surface.material.w);
                return diffuse + specular;
            }

            // Vertex shader
            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldCam : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.worldPos = mul(_ModelMatrix, v.vertex).xyz;
                o.worldCam = mul(_InverseModelMatrix, float4(0, 0, 0, 1)).xyz;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            // Fragment shader
            fixed4 frag(v2f i) : SV_Target
            {
                float depth = tex2D(_DepthTex, i.worldPos.xy).x;
                float3 camDir = normalize(i.worldPos - i.worldCam);
                SDF_Surface result = march(i.worldCam, camDir, depth);

                if (_IsDebug > 0.5)
                {
                    return float4(result.color, 0.5);
                }
                else
                {
                    float3 normal = normalize(float3(0, 0, 1)); // Placeholder for real normal computation
                    float3 lightDir = normalize(float3(0.1, 0.2, 0.4));
                    return float4(phong(lightDir, normal, camDir, result), result.material.w);
                }
            }
            ENDCG
        }
    }
}

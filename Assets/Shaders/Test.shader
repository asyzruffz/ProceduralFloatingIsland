Shader "Custom/Test" {
	Properties {
		_Color("Color", Color) = (1,0,0,1)
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader {
		Tags{ "RenderType" = "Opaque" "LightMode" = "ForwardBase" }

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3	norm : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 wpos : TEXCOORD0;
				float3	norm : NORMAL;
				fixed4 diff : COLOR0;
				//float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD1;
			};

			sampler2D _MainTex;
			fixed4 _Color;

			v2f vert (appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				// compute world space position of the vertex
				o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
				// compute world space view direction
				//float3 worldViewDir = normalize(UnityWorldSpaceViewDir(o.wpos));
				// world space normal
				o.norm = UnityObjectToWorldNormal(v.norm);
				// dot product between normal and light direction for
				// standard diffuse (Lambert) lighting
				half nl = max(0, dot(o.norm, _WorldSpaceLightPos0.xyz));
				// factor in the light color
				o.diff = nl * _LightColor0;
				//o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);

				float3 x = ddx(i.wpos);
				float3 y = ddy(i.wpos);
				float3 vn = normalize(cross(x, y));

				col.rgb *= _Color.rgb;
				col.rgb *= (UNITY_LIGHTMODEL_AMBIENT.rgb + i.diff); // ambient + light colour
				col.rgb *= max(0.0, dot(vn, _WorldSpaceLightPos0.xyz));
				
				return col;
			}
			ENDCG
		}
	}

	FallBack "Diffuse"
}

Shader "Volumetric Audio/Wireframe"
{
	Properties
	{
		_Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Radius("Radius", Float) = 2.0
		_Smooth("Smooth", Float) = 2.0
	}

	SubShader
	{
		Cull Off
		Zwrite Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Tags
		{
			"Queue" = "Transparent" "DisableBatching" = "True"
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			float4 _Color;
			float  _Radius;
			float  _Smooth;

			struct a2v
			{
				float4 vertex    : POSITION;
				float4 texcoord0 : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 pos    : TEXCOORD0;
				float4 posA   : TEXCOORD1;
				float4 posB   : TEXCOORD2;
			};

			struct f2g
			{
				float4 color : SV_TARGET;
			};

			void Intersect(float4 a, inout float4 b)
			{
				float4 d = b - a;

				//b = a + d * min(1.0f, a.z / d.z);
			}

			void Vert(a2v i, out v2f o)
			{
				float4 scale  = float4(_ScreenParams.xy, 1.0f, 1.0f);
				float4 pointA = UnityObjectToClipPos(float4(i.texcoord0.xyz, 1.0f)) * scale; pointA.xyz /= pointA.w;
				float4 pointB = UnityObjectToClipPos(float4(i.texcoord1.xyz, 1.0f)) * scale; pointB.xyz /= pointB.w;
				float4 pointM = lerp(pointA, pointB, i.texcoord1.w);
				float2 pointD = normalize(pointB.xy - pointA.xy);
				float2 pointZ = float2(-pointD.y, pointD.x);

				pointM.xy += pointZ * i.texcoord0.w * _Radius;
				pointM.xy += pointD * i.texcoord1.w * _Radius;

				o.pos    = pointM;
				o.posA   = pointA;
				o.posB   = pointB;
				o.vertex = float4(pointM.xyz * pointM.w, pointM.w) / scale;
			}

			float Dist(float2 a, float2 b, float2 p)
			{
				float2 ba = b - a;
				float2 pa = p - a;
				float  t = saturate(dot(pa, ba) / dot(ba, ba));

				return length(pa - t * ba);
			}

			void Frag(v2f i, out f2g o)
			{
				float dist = Dist(i.posA, i.posB, i.pos) / _Radius;

				o.color = _Color;

				o.color.a *= smoothstep(1.0f, 1.0f - _Smooth / _Radius, dist);
			}
			ENDCG
		} // Pass
	} // SubShader
} // Shader
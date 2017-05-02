Shader "SparkySpark/Additive_Spark" {
Properties {
	_EndColor ("Tip Color", Color) = (0.5,0.5,0.5,0.5)	
	_FadeEnd("Tip Fade", Range(0,1.0)) = 0
	_StartColor ("Tail Color", Color) = (0.5,0.5,0.5,0.5)	
	_FadeStart("Tail Fade", Range(0,1.0)) = 0
	_HighlightColor("Highlight Color",Color) = (1,1,1,1)
	_HighlightStrength ("Highlight Strength", Range(0.0,1.0)) = 0.5	
	_MainTex ("Texture", 2D) = "white" {}
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha One
	AlphaTest Greater .01
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
	
	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _StartColor;
			fixed4 _EndColor;
			fixed4 _HighlightColor;
			half _HighlightStrength;
			half _FadeEnd;
			half _FadeStart;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;												
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);				
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);				
				return o;
			}

			sampler2D _CameraDepthTexture;
			
			fixed4 frag (v2f i) : COLOR
			{				
			
				float4 c = tex2D(_MainTex, i.texcoord);
				half fadeCoef = saturate((1-i.texcoord.x)/_FadeEnd) * saturate((i.texcoord.x)/_FadeStart);
				float4 col = i.color * lerp(lerp(_StartColor,_EndColor,i.texcoord.x),_HighlightColor,_HighlightStrength * c.r) * c * 2.0f;
				c = fadeCoef * (col);
				return c;
			}
			ENDCG 
		}
	}	
}
}

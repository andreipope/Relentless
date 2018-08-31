Shader "Sprites/Opaque"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Geometry"
			"IgnoreProjector"="True"
			"RenderType"="Opaque"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite On
		ZTest LEqual
		Fog { Mode Off }
		Blend Off

		Pass
		{
		CGPROGRAM
			#pragma fragment frag
			#pragma vertex SpriteVert
			#pragma target 2.0
			#pragma multi_compile_instancing
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnitySprites.cginc"

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
				return c;
			}
		ENDCG
		}
	}
}

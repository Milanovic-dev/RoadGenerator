Shader "Nimi/Outline"
{
	Properties
	{
		_MainText("Texture", 2D) = "white" {}
		_OutlineColor("Outline color", Color) = (1,1,1,1)
		_OutlineWidth("Outline width", Float) = 1
	}

	CGINCLUDE
#include "UnityCG.cginc"

		struct appdata {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
		};

		struct v2f {
			float4 pos : POSITION;
			float4 color : COLOR;
			float3 normal : NORMAL;
		};

		float4 _OutlineColor;
		float _OutlineWidth;

		v2f vert(appdata v) {

			v.vertext.xyz *= _OutlineWidth;

			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.color = _OutlineColor;
			return o;
		};

		SubShader{
			Pass
			{
			}
		}

		ENDCG
}

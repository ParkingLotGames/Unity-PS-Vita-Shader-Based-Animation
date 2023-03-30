/*
Created by jiadong chen
https://jiadong-chen.medium.com/
*/

Shader "chenjd/BuiltIn/AnimMapShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_AnimTex("Baked Animation Texture", 2D) = "white" {}
		_AnimationDurationScale("Animation Duration Scale", Float) = 1
	}

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Cull off
        LOD 100

        Pass
        {

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #define ANIM_ROTMAT animationRotationCorrectionMatrix
        float4x4 animationRotationCorrectionMatrix = float4x4(-1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1);
        #include "UnityCG.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            float index : TEXCOORD1;
        };

        struct v2f
        {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        sampler2D _MainTex;
        sampler2D _AnimTex;
        
        int _VertexNum;

        float _AnimationDurationScale;

        float4 _MainTex_ST;
        float4 _AnimTex_TexelSize;

        v2f vert(appdata v)
        {

            float animationDuration = _Time.y / _AnimationDurationScale;
            fmod(animationDuration, 1.0);

            float index = v.index * _VertexNum;
            float2 animUV = 0;
            animUV.x = (index + 0.5) * _AnimTex_TexelSize.x;
            animUV.y = animationDuration;
            float4 vertex = tex2Dlod(_AnimTex, float4(animUV, 0, 0));

            vertex = mul(ANIM_ROTMAT, vertex);
            vertex.y -= 0.4065;

            v2f o;
            o.pos = UnityObjectToClipPos(vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            return o;
        }


        fixed4 frag(v2f i) : SV_Target
        {
            fixed4 col = tex2D(_MainTex, i.uv);
            return col;
        }

        ENDCG
        }
    }
}

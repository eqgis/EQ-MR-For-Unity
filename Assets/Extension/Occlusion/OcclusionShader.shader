Shader"Holo/OcclusionShader"
{
	Properties
	{
         _MainTex("Texture", 2D) = "white" {}
         _Alpha("Alpha",Range(0.00,1.00)) = 0

        _ShadowIntensity("Shadow Intensity", Range(0, 1)) = 0.1
	}
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry-100" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            Tags {"LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase



            #include "UnityCG.cginc"
            #include "AutoLight.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                LIGHTING_COORDS(0, 1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Alpha;
            uniform float _ShadowIntensity;


            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float shadow = (1 - LIGHT_ATTENUATION(i)) * _ShadowIntensity;
                fixed4 shadow_col = fixed4(0, 0, 0, 1);
                fixed4 main_col = tex2D(_MainTex, i.uv) * _Alpha;

                fixed4 col = shadow_col * shadow + main_col * (1 - shadow);
                return col;
            }
            ENDCG
        }
    }
	Fallback "Diffuse"
}
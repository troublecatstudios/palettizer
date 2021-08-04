Shader "TroubleCat/Sprites/Palette-Swap-Unlit"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _PaletteTex("Palette Texture", 2D) = "white" {}
        _MapTex("Palette Map Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [MaterialToggle] PaletteEnabled("Enable Palette", Float) = 0
        [MaterialToggle] ShowMap("Show Map", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
    }
        SubShader
        {

            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Cull Off
            Lighting Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;

                sampler2D _PaletteTex;
                float4 _PaletteTex_ST;
                float4 _PaletteTex_TexelSize;

                sampler2D _MapTex;
                float4 _MapTex_ST;
                float4 _MapTex_TexelSize;

                float4 _Color;

                float PaletteEnabled;
                float ShowMap;

                struct v2f
                {
                    float4  pos : SV_POSITION;
                    float2  uv : TEXCOORD0;
                };

                v2f vert(appdata_base IN)
                {
                    v2f OUT;

                    OUT.pos = UnityObjectToClipPos(IN.vertex);
                    OUT.uv = IN.texcoord;

                    return OUT;
                }

                float4 frag(v2f IN) : SV_Target
                {
                    // sample the main texture... pretty standard stuff
                    float4 mainTex = tex2D(_MainTex, IN.uv);

                    // MapTex is a basic Texture2D you might
                    // want to change this to a [PerRendererData] 
                    // and assign via MaterialPropertyBlock
                    float4 mapTex = tex2D(_MapTex, IN.uv);

                    // scale up the RED (x) and GREEN (y) values from the color
                    int x = round(mapTex.x * 255) / 8;
                    int y = round(mapTex.y * 255) / 8;
                    
                    // convert the values to their respective index within the palette
                    x -= 1;
                    y -= 1;

                    // convert the color values into texture space coords
                    float2 uv = float2(x * _PaletteTex_TexelSize.x, y * _PaletteTex_TexelSize.y);

                    // PaletteTex is a basic Texture2D you might
                    // want to change this to a [PerRendererData] 
                    // and assign via MaterialPropertyBlock
                    float4 palette = tex2D(_PaletteTex, uv);  // read the palette color value

                    float4 c;
                    c.rgb = palette.rgb;
                    c.a = mainTex.a * palette.a; // palette alpha overrides the main texture alpha
                                                 // useful if you want to hide parts of your sprite

                    // PaletteEnabled is a [MaterialToggle] float
                    // you might want to change this to a 
                    // [PerRendererData] and assign via MaterialPropertyBlock
                    // if its not set, fallback to the main texture data
                    if (PaletteEnabled == 0.0) {
                        c.rgb = mainTex.rgb;
                        c.a = mainTex.a;
                    }

                    // ShowMap is a [MaterialToggle] float
                    // if its set, then show the map data instead
                    if (ShowMap == 1.0) {
                        c.rg = mapTex.xy;
                        c.b = 0;
                        return c;
                    }

                    return c;
                }
                ENDCG
            }

        }
}

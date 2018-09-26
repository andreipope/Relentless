// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:Particles/Additive,iptp:0,cusa:False,bamd:0,cgin:,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:5510,x:32970,y:32688,varname:node_5510,prsc:2|emission-5708-OUT;n:type:ShaderForge.SFN_Tex2d,id:8274,x:31333,y:32620,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-2985-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9244,x:31323,y:32932,ptovrint:False,ptlb:Strength,ptin:_Strength,varname:_Strength,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Color,id:523,x:31809,y:33086,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_VertexColor,id:1002,x:32061,y:32954,varname:node_1002,prsc:2;n:type:ShaderForge.SFN_Multiply,id:1780,x:32054,y:32837,varname:node_1780,prsc:2|A-1182-OUT,B-523-RGB,C-523-A,D-8274-RGB;n:type:ShaderForge.SFN_Multiply,id:1182,x:31497,y:32770,varname:node_1182,prsc:2|A-8274-RGB,B-9244-OUT,C-8274-A;n:type:ShaderForge.SFN_Multiply,id:6926,x:32335,y:32845,varname:node_6926,prsc:2|A-1780-OUT,B-1002-RGB,C-1002-A,D-4442-RGB;n:type:ShaderForge.SFN_Tex2d,id:4442,x:32170,y:33188,ptovrint:False,ptlb:Mask,ptin:_Mask,varname:_Mask,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-1120-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3909,x:31327,y:33317,ptovrint:False,ptlb:Mask_U,ptin:_Mask_U,varname:_Mask_U,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:4141,x:31327,y:33471,ptovrint:False,ptlb:Mask_V,ptin:_Mask_V,varname:_Mask_V,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Append,id:783,x:31548,y:33382,varname:node_783,prsc:2|A-3909-OUT,B-4141-OUT;n:type:ShaderForge.SFN_Time,id:5465,x:31567,y:33554,varname:node_5465,prsc:2;n:type:ShaderForge.SFN_Multiply,id:5413,x:31741,y:33454,varname:node_5413,prsc:2|A-783-OUT,B-5465-T;n:type:ShaderForge.SFN_TexCoord,id:3009,x:31741,y:33304,varname:node_3009,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Add,id:1120,x:31942,y:33379,varname:node_1120,prsc:2|A-3009-UVOUT,B-5413-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9251,x:30426,y:32587,ptovrint:False,ptlb:MainTex_U,ptin:_MainTex_U,varname:_MainTex_U,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:2023,x:30426,y:32741,ptovrint:False,ptlb:MainTex_V,ptin:_MainTex_V,varname:_MainTex_V,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Append,id:2393,x:30647,y:32652,varname:node_2393,prsc:2|A-9251-OUT,B-2023-OUT;n:type:ShaderForge.SFN_Time,id:6041,x:30666,y:32824,varname:node_6041,prsc:2;n:type:ShaderForge.SFN_Multiply,id:7557,x:30840,y:32724,varname:node_7557,prsc:2|A-2393-OUT,B-6041-T;n:type:ShaderForge.SFN_TexCoord,id:9791,x:30840,y:32574,varname:node_9791,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Add,id:2985,x:31042,y:32649,varname:node_2985,prsc:2|A-9791-UVOUT,B-7557-OUT;n:type:ShaderForge.SFN_Tex2d,id:4988,x:32292,y:33596,ptovrint:False,ptlb:Mask2,ptin:_Mask2,varname:_Mask2,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-5349-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1278,x:31449,y:33725,ptovrint:False,ptlb:Mask2_U,ptin:_Mask2_U,varname:_Mask2_U,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:3376,x:31449,y:33878,ptovrint:False,ptlb:Mask2_V,ptin:_Mask2_V,varname:_Mask2_V,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Append,id:5345,x:31669,y:33789,varname:node_5345,prsc:2|A-1278-OUT,B-3376-OUT;n:type:ShaderForge.SFN_Time,id:7777,x:31689,y:33961,varname:node_7777,prsc:2;n:type:ShaderForge.SFN_Multiply,id:4928,x:31863,y:33861,varname:node_4928,prsc:2|A-5345-OUT,B-7777-T;n:type:ShaderForge.SFN_TexCoord,id:7,x:31863,y:33712,varname:node_7,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Add,id:5349,x:32064,y:33787,varname:node_5349,prsc:2|A-7-UVOUT,B-4928-OUT;n:type:ShaderForge.SFN_Multiply,id:5708,x:32631,y:32954,varname:node_5708,prsc:2|A-6926-OUT,B-4988-RGB;proporder:8274-9251-2023-523-9244-4442-3909-4141-4988-1278-3376;pass:END;sub:END;*/

Shader "pXs/ThreeTexturedParticle" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _MainTex_U ("MainTex_U", Float ) = 0
        _MainTex_V ("MainTex_V", Float ) = 0
        _Color ("Color", Color) = (0.5,0.5,0.5,1)
        _Strength ("Strength", Float ) = 0
        _Mask ("Mask", 2D) = "white" {}
        _Mask_U ("Mask_U", Float ) = 0
        _Mask_V ("Mask_V", Float ) = 0
        _Mask2 ("Mask2", 2D) = "white" {}
        _Mask2_U ("Mask2_U", Float ) = 0
        _Mask2_V ("Mask2_V", Float ) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 200
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x xboxone ps4 psp2 n3ds wiiu 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _Strength;
            uniform float4 _Color;
            uniform sampler2D _Mask; uniform float4 _Mask_ST;
            uniform float _Mask_U;
            uniform float _Mask_V;
            uniform float _MainTex_U;
            uniform float _MainTex_V;
            uniform sampler2D _Mask2; uniform float4 _Mask2_ST;
            uniform float _Mask2_U;
            uniform float _Mask2_V;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 node_6041 = _Time;
                float2 node_2985 = (i.uv0+(float2(_MainTex_U,_MainTex_V)*node_6041.g));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_2985, _MainTex));
                float4 node_5465 = _Time;
                float2 node_1120 = (i.uv0+(float2(_Mask_U,_Mask_V)*node_5465.g));
                float4 _Mask_var = tex2D(_Mask,TRANSFORM_TEX(node_1120, _Mask));
                float4 node_7777 = _Time;
                float2 node_5349 = (i.uv0+(float2(_Mask2_U,_Mask2_V)*node_7777.g));
                float4 _Mask2_var = tex2D(_Mask2,TRANSFORM_TEX(node_5349, _Mask2));
                float3 emissive = ((((_MainTex_var.rgb*_Strength*_MainTex_var.a)*_Color.rgb*_Color.a*_MainTex_var.rgb)*i.vertexColor.rgb*i.vertexColor.a*_Mask_var.rgb)*_Mask2_var.rgb);
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x xboxone ps4 psp2 n3ds wiiu 
            #pragma target 3.0
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Particles/Additive"
    CustomEditor "ShaderForgeMaterialInspector"
}

// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:9511,x:33283,y:32675,varname:node_9511,prsc:2|diff-6251-OUT;n:type:ShaderForge.SFN_Color,id:6278,x:31705,y:32427,ptovrint:False,ptlb:FrenelColor,ptin:_FrenelColor,varname:_FrenelColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.6698113,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Code,id:6251,x:32351,y:32764,varname:node_6251,prsc:2,code:aQBmACAAKABXAG8AcgBsAGQAUABvAHMALgB6ACAAPgAgAEwAaQBtAGkAdABZACkAIABkAGkAcwBjAGEAcgBkADsACgByAGUAdAB1AHIAbgAgAEkAbgBDAG8AbABvAHIAOwA=,output:2,fname:Discard,width:618,height:174,input:2,input:2,input:0,input_1_label:InColor,input_2_label:WorldPos,input_3_label:LimitY|A-2959-OUT,B-953-XYZ,C-3846-OUT;n:type:ShaderForge.SFN_FragmentPosition,id:953,x:31873,y:32860,varname:node_953,prsc:2;n:type:ShaderForge.SFN_Vector1,id:2524,x:32064,y:32894,varname:node_2524,prsc:2,v1:30;n:type:ShaderForge.SFN_Tex2d,id:2470,x:31895,y:32398,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Fresnel,id:860,x:31592,y:32794,varname:node_860,prsc:2|EXP-5981-OUT;n:type:ShaderForge.SFN_ValueProperty,id:5981,x:31425,y:32810,ptovrint:False,ptlb:FresnelAmount,ptin:_FresnelAmount,varname:_FresnelAmount,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:9924,x:31768,y:32680,varname:node_9924,prsc:2|A-4929-OUT,B-860-OUT;n:type:ShaderForge.SFN_ValueProperty,id:4929,x:31533,y:32662,ptovrint:False,ptlb:FrenelSTR,ptin:_FrenelSTR,varname:_FrenelSTR,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:9643,x:31941,y:32649,varname:node_9643,prsc:2|A-6278-RGB,B-9924-OUT;n:type:ShaderForge.SFN_Add,id:2959,x:32123,y:32432,varname:node_2959,prsc:2|A-2470-RGB,B-9643-OUT;n:type:ShaderForge.SFN_Color,id:5703,x:32819,y:33008,ptovrint:False,ptlb:OpacityColor,ptin:_OpacityColor,varname:_OpacityColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:3846,x:32178,y:32976,ptovrint:False,ptlb:WorldPos,ptin:_WorldPos,varname:_WorldPos,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:30;proporder:6278-2470-5981-4929-5703-3846;pass:END;sub:END;*/
Shader "Unlit/DiscardAxisY" {
    Properties {
        _FrenelColor ("FrenelColor", Color) = (0.6698113,0,0,1)
        _MainTex ("MainTex", 2D) = "white" {}
        _FresnelAmount ("FresnelAmount", Float ) = 1
        _FrenelSTR ("FrenelSTR", Float ) = 1
        _OpacityColor ("OpacityColor", Color) = (0.5,0.5,0.5,1)
        _WorldPos ("WorldPos", Float ) = 30
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        LOD 100
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x xboxone ps4 psp2 n3ds wiiu switch 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform float4 _FrenelColor;
            float3 Discard( float3 InColor , float3 WorldPos , float LimitY ){
            if (WorldPos.z > LimitY) discard;
            return InColor;
            }
            
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _FresnelAmount;
            uniform float _FrenelSTR;
            uniform float _WorldPos;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                LIGHTING_COORDS(3,4)
                UNITY_FOG_COORDS(5)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                UNITY_LIGHT_ATTENUATION(attenuation,i, i.posWorld.xyz);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float3 diffuseColor = Discard( (_MainTex_var.rgb+(_FrenelColor.rgb*(_FrenelSTR*pow(1.0-max(0,dot(normalDirection, viewDirection)),_FresnelAmount)))) , i.posWorld.rgb , _WorldPos );
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x xboxone ps4 psp2 n3ds wiiu switch 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform float4 _FrenelColor;
            float3 Discard( float3 InColor , float3 WorldPos , float LimitY ){
            if (WorldPos.z > LimitY) discard;
            return InColor;
            }
            
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _FresnelAmount;
            uniform float _FrenelSTR;
            uniform float _WorldPos;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                LIGHTING_COORDS(3,4)
                UNITY_FOG_COORDS(5)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                UNITY_LIGHT_ATTENUATION(attenuation,i, i.posWorld.xyz);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float3 diffuseColor = Discard( (_MainTex_var.rgb+(_FrenelColor.rgb*(_FrenelSTR*pow(1.0-max(0,dot(normalDirection, viewDirection)),_FresnelAmount)))) , i.posWorld.rgb , _WorldPos );
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}

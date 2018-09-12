
Shader "Custom/ZMask" {
 
    Properties {
    }
 
    SubShader {
        LOD 100
   
        Tags { "Queue"="Geometry-50" "IgnoreProjector"="True" "RenderType"="Opaque" }
       
        Pass {
            ColorMask 0
        }
    }
}
 
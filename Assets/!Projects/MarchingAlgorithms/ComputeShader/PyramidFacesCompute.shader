Shader "Unlit/PyramidFacesCompute"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorTint ("Color Tint", Color) = (1,1,1,1) 
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        
        Pass
        {
            Name "ForwardLit"
            Tags {"LightMode" = "UniversalForward"}

            HLSLPROGRAM
            //signal this shader requires compute buffer
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 5.0 
            //earliest target which supports compute shaders

            //Lighting and shadow keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            //Register out functions
            #pragma vertex Vertex
            #pragma fragment Fragment

            //Include logic file
            #include "PyramidFacesCompute.hlsl"

            ENDHLSL
        }

        //shadow caster pass. this pass renders a shadow map
        //we trat it almost the same, except sri[t out any color/ lightinf logic
        Pass
        {
            Name "ShadowCaster"
            Tags {"LightMode" = "ShadowCaster"}

            HLSLPROGRAM
            //signal this shader requires compute buffer
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 5.0 
            //earliest target which supports compute shaders

            //Lighting and shadow keywords
            #pragma multi_compile _shadowcaster

            //Register out functions
            #pragma vertex Vertex
            #pragma fragment Fragment

            //define a special keyword so our logic can change if inside the shadow caster pass
            #define SHADOW_CASTER_PASS

            //Include logic file
            #include "PyramidFacesCompute.hlsl"

            ENDHLSL
        }
    }
}

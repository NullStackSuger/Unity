#version 450

/*glslc D://Rider//Project//Unity//UnityEngine//Render//RenderPass//Object//object.frag -o D://Rider//Project//Unity//Sandbox//Asset//Shaders//Object//object.frag.spv*/

layout(location = 0) in vec3 fragColor;
layout(location = 1) in vec2 fragUV;
layout(location = 2) in vec3 fragWorldPos;

layout(location = 0) out vec4 outColor;

layout (set = 0, binding = 0) uniform MVP
{
    mat4 model;
    mat4 view;
    mat4 projection;
} mvp;

layout (set = 0, binding = 1) uniform LightVP
{
    mat4 vp;
} lightVp;

layout(set = 0, binding = 2) uniform sampler2D shadowMap;

void main() 
{
    vec4 shadowCoord = lightVp.vp * vec4(fragWorldPos, 1.0);
    shadowCoord /= shadowCoord.w;
    shadowCoord = shadowCoord * 0.5 + 0.5;

    float shadowDepth = texture(shadowMap, shadowCoord.xy).r;
    float currentDepth = shadowCoord.z;

    float bias = 0.005;
    float shadow = currentDepth - bias > shadowDepth ? 0.3 : 1.0;
    
    outColor = vec4(fragColor * shadow, 1.0);
}
#version 450
#extension GL_ARB_separate_shader_objects : enable

// glslc D://Rider//Project//Unity//UnityEngine//Render//Shaders//uiShader.frag -o D://Rider//Project//Unity//Sandbox//bin//Debug//net8.0-windows//Shaders//uiShader.frag.spv

layout(set = 1, binding = 0) uniform texture2D FontTexture;
layout(set = 0, binding = 1) uniform sampler FontSampler;

layout (location = 0) in vec4 color;
layout (location = 1) in vec2 uv;
layout (location = 0) out vec4 outColor;

void main()
{
    outColor = color * texture(sampler2D(FontTexture, FontSampler), uv);
}
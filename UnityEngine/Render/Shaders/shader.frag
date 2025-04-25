#version 450
#extension GL_ARB_separate_shader_objects : enable

// glslc D://Rider//Project//Unity//UnityEngine//Render//Shaders//shader.frag -o D://Rider//Project//Unity//Sandbox//bin//Debug//net8.0-windows//Shaders//frag.spv

layout(location = 0) in vec3 fragColor;

layout(location = 0) out vec4 outColor;

void main() 
{
    outColor = vec4(fragColor, 1.0);
}
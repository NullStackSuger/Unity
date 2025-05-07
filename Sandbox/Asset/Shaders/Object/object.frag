#version 450

/*glslc D://Rider//Project//Unity//UnityEngine//Render//RenderPass//Object//object.frag -o D://Rider//Project//Unity//Sandbox//bin//Debug//net8.0-windows//Shaders//Object//object.frag.spv*/

layout(location = 0) in vec3 fragColor;

layout(location = 0) out vec4 outColor;

void main() 
{
    outColor = vec4(fragColor, 1.0);
}
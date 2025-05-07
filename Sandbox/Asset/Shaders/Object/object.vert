#version 450

/*glslc D://Rider//Project//Unity//UnityEngine//Render//RenderPass//Object//object.vert -o D://Rider//Project//Unity//Sandbox//bin//Debug//net8.0-windows//Shaders//Object//object.vert.spv*/

layout(location = 0) out vec3 fragColor;

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 uv;

layout (set = 0, binding = 0) uniform MVP
{
    mat4 model;
    mat4 view;
    mat4 projection;
} mvp;

void main() 
{
    gl_Position = mvp.projection * mvp.view * mvp.model * vec4(position, 1.0);
    fragColor = vec3(uv, 0.0);
}
#version 450

// glslc D://Rider//Project//Unity//UnityEngine//Render//RenderPass//Shadow//shadow.vert -o D://Rider//Project//Unity//Sandbox//Asset//Shaders///Shadow//shadow.vert.spv

layout(location = 0) in vec3 position;

layout (set = 0, binding = 0) uniform Model
{
    mat4 model;
} model;

layout (set = 0, binding = 1) uniform LightVp
{
    mat4 view;
    mat4 projection;
} lightVp;

void main()
{
    gl_Position = lightVp.projection * lightVp.view * model.model * vec4(position, 1.0);
}
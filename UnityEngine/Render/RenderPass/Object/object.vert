#version 450

/*glslc D://Rider//Project//Unity//UnityEngine//Render//RenderPass//Object//object.vert -o D://Rider//Project//Unity//Sandbox//Asset//Shaders///Object//object.vert.spv*/

layout(location = 0) out vec3 fragColor;
layout(location = 1) out vec2 fragUV;
layout(location = 2) out vec3 fragWorldPos;

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 uv;

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

void main() 
{
    fragColor = vec3(1.0, 1.0, 1.0);
    fragUV = uv;
    fragWorldPos = (mvp.model * vec4(position, 1.0)).xyz;
    gl_Position = mvp.projection * mvp.view * mvp.model * vec4(position, 1.0);
}
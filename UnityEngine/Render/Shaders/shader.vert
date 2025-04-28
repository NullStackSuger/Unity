#version 450
#extension GL_ARB_separate_shader_objects : enable

// glslc D://Rider//Project//Unity//UnityEngine//Render//Shaders//shader.vert -o D://Rider//Project//Unity//Sandbox//bin//Debug//net8.0-windows//Shaders//shader.vert.spv

layout(location = 0) out vec3 fragColor;

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 color;
layout(location = 2) in vec2 uv;

layout (set = 0, binding = 0) uniform UBO
{
    mat4 view;
    mat4 projection;
} ubo;

layout (push_constant) uniform PushConstant
{
    mat4 model;
} pushConstant;

void main() {
    gl_Position = ubo.projection * ubo.view * pushConstant.model * vec4(position, 1.0);
    fragColor = vec3(uv, 0.0);
}
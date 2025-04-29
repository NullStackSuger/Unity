#version 450
#extension GL_ARB_separate_shader_objects : enable

// glslc D://Rider//Project//Unity//UnityEngine//Render//Shaders//uiShader.vert -o D://Rider//Project//Unity//Sandbox//bin//Debug//net8.0-windows//Shaders//uiShader.vert.spv

layout (location = 0) in vec2 position;
layout (location = 1) in vec2 uv;
layout (location = 2) in vec4 color;

layout (binding = 0) uniform UiUniform
{
    mat4 projection;
};

layout (location = 0) out vec4 fragColor;
layout (location = 1) out vec2 texCoord;

void main() 
{
    gl_Position = projection * vec4(position, 0, 1);
    fragColor = color;
    texCoord = uv;
}

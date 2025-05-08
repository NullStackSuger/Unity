#version 450

/*glslc D://Rider//Project//Unity//UnityEngine//Render//RenderPass//Shadow//shadow.vert -o D://Rider//Project//Unity//Sandbox//Asset//Shaders///Shadow//shadow.vert.spv*/

layout(location = 0) in vec3 position;

layout (set = 0, binding = 0) uniform MVP
{
    mat4 model;
    mat4 view;
    mat4 projection;
} mvp;

void main()
{
    gl_Position = mvp.projection * mvp.view * mvp.model * vec4(position, 1.0);
}
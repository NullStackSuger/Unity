#version 450

// glslc D://Rider//Project//Unity//UnityEngine//Render//RenderPass//Object//object.frag -o D://Rider//Project//Unity//Sandbox//Asset//Shaders//Object//object.frag.spv

layout(location = 0) in vec2 fragUV;
layout(location = 1) in vec3 fragWorldNormal;
layout(location = 2) in vec3 fragWorldPos;

layout(location = 0) out vec4 outColor;

layout (set = 0, binding = 1) uniform Light
{
    mat4 view;
    mat4 projection;
    vec3 dir;
    float intensity;
    vec3 color;
} light;

layout(set = 0, binding = 2) uniform texture2D shadowMap;
layout(set = 0, binding = 3) uniform sampler shadowMapSampler;

void main() 
{
    vec4 shadowCoord = light.projection * light.view * vec4(fragWorldPos, 1.0);
    shadowCoord /= shadowCoord.w;
    // 不需要这一句, 我们本来就是[0, 1]了
    //shadowCoord = shadowCoord * 0.5 + 0.5;
    
    float shadowDepth = texture(sampler2D(shadowMap, shadowMapSampler), shadowCoord.xy).r;
    float currentDepth = shadowCoord.z;
    
    float shadow = currentDepth > shadowDepth ? 0.0 : 1.0;

    // 漫反射光照
    vec3 normal = normalize(fragWorldNormal);
    float ndl = max(dot(normal, -light.dir), 0.0);
    vec3 baseColor = vec3(1.0); // 可替换为材质颜色
    vec3 diffuse = baseColor * light.color * ndl * light.intensity;

    outColor = vec4(diffuse * shadow, 1.0);
}
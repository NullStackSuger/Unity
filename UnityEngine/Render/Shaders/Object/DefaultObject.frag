#version 450

// glslc D://Rider//Project//Unity//UnityEngine//Render//Shaders//Object//DefaultObject.frag -o D://Rider//Project//Unity//Sandbox//Asset//Shaders//Object//DefaultObject.frag.spv

layout(location = 0) in vec2 fragUV;
layout(location = 1) in vec3 fragWorldNormal;
layout(location = 2) in vec3 fragWorldPos;

layout(location = 0) out vec4 outColor;

layout (set = 0, binding = 2) uniform Light
{
    mat4 view;
    mat4 projection;
    vec3 dir;
    float intensity;
    vec3 color;
} light;

layout(set = 0, binding = 3) uniform texture2D shadowMap;
layout(set = 0, binding = 4) uniform sampler shadowMapSampler;

void main() 
{
    vec4 shadowCoord = light.projection * light.view * vec4(fragWorldPos, 1.0);
    shadowCoord /= shadowCoord.w;
    // 这里要注意, xy的范围是[-1, 1], z是[0, 1]
    shadowCoord.xy = shadowCoord.xy * 0.5 + 0.5;
    
    float shadowDepth = texture(sampler2D(shadowMap, shadowMapSampler), shadowCoord.xy).r;
    float currentDepth = shadowCoord.z;
    
    float bias = 0.01;
    // 同时这里注意要有=
    float shadow = currentDepth - bias >= shadowDepth ? 0.3 : 1.0;

    // 漫反射光照
    vec3 normal = normalize(fragWorldNormal);
    float ndl = max(dot(normal, -light.dir), 0.0);
    vec3 baseColor = vec3(1.0); // 可替换为材质颜色
    vec3 diffuse = baseColor * light.color * ndl * light.intensity;
    
    outColor = vec4(diffuse * shadow, 1.0);
}
shader_type spatial;
render_mode world_vertex_coords;

// TODO: little white pixels for smooth materials like human's bullet hole when moving and looking
// at the decals (appear at Runtime).
// TODO: handle metalic parameter properly (it appears too dark when the material is pretty metalic)
// TODO: materials with low roughness appear too white when looking at them with sharp angle
uniform vec4 albedo : source_color;  
uniform sampler2D texture_albedo : source_color;
uniform sampler2D DEPTH_TEXTURE: hint_depth_texture;
uniform float cube_half_size = 1.0;
uniform float alpha_threshold = 0.1; 
uniform sampler2D normal_map : hint_normal;
uniform sampler2D orm_map : source_color;
varying mat4 INV_MODEL_MATRIX;

void vertex() {
    INV_MODEL_MATRIX = inverse(MODEL_MATRIX);
}

// Fresnel calculation using Schlick's approximation
vec3 fresnel_schlick(float cos_theta, vec3 F0) {
    return F0 + (1.0 - F0) * pow(1.0 - cos_theta, 5.0);
}

// GGX normal distribution function (NDF)
float distribution_ggx(vec3 N, vec3 H, float roughness) {
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH * NdotH;

    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    return a2 / (PI * denom * denom);
}

// Geometry function for specular occlusion
float geometry_schlick_ggx(float NdotV, float roughness) {
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;
    return NdotV / (NdotV * (1.0 - k) + k);
}

// Full geometry function for both view and light directions
float geometry_smith(vec3 N, vec3 V, vec3 L, float roughness) {
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx1 = geometry_schlick_ggx(NdotV, roughness);
    float ggx2 = geometry_schlick_ggx(NdotL, roughness);
    return ggx1 * ggx2;
}

// Converts depth to world position
vec3 world_pos_from_depth(float depth, vec2 screen_uv, mat4 inverse_proj, mat4 inverse_view) {
    float z = depth;
    vec4 clipSpacePosition = vec4(screen_uv * 2.0 - 1.0, z, 1.0);
    vec4 viewSpacePosition = inverse_proj * clipSpacePosition;
    viewSpacePosition /= viewSpacePosition.w;
    vec4 worldSpacePosition = inverse_view * viewSpacePosition;
    return worldSpacePosition.xyz;
}

void fragment() {
    float depth = texture(DEPTH_TEXTURE, SCREEN_UV).x;
    vec3 world_pos = world_pos_from_depth(depth, SCREEN_UV, INV_PROJECTION_MATRIX, INV_VIEW_MATRIX);
    vec4 test_pos = (INV_MODEL_MATRIX * vec4(world_pos, 1.0));

    if (abs(test_pos.x) > cube_half_size || abs(test_pos.y) > cube_half_size || abs(test_pos.z) > cube_half_size) {
        discard;
    }

    vec4 tex_color = texture(texture_albedo, (test_pos.xz) + 0.5);
    ALBEDO = tex_color.rgb * albedo.rgb;

    if (tex_color.a < alpha_threshold) {
        discard;
    }
    ALPHA = tex_color.a * albedo.a;

    vec3 normal = texture(normal_map, (test_pos.xz) + 0.5).xyz * 2.0 - 1.0;
    normal = normalize(normal);

    vec4 orm = texture(orm_map, (test_pos.xz) + 0.5);
    float occlusion = orm.r;
    float roughness = clamp(orm.g, 0.05, 1.0);  // Clamping roughness to prevent sharp highlights
    float metallic = orm.b;

    vec3 light_dir = normalize(vec3(1.0, 1.0, -1.0));  // Example light direction
    vec3 view_dir = normalize(-world_pos);  // Camera assumed to be at (0,0,0)
    vec3 halfway_dir = normalize(light_dir + view_dir); 

    float NdotL = max(dot(normal, light_dir), 0.0);
    float NdotV = max(dot(normal, view_dir), 0.0);

    // Base reflectivity for metals comes from the albedo, for non-metals it's a constant (e.g., 0.04)
    vec3 F0 = mix(vec3(0.04), ALBEDO, metallic);

    // Fresnel effect with clamping for better precision
    vec3 F = fresnel_schlick(clamp(dot(halfway_dir, view_dir), 0.0, 1.0), F0);
    float D = distribution_ggx(normal, halfway_dir, roughness);
    float G = geometry_smith(normal, view_dir, light_dir, roughness);

    // Specular component with adjusted denominator to avoid artifacts
    vec3 numerator = D * G * F;
    float denominator = 4.0 * NdotV * NdotL + 0.01;  // Increased epsilon to prevent division by zero
    vec3 specular = clamp(numerator / denominator, 0.0, 1.0);  // Clamp specular intensity

    // For metallic objects, the diffuse component should be minimal
    vec3 kS = F;  // Specular component
    vec3 kD = vec3(1.0) - kS;  // Diffuse component
    kD *= 1.0 - metallic;  // Reduce diffuse lighting for metallic objects

    // Apply ambient, diffuse, and specular lighting
    vec3 ambient = ALBEDO * occlusion;
    vec3 diffuse = kD * ALBEDO * NdotL;  // Only apply diffuse for non-metallic surfaces
    vec3 final_color = ambient + diffuse + specular * NdotL;

    ALBEDO = final_color;
    ROUGHNESS = roughness;
    METALLIC = metallic;
}

void light() {
    DIFFUSE_LIGHT += clamp(dot(NORMAL, LIGHT), 0.0, 1.0) * ATTENUATION * LIGHT_COLOR;
}

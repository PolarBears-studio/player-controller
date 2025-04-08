shader_type spatial;

uniform float edge_threshold : hint_range(0.0, 1.0) = 0.1; // Control the size of the fade effect

void fragment() {
    vec2 uv = FRAGCOORD.xy / VIEWPORT_SIZE.xy;
    
    // Detect how close we are to the edges
    float edge_distance = min(uv.x, uv.y);
    edge_distance = min(edge_distance, 1.0 - uv.x);
    edge_distance = min(edge_distance, 1.0 - uv.y);
    
    // Fade out near the edges
    float fade = smoothstep(edge_threshold, 0.0, edge_distance);
    
    // Modify alpha to create the edge fade effect
    ALBEDO = vec3(1.0); // Your object's color here
    ALPHA = fade;
}
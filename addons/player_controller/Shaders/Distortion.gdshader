shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform float screen_darkness = 0.0;
uniform float darkness_progression = 0.0;

uniform sampler2D smoke;
uniform float size: hint_range(0.0, 1.0);

uniform vec2 uv_offset;

vec4 lerp(vec4 a, vec4 b, float t){
	vec4 val = vec4(
		(b.x * t) + a.x * (1.0 - t),
		(b.y * t) + a.y * (1.0 - t),
		(b.z * t) + a.z * (1.0 - t),
		(b.w * t) + a.w * (1.0 - t));
	
	return clamp(val, 0, 1); 
}

// Function to remap a value from one range to another
float remap(float value, float old_min, float old_max, float new_min, float new_max) {
    return new_min + (value - old_min) * (new_max - new_min) / (old_max - old_min);
}

void fragment(){
	vec2 smoke_uv = UV;
    smoke_uv += uv_offset;
	
	vec4 smoke_color = texture(smoke, fract(smoke_uv));
	
	float size_remapped = remap(size, 0.0, 1.0, 0.0, 0.008);
	
	smoke_color = clamp(smoke_color * size_remapped, 0.0, 1.0);
	
	vec4 img_color = texture(SCREEN_TEXTURE, SCREEN_UV + vec2(smoke_color.g - size_remapped/2.0,0.0));
	
	float screen_darkness_inverted = 1.0 - screen_darkness;
	
	float darkness_scalar = (img_color.x + img_color.y + img_color.z) / 3.0 * screen_darkness_inverted;
	vec4 darkness_color = vec4(darkness_scalar, darkness_scalar, darkness_scalar, 1.0);
	
	COLOR = lerp(img_color, darkness_color, darkness_progression);
}
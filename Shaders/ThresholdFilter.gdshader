shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

uniform float threshold: hint_range(0.0,1.0) = 0.5;
uniform float delta: hint_range(0.0,1.0) = 0.2;

void fragment(){
	vec4 screen_color = texture(SCREEN_TEXTURE, SCREEN_UV); // Get the color from the screen texture
	
	float avg = (screen_color.r + screen_color.g + screen_color.b) / 3.0;
	
	if (avg< threshold){
		COLOR.rgb = vec3(screen_color.r - delta, screen_color.r - delta, screen_color.r - delta);
	}else{
		COLOR.rgb = vec3(screen_color.r + delta, screen_color.r + delta, screen_color.r + delta);
	}
}
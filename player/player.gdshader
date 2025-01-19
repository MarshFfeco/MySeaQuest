shader_type canvas_item;

uniform sampler2D danger;
uniform float oxygen: hint_range(0.0, 1.0, 0.1) = 1.0;

void fragment() {	
	float filter = (COLOR.r + COLOR.g + COLOR.b) / 3.0;
	vec2 md = vec2(filter);
	
	vec3 newTex = texture(danger, md).rgb;
	
	if(oxygen < 0.15) {
		COLOR.rgb = mix(newTex, COLOR.rgb, abs(cos(TIME * 2.0)));
	}
	else
		COLOR.rgb = COLOR.rgb;
}

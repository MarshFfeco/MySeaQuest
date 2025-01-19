shader_type canvas_item;

uniform float strength = 0.0;

void vertex() {
	if(strength > 0.0) {
		VERTEX.x += cos(TIME * strength);
		VERTEX.y += sin(TIME * strength * 2.0);
	}
}

void fragment() {
	if(strength > 3.5) {
		COLOR.rgb = mix(COLOR.rgb, vec3(1), abs(cos(TIME * strength)));
	}
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}

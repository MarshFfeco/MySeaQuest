shader_type canvas_item;

uniform float offset = 0.0;

 void vertex() {
	VERTEX += vec2(VERTEX.x, sin(TIME + offset) * 10.0);
 }

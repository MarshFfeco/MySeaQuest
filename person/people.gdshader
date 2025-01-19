shader_type canvas_item;

uniform bool isFull = false;
uniform float offset;

void vertex() {
	if(isFull) { 
		VERTEX.y += sin(TIME * 2.0 + offset) * 2.0;
	}
}

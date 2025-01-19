shader_type canvas_item;

uniform float speed = 1.0;
uniform float strength = 5.0; 
uniform float height = 1.0;

uniform bool useGradient = false;
uniform sampler2D colors: filter_nearest;

void vertex()
{
	VERTEX.x += strength * cos(TIME * speed + VERTEX.y);
	VERTEX.y += strength * height * sin(TIME * speed + VERTEX.x);
}

void fragment() {
	if(useGradient) 
	{
		vec3 finalColor = texture(colors, vec2(UV.y)).rgb;

		COLOR.rgb = finalColor;
	}
}

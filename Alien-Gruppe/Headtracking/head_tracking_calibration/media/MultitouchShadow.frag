// The rendered texture
uniform sampler2D tex;
uniform sampler2D bg;

// Quadratic "radius" for contour detection
uniform float radius;

// Width and height of the texture
uniform float image_width;
uniform float image_height;
 
/// Returns 0 if both vectors are equal, otherwise 1
///
float compare(in vec4 color1, in vec4 color2) {
	float x_diff = abs(color1.x - color2.x);
	float y_diff = abs(color1.y - color2.y);
	float z_diff = abs(color1.z - color2.z);
	
	float sum = x_diff + y_diff + z_diff;
	
	if (sum > 0.0) {
		return 1.0;
	}
	else {
		return 0.0;
	}
}

/// Apply contour detection
///
vec4 contour_detection(in vec4 center_color, in vec2 center_location) {
	// Horizontal and vertical step width
	float step_x = 1.0 / image_width;
	float step_y = 1.0 / image_height;
		
	// Count amount of differences
	float differenceCounter = 0.0;
	
	// Sample with radius around current location
	for (float i = -radius; i <= radius; i++) {
		for (float j = -radius; j<= radius; j++) {
			vec2 current_location = center_location + vec2(i * step_x, j * step_y);
			vec4 current_color = texture2D(tex, current_location);
			
			// Sum differences up
			differenceCounter += compare(center_color, current_color);
		}
	}
	
	// Normalize the differences (i.e. lies now in [0,1])
	float normalizedDifferences = differenceCounter / (radius * radius);
	
	// Compute color	
	vec4 edge_color  = vec4(0.0, 0.0, 0.0, 0.65);
	vec4 inner_color = vec4(0.3, 0.3, 0.3, 0.5);
	vec4 final_color = mix(inner_color, edge_color, normalizedDifferences);
	
	return final_color;
}
 
void main() {
	// Texture coordinates
	vec2 tcoord = gl_TexCoord[0].xy;
	
	//tcoord.x = 1.0 - tcoord.x;
	//tcoord.y = 1.0f - tcoord.y;
	
	// Get the color fragment
	vec4 color = texture2D(tex, tcoord);
	
	//	Check for object fragment
	//if (color.r != 0.0 || color.g != 0.0 || color.b != 0.0) {
	if (color.a != 0.0) {
		//color = contour_detection(color, tcoord);
	}
	else {
		vec4 bg_color = texture2D(bg, tcoord);
		bg_color.a = 0.15;
		color = bg_color;
	}
	
	// Emit fragment color
	gl_FragColor = vec4(color.r, color.g, color.b, color.a);
}


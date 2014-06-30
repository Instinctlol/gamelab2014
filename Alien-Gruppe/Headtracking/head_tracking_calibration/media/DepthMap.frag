varying vec2 depth;
//varying vec3 normal;

//uniform vec3 viewDirection;

void main()
{
	// float edge = dot(normal, -viewDirection);
	// edge = clamp(edge, 0.0, 1.0);
	
	// vec4 diffuseColor = vec4(depth.x, depth.x, depth.x, 1);

	// vec3 n = (normal + vec3(1)) / 2.0;
	
	// gl_FragColor = vec4(n, 1);
	
	// if(edge < 0.2)
		// gl_FragColor = mix(vec4(0, 0, 0, 1), diffuseColor, edge);
		
	//gl_FragColor = vec4(depth.x, depth.x, depth.x, 1);
	
	gl_FragColor = vec4(depth.x, depth.x, depth.x, 1);
}
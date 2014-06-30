//uniform mat4 worldViewProj;
//uniform vec4 texelOffsets;
  
// X = minDepth
// Y = maxDepth
// Z = depthRange
// W = 1.0 / depthRange                                                         
//uniform vec4 depthRange;

varying vec2 depth;

void main()
{
	depth.x = (gl_ModelViewMatrix * gl_Vertex).z;

	//vec4 position = gl_ModelViewMatrix * gl_Vertex;
	vec4 position = gl_ModelViewMatrix * vec4((gl_Vertex.xyz - 0.15 * normalize(gl_Normal)), 1.0);

	position.z += 2.0;
	
	gl_Position = gl_ProjectionMatrix * position;

	
}
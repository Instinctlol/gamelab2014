
void main()
{
	vec3 position = gl_Vertex.xyz;// + 0.2 * normalize(gl_Normal);
	gl_Position = gl_ProjectionMatrix * gl_ModelViewMatrix * vec4(position, 1.0);
}
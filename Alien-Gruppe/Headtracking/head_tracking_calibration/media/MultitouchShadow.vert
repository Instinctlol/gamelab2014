//varying out float cameraZ;

void main(){	
	gl_FrontColor = gl_Color;	
	gl_TexCoord[0] = gl_MultiTexCoord0;	
	//gl_Position = ftransform();
	
	vec4 position = gl_ProjectionMatrix * gl_ModelViewMatrix * gl_Vertex;
	gl_Position = position;
	
	//cameraZ = (gl_ModelViewMatrix * gl_Vertex).z;
}

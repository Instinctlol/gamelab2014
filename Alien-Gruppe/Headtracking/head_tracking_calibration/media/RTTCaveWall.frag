uniform sampler2D tex;

uniform vec2 tctl;
uniform vec2 tctr;
uniform vec2 tcbl;
uniform vec2 tcbr;
 
void main()
{
	vec2 t0e;
	vec2 t1e;
	vec2 p;
	
	vec2 tcoord = gl_TexCoord[0].xy;
	float tcx = tcoord.x;
	float tcy = tcoord.y;
	
	t0e = (1.0 - tcy) * tctl + tcy * tcbl;
	t1e = (1.0 - tcy) * tctr + tcy * tcbr;
	p   = (1.0 - tcx) * t0e + tcx * t1e;
	
	vec4 color = texture2D(tex, p.xy);
	gl_FragColor = vec4(vec3(texture2D(tex, p.xy)), 1.0);
}


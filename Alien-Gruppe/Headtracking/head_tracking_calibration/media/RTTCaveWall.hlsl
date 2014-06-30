void mainVS( inout   float4   pos    : POSITION, 
			 inout   float2   texco  : TEXCOORD0, 
			 uniform float4x4 worldViewProj_m )
{
	pos = mul( worldViewProj_m, pos );
}
 
void mainPS( in    	 float2    texco    : TEXCOORD0, 
			 out     float4    finColor : COLOR, 
			 uniform sampler2D RttTex,
			 uniform float2 tctl,
			 uniform float2 tctr,
			 uniform float2 tcbl,
			 uniform float2 tcbr)
{
	float tcx = texco.x;
	float tcy = texco.y;
	
	float2 t0e = (1.0 - tcy) * tctl + tcy * tcbl;
	float2 t1e = (1.0 - tcy) * tctr + tcy * tcbr;
	float2 p   = (1.0 - tcx) * t0e + tcx * t1e;
	
	float4 color = tex2D(RttTex, p);
	finColor = color;
}

// Effect dynamically changes color saturation. 
 
float2 d;
sampler TextureSampler : register(s0); 
 
 
float4 BlurShader(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0 
{ 
    float4 tex      = tex2D(TextureSampler, texCoord);
	float4 Color =  tex2D(TextureSampler, texCoord.xy);
    Color += tex2D(TextureSampler, texCoord+d);
	//Color += tex2D(TextureSampler, texCoord+d);
    Color = Color / 2;  
    tex.rgb         = Color;
    return tex; 
} 
 
 
technique Desaturate 
{ 
    pass Pass1 
    { 
        PixelShader = compile ps_2_0 BlurShader(); 
    } 
} 
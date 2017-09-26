float4x4 World;
float4x4 View;
float4x4 Projection;
 
float3 CameraPosition;
 
TextureCube SkyBoxTexture; 
SamplerState SkyBoxSampler
{    
   Filter = MIN_MAG_LINEAR_LINEAR;   
   AddressU = Mirror; 
   AddressV = Mirror; 
};
 
struct VertexShaderInput
{
	float4 Position : SV_POSITION;	
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 TextureCoordinate : TEXCOORD;
};

 
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
 
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
 
    float4 VertexPosition = mul(input.Position, World);
    output.TextureCoordinate = VertexPosition - CameraPosition;
 
    return output;
}
 
float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
    return SkyBoxTexture.Sample(SkyBoxSampler, normalize(input.TextureCoordinate));
}
 
technique Skybox
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
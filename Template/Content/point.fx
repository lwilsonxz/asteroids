float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float3 LightPosition[3];
float4 LightColor[3];
float3 Camera;
float4 DiffuseColor = float4(1,1,1,1);//block color

static const float PI = 3.14159265f;

float4 AmbientColor = float4(0.5f, 0.5f, 0.5f, 1);
float AmbientIntensity = 0.2f;
float LightRadius = 150;

float DiffuseIntensity = 0.7;

float Shininess = 50;
float4 SpecularColor = float4(1,1,1,1);
float SpecularIntensity = 0.5f;

struct VertexShaderInput
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 WorldPosition : POSITIONT;	
	float3 Normal : NORMAL;	
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	output.WorldPosition = worldPosition;
		
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	float3 normal = normalize(mul(input.Normal, (float3x3)WorldInverseTranspose));
	output.Normal = normal;
	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{

	//return DiffuseColor;
	float4 diffuse = float4(0, 0, 0, 0);
	float4 spec = float4(0, 0, 0, 0);
	float3 normal = normalize(input.Normal);
	float3 view = normalize(Camera - (float3)input.WorldPosition);

	

	for (int i = 0; i < 3; ++i)
	{
		float3 lightDirection = LightPosition[i] - (float3)input.WorldPosition;
		float intensity = pow(1.0f - saturate(length(lightDirection) / LightRadius), 2);
		lightDirection = normalize(lightDirection); //normalize after
		float diffuseColor = saturate(dot(normal, lightDirection) * intensity);
		float3 reflect = normalize(2 * diffuseColor * normal - lightDirection);
		float dotProduct = dot(reflect, view);
		float4 specular = (8 + Shininess) / (8 * PI) * SpecularIntensity * SpecularColor * pow(max(dotProduct, 0), Shininess) * length(diffuseColor);
		diffuse += diffuseColor * LightColor[i];
		spec += specular;
	}
	return  saturate(diffuse * DiffuseColor + AmbientColor * AmbientIntensity + spec);
}

technique PointLight
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}
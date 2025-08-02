#ifndef BUDGET_SKINNING_INCLUDED
#define BUDGET_SKINNING_INCLUDED

void SkinningDeform(inout float3 pos, uint4 joints, float4 weights, Texture2D jointMap, uint width, uint offset)
{
    float4x4 mat = float4x4(
        0,0,0,0,
        0,0,0,0,
        0,0,0,0,
        0,0,0,0
    );

    for (int n = 0; n < 4; n++)
    {
        uint i = joints[n] * 3u + offset / 4u;
        uint y = i / width;
        uint x = i % width;

        float4 c[3];
        for(int j = 0; j < 3; j++) 
        {
            c[j] = jointMap.Load(int3(x, y, 0));
            if (x == width - 1u)
            {
                y++;
                x = 0u;
            }
            else
            {
                x++;
            }
        }
        mat += mul(float4x4(
            float4(c[0].xyz, 0.0), 
            float4(c[1].xyz, 0.0), 
            float4(c[2].xyz, 0.0), 
            float4(c[0].w, c[1].w, c[2].w, 1.0)), 
            weights[n]);
    }
    
    pos = mul(float4(pos, 1.0), mat).xyz;
}

void SkinningDeform_float(float3 Pos, float4 Joints, float4 Weights, Texture2D JointMap, float Width, float Offset, out float3 Out)
{
    SkinningDeform(Pos, uint4(Joints), Weights, JointMap, uint(Width), uint(Offset));
    Out = Pos;
}

#endif 
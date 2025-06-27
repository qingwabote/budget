#ifndef BUDGET_SKINNING_INCLUDED
#define BUDGET_SKINNING_INCLUDED

void SkinningDeform_float(float3 Pos, float4 Joints, float4 Weights, UnityTexture2D JointMap, float Width, float Offset, out float3 Out)
{
    uint4 joints = uint4(Joints);
    uint width = uint(Width);
    uint offset = uint(Offset);

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
            c[j] = JointMap.Load(int3(x, y, 0));
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
            Weights[n]);
    }
    
    Out = mul(float4(Pos, 1.0), mat).xyz;
}
#endif 
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
        uint i = joints[n] * 4u + offset / 4u;
        uint y = i / width;
        uint x = i % width;

        float4 rows[4];
        for(int j = 0; j < 4; j++) 
        {
            rows[j] = JointMap.Load(int3(x, y, 0));
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
        mat += mul(float4x4(rows[0], rows[1], rows[2], rows[3]), Weights[n]);
    }
    
    // Out = mul(mat, float4(Pos, 1.0)).xyz; Why not this?
    Out = mul(float4(Pos, 1.0), mat).xyz;
}
#endif 
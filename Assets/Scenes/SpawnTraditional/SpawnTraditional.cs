using System;
using UnityEngine;

public class SpawnTraditional : MonoBehaviour
{
    public GameObject Prefab;

    public uint Num;

    private Transform[] Transforms;

    private Vector3[] Destinations;

    private Unity.Mathematics.Random _Random;

    void Start()
    {
        Transforms = new Transform[Num];
        Destinations = new Vector3[Num];
        for (int i = 0; i < Num; i++)
        {

            Transforms[i] = Instantiate(Prefab).transform;
            Destinations[i] = new Vector3();
        }
        _Random = new Unity.Mathematics.Random(1);
    }

    void Update()
    {
        for (int i = 0; i < Num; i++)
        {
            var transform = Transforms[i];
            var destination = Destinations[i];

            var d = destination - transform.position;
            if (d.magnitude < 1)
            {
                Destinations[i] = new Vector3(_Random.NextInt(-3, 3), 0, _Random.NextInt(-6, 6));
                continue;
            }

            var rot = Quaternion.LookRotation(d);

            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 0.05f);

            var move = transform.rotation * new Vector3(0, 0, 0.05f);
            transform.position += move;

            Transforms[i] = transform;
        }
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumberPopupTMP : MonoBehaviour
{
    public GameObject Prefab;

    public int Num;

    private List<TMP_Text> Labels;

    private Unity.Mathematics.Random _Random = new(1);

    void Start()
    {
        Labels = new();

        for (int i = 0; i < Num; i++)
        {
            var instance = Instantiate(Prefab, transform);
            instance.transform.localPosition = new Vector3(_Random.NextFloat(-170, 170), _Random.NextFloat(-426, 380), 0);
            Labels.Add(instance.GetComponent<TMP_Text>());
        }
    }

    void Update()
    {
        foreach (var label in Labels)
        {
            label.text = _Random.NextInt(0, 99).ToString();
        }
    }
}

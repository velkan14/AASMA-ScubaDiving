using UnityEngine;
using System.Collections;

public class SupermarketDemand : MonoBehaviour {

    [HideInInspector] public int pears;
    [HideInInspector] public int strawberries;
    [HideInInspector] public int apples;
    [HideInInspector] public int bananas;

    void Start () {
        pears = Random.Range(0, 20);
        strawberries = Random.Range(0, 20);
        apples = Random.Range(0, 20);
        bananas = Random.Range(0, 20);
    }
}

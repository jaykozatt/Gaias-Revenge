using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pieces", menuName = "ScriptableObjects/Pieces", order = 0), ExecuteInEditMode] 
public class Pieces : ScriptableObject 
{
    public GameObject[] list;

    public GameObject GetRandom()
    {
        int value = Random.Range(0, list.Length);
        return list[value];
    }

    private void OnValidate() {
        list = Resources.LoadAll<GameObject>("Placeables");
    }

}
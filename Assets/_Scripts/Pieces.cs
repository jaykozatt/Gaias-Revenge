using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pieces", menuName = "ScriptableObjects/Pieces", order = 0), ExecuteInEditMode] 
public class Pieces : ScriptableObject 
{
    private GameObject _template;
    public List<GameObject> list;

    public GameObject GetRandom()
    {
        int value = Random.Range(0, list.Count);
        return list[value];
    }

    private void OnValidate() {
        _template = Resources.Load<GameObject>("Placeables/Template");
        list = new List<GameObject>(Resources.LoadAll<GameObject>("Placeables"));
        list.Remove(_template);
    }

}
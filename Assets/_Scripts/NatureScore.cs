using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NatureScore : MonoBehaviour
{
    TextMeshProUGUI textMesh;

    // Start is called before the first frame update
    void Start()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        GameBoard.Instance.OnNatureScoreChanged += UpdateScore;
    }

    // Update is called once per frame
    void UpdateScore(int value)
    {
        textMesh.text = "Nature: "+value.ToString();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class EndMessage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMesh;

    // Start is called before the first frame update
    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        GameManager.Instance.OnGameEnded += UpdateMessage;
    }

    // Update is called once per frame
    void UpdateMessage()
    {
        if (GameBoard.Instance.NatureScore < GameBoard.Instance.HumanScore)
            textMesh.text = "Nature Lost!";
        else 
            textMesh.text = "Humans Contained!";
    }
}

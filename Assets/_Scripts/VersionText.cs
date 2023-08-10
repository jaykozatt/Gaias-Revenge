using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace AllBets
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class VersionText : MonoBehaviour
    {
        TextMeshProUGUI _text;
        
        // Start is called before the first frame update
        void Start()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _text.text = "Version " + Application.version;
        }
    }
}

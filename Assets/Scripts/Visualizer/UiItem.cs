using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiItem : MonoBehaviour
{
    public TextMeshProUGUI KeyText;
    public TextMeshProUGUI ValueText;
    public string key = null;

    public void KeyValueTextSet(string Key , string Value)
    {
        key = Key; 
        KeyText.text = $"K: {Key}";
        ValueText.text = $"V: {Value}";
    }
}

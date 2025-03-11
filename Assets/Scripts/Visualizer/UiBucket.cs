using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiBucket : MonoBehaviour
{
    public TextMeshProUGUI bucketText;
    public UiItem uiItem;
    public string index;
        
    public void BucketTextSet(string text)
    {
        index = text;
        bucketText.text = $"I: {text}";
    }
}

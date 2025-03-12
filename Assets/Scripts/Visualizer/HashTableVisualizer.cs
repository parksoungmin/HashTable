using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HashTableVisualizer : MonoBehaviour
{
    public TMP_InputField keyInputField;
    public TMP_InputField valueInputField;

    public Button addButton;
    public Button removeButton;
    public Button clearButton;

    public IDictionary<string, string> hashTable = null;

    public TMP_Dropdown hashTableDropdown;
    public TMP_Dropdown colisionDropdown;

    private int count = 0;

    public GameObject bucket;
    public GameObject item;
    public GameObject contant;
    public List<GameObject> buckets = new List<GameObject>();
    private void Start()
    {
        hashTable = ChackHashTable(hashTable);
        {
            for (int i = 0; i < 10; i++)
            {
                var ranKey = Random.Range(0, 100).ToString();
                var ranValue = Random.Range(0, 100).ToString();
                hashTable.Add(ranKey, ranValue);
                var getIndexHash = (IHashTables<string, string>)hashTable;
                var newIndex = getIndexHash.GetArrayIndex(i.ToString()).ToString();
                if (ChackIndex(newIndex))
                {
                    foreach (var bucket in buckets)
                    {
                        var bucketInIndex = bucket.GetComponent<UiBucket>().index;
                        if (newIndex == bucketInIndex)
                        {
                            var hash = Instantiate(item, bucket.transform);
                            var setHash = hash.GetComponent<UiItem>();
                            setHash.KeyValueTextSet(ranKey, ranValue);
                            buckets.Add(hash);
                        }
                    }
                }
                else
                {
                    var hash = Instantiate(bucket, contant.transform);
                    var setHash = hash.GetComponent<UiBucket>();
                    setHash.BucketTextSet(getIndexHash.GetArrayIndex(i.ToString()).ToString());
                    setHash.uiItem.KeyValueTextSet(ranKey, ranValue);
                    buckets.Add(hash);
                }
            }
        }
    }
    public IDictionary<string, string> ChackHashTable(IDictionary<string, string> hashTable)
    {
        switch (hashTableDropdown.value)
        {
            case (int)HashTable.ChainingHashTable:
                this.hashTable = new ChainingHashTable<string, string>();
                break;
            case (int)HashTable.OpenAddressingHashTable:
                this.hashTable = new OpenAddressingHashTable<string, string>();
                break;
        }
        return this.hashTable;
    }
    public void AddButtonClick()
    {
        hashTable.Add(keyInputField.text.ToString(), valueInputField.text.ToString());
        var hash = Instantiate(bucket, contant.transform);

        var setHash = hash.GetComponent<UiBucket>();

        var getIndexHash = (IHashTables<string, string>)hashTable;
        var newIndex = getIndexHash.GetArrayIndex(keyInputField.text.ToString()).ToString();
        if (ChackIndex(newIndex))
        {
            foreach (var bucket in buckets)
            {
                var bucketInIndex = bucket.GetComponent<UiBucket>().index;
                if (newIndex == bucketInIndex)
                {
                    var newHash = Instantiate(item, bucket.transform);
                    var newSetHash = newHash.GetComponent<UiItem>();
                    newSetHash.KeyValueTextSet(keyInputField.text.ToString(), valueInputField.text.ToString());
                    buckets.Add(newHash);
                }
            }
        }
        else
        {
            var newHash = Instantiate(bucket, contant.transform);
            var newSetHash = newHash.GetComponent<UiBucket>();
            newSetHash.BucketTextSet(getIndexHash.GetArrayIndex(keyInputField.text.ToString()).ToString());
            newSetHash.uiItem.KeyValueTextSet(keyInputField.text.ToString(), valueInputField.text.ToString());
            buckets.Add(hash);
        }
        // 여기부터 해야함
    }
    public void RemoveButtonClick()
    {
        Debug.Log("remove호출");
        hashTable.Remove(keyInputField.text.ToString());
        foreach (var hash in buckets)
        {
            var hashInformation = hash.GetComponent<UiBucket>();
            if (hashInformation.uiItem.key == keyInputField.text.ToString())
            {
                buckets.Remove(hash);
                Destroy(hash);
                return;
            }
        }
    }
    public void ClearButtonClick()
    {
        hashTable.Clear();
        foreach (var hash in buckets)
        {
            Destroy(hash);
        }
        buckets.Clear();
    }
    public bool ChackIndex(string index)
    {
        var getIndexHash = (IHashTables<string, string>)hashTable;
        foreach (var hash in hashTable)
        {
            if (index == getIndexHash.GetArrayIndex(hash.Key).ToString())
            {
                return true;
            }
        }
        return false;
    }
}
public enum HashTable
{
    ChainingHashTable,
    OpenAddressingHashTable,
}

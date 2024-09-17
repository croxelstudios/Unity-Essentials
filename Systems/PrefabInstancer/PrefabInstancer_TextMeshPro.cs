using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PrefabInstancer))]
public class PrefabInstancer_TextMeshPro : MonoBehaviour
{
    PrefabInstancer pi;
    [SerializeField]
    string preText = "";
    [SerializeField]
    string postText = "";


    private void Awake()
    {
        pi = GetComponent<PrefabInstancer>();
    }

    public void SetPreText(string text)
    {
        preText = text;
    }
    public void SetPostText(string text)
    {
        postText = text;
    }

    public void ChangeText(float newText)
    {
        if (pi.entities != null)
            foreach (SpawnedEntity se in pi.entities)
            {
                TextMeshPro tmp = se.GetComponentInChildren<TextMeshPro>();
                if (tmp)
                    tmp.text = preText + newText.ToString() + postText;
            }
    }

    public void ChangeText(string newText)
    {
        if (pi.entities != null)
            foreach (SpawnedEntity se in pi.entities)
            {
                TextMeshPro tmp = se.GetComponent<TextMeshPro>();
                if (tmp)
                    tmp.text = preText + newText + postText;
            }
    }
}

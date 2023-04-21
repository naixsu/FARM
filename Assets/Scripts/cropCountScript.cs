using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class cropCountScript : MonoBehaviour
{
    public static int cropValue;
    [SerializeField] private TMP_Text crop;
    // Start is called before the first frame update
    void Start()
    {
        cropValue = 0;
    }

    // Update is called once per frame
    void Update()
    {
        crop.text = "Crop Count: " + cropValue;
    }
}

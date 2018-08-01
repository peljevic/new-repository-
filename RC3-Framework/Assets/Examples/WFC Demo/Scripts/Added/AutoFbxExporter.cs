using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFBXExporter;
using RC3.Unity.WFCDemo;
using FbxExporters;

public class AutoFbxExporter : MonoBehaviour
{
    [SerializeField] private GameObject _tileModel;
    [SerializeField] private string _folder = "Exports/00";
    private FBXExporter _exporter;

    private void Awake()
    {
       
 
    }

    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            FBXExporter.ExportGameObjToFBX(_tileModel, _folder, false, false);
            Debug.Log("EXPORT TRIGGERED!!!!!!!!!!");
        }
   
    }
}

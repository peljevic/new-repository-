using System;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RC3.Graphs;
using RC3.WFC;
using RC3.Unity.WFCDemo;

using SpatialSlur.Core;

public class GraphVisualizer : MonoBehaviour
{
    [SerializeField] private SharedAnalysisEdgeGraph _sharedAnalysisGraph;
    [SerializeField] private GraphAnalysisManager _graphAnalysisManager;
    [SerializeField] private Material _material;
    [SerializeField] private Material _structureMaterial;
    [SerializeField] private Color[] _spectrum;
    [SerializeField] private float _minEdgeSize = .5f;
    [SerializeField] private float _maxEdgeSize = 3f;

    [SerializeField] private SharedDigraph _originalGraph;

    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private Mesh _mesh;
    private GameObject _meshObj;
    private bool _isvisible = true;

    // private float[] _currentForces;
    private Vector2[] _uv1;
    private Vector2[] _uv2;

    /*
    private float _maxForce;
    private float _minForce;
    private float _maxTorque;
    private float _minTorque;
    */

    public RenderMode _vizmode = RenderMode.StressAnalysis;

    public enum RenderMode
    {
        DepthFromSource,
        Components,
        ComponentsSize,
        StressAnalysis
    }

    void Awake()
    {
        int nv = _originalGraph.VertexObjects.Count;
        _uv1 = new Vector2[nv];
        _uv2 = new Vector2[nv];

        //_currentForces = new float[_originalGraph.VertexObjects.Count];

        _meshObj = new GameObject("Graph Mesh Object");
        _meshObj.transform.parent = gameObject.transform;
        _meshFilter = _meshObj.AddComponent(typeof(MeshFilter)) as MeshFilter;
        _meshRenderer = _meshObj.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

        _mesh = new Mesh();
        _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _mesh.name = "GraphMesh";
        _meshFilter.mesh = _mesh;

    }

    public void CreateMesh()
    {
        _mesh = new Mesh();
        _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _mesh.name = "GraphMesh";
        _meshFilter.mesh = _mesh;

        _mesh.vertices = _sharedAnalysisGraph.Vertices;
        _mesh.SetIndices(_sharedAnalysisGraph.LineIndices.ToArray<int>(), MeshTopology.Lines, 0);
        SetVizColors();
    }


    public void SetVizColors()
    {
        if(_vizmode == RenderMode.StressAnalysis)
        {
            StressForceRenderMode();
        }

        if (_vizmode == RenderMode.DepthFromSource)
        {
            ChangeRenderMode(_sharedAnalysisGraph.NormalizedDepths);
        }

        if (_vizmode == RenderMode.Components)
        {
            ChangeRenderMode(_sharedAnalysisGraph.NormalizedComponents);           
        }

        if (_vizmode == RenderMode.ComponentsSize)
        {
            ChangeRenderMode(_sharedAnalysisGraph.NormalizedComponentsBySize);
        }

    }



    private void StressForceRenderMode()
    {
        _meshRenderer.sharedMaterial = _structureMaterial;
        
        var vertObjs = _originalGraph.VertexObjects;
        
        _uv1.Set(vertObjs.Select(v => new Vector2(v.JointAvgForce(), 0.0f)));
        var invMax = 1.0f / _uv1.Max(v => v.x);
        
        for (int i = 0; i < _uv1.Length; i++)
            _uv1[i].x *= invMax;

        //Vector2[] uvs = new Vector2[_mesh.vertices.Length];

        /*
        for (int i = 0; i < _mesh.vertices.Length; i++)
        {
            Vector2 uv = new Vector2(analysisValues[i], 0);
            uvs[i] = uv;
        }
        */

        _mesh.uv = _uv1;


        /*
        Vector2[] uv2s = new Vector2[_mesh.vertices.Length];
        float[] edgethicknessarray = RemapValues(analysisValues, _minEdgeSize, _maxEdgeSize);
        for (int i = 0; i < _mesh.vertices.Length; i++)
        {
            Vector2 uv = new Vector2(edgethicknessarray[i], 0);
            uv2s[i] = uv;
        }
        _mesh.uv2 = uv2s;
        */

        _mesh.vertices = _sharedAnalysisGraph.Vertices;
        _mesh.SetIndices(_sharedAnalysisGraph.LineIndices.ToArray<int>(), MeshTopology.Lines, 0);
    }

    

    private void StressRenderMode2()
    {
        _meshRenderer.sharedMaterial = _structureMaterial;
        const float factor = 0.75f;


      //  StartCoroutine(CurrentForceUpdate());

        Color[] colors = new Color[_mesh.vertices.Length];

        for (int i = 0; i < _mesh.vertices.Length; i++)
        {
            //Color colorMesh = CurrentForceColor(i);
            //Debug.Log("<color=red>current color :</color> vertex " + i);
            //  Debug.Log($"color ");
            // Color.Lerp(_mesh.colors[i], CurrentForceColor(i), factor);
            //colors[i] = colorMesh;
            //Debug.Log(colorMesh);
        }

        //Vector2[] uv2s = new Vector2[_mesh.vertices.Length];
        //float[] edgeThicknessArray = RemapValues(_currentForces, _minEdgeSize, _maxEdgeSize);
        //for (int i = 0; i < _mesh.vertices.Length; i++)
        //{
        //    Vector2 uv = new Vector2(edgeThicknessArray[i], 0);
        //    uv2s[i] = uv;
        //}
        _mesh.colors = colors;
      //  _mesh.uv2 = uv2s;

        _mesh.vertices = _sharedAnalysisGraph.Vertices;
        _mesh.SetIndices(_sharedAnalysisGraph.LineIndices.ToArray<int>(), MeshTopology.Lines, 0);
    }

    /*
    private Color CurrentForceColor(int index)
    {
        var v = _originalGraph.VertexObjects;
        var vertex = v[index];
        var average = vertex.JointAvgForce();

        if (average == 0)
            return _spectrum[0];

       else return Lerp(_spectrum, average / _maxForce);
    }

    public static Color Lerp(IReadOnlyList<Color> colors, float factor)
    {
        int last = colors.Count - 1;
        int i;
        factor = SlurMath.Fract(factor * last, out i);

        if (i < 0)
            return colors[0];
        else if (i >= last)
            return colors[last];

        return Color.LerpUnclamped(colors[i], colors[i + 1], factor);
    }
   

    private IEnumerator CurrentForceUpdate()
    {      
        var v = _originalGraph.VertexObjects;

        while (true)
        {
            for (int i = 0; i < _originalGraph.VertexObjects.Count; i++)
            {
                _currentForces[i] = v[i].JointAvgForce();
                Debug.Log($"Force on {i} is {_currentForces[i]}");
            }

            _maxForce = _currentForces.Max();
            _minForce = _currentForces.Min();

            yield return new WaitForSeconds(0.5f);
        }
    }
    */

    private void ChangeRenderMode(float[] analysisValues)
    {
        _meshRenderer.sharedMaterial = _material;

        Vector2[] uvs = new Vector2[_mesh.vertices.Length];
        for (int i = 0; i < _mesh.vertices.Length; i++)
        {
            Vector2 uv = new Vector2(analysisValues[i], 0);
            uvs[i] = uv;
        }

        Vector2[] uv2s = new Vector2[_mesh.vertices.Length];
        float[] edgethicknessarray = RemapValues(analysisValues, _minEdgeSize, _maxEdgeSize);
        for (int i = 0; i < _mesh.vertices.Length; i++)
        {
            Vector2 uv = new Vector2(edgethicknessarray[i], 0);
            uv2s[i] = uv;
        }
        _mesh.uv = uvs;
        _mesh.uv2 = uv2s;

        _mesh.vertices = _sharedAnalysisGraph.Vertices;
        _mesh.SetIndices(_sharedAnalysisGraph.LineIndices.ToArray<int>(), MeshTopology.Lines, 0);
    }


    /// <summary>
    /// Remaps an array of values to a new range
    /// </summary>
    /// <param name="inputValues"></param>
    /// <param name="minOutValue"></param>
    /// <param name="maxOutValue"></param>
    /// <returns></returns>
    private float[] RemapValues(int[] inputValues, float minOutValue, float maxOutValue)
    { 
        float[] outputvalues = new float[inputValues.Length];
        int maxValue = inputValues.Max();
        int minValue = inputValues.Min();
            
        for (int i = 0; i < inputValues.Length; i++)
        {
            outputvalues[i] = Remap((float)inputValues[i], (float)minValue, (float)maxValue, minOutValue, maxOutValue);
        }

        return outputvalues;
    }

    /// <summary>
    /// Remaps an array of values to a new range
    /// </summary>
    /// <param name="inputvalues"></param>
    /// <param name="minoutvalue"></param>
    /// <param name="maxoutvalue"></param>
    /// <returns></returns>
    private float[] RemapValues(float[] inputvalues, float minoutvalue, float maxoutvalue)
    {
        float[] outputvalues = new float[inputvalues.Length];
        float maxvalue = inputvalues.Max(); //float.MinValue;
        float minvalue = inputvalues.Min();   //float.MaxValue;
             
        for (int i = 0; i < inputvalues.Length; i++)
        {
            outputvalues[i] = Remap(inputvalues[i], minvalue, maxvalue, minoutvalue, maxoutvalue);
        }

        return outputvalues;
    }

    /// <summary>
    /// Remap a value from one range to another range
    /// </summary>
    /// <param name="value"></param>
    /// <param name="inputfrom"></param>
    /// <param name="inputto"></param>
    /// <param name="outputfrom"></param>
    /// <param name="outputto"></param>
    /// <returns></returns>
    private float Remap(float value, float inputfrom, float inputto, float outputfrom, float outputto)
    {
        return (value - inputfrom) / (inputto - inputfrom) * (outputto - outputfrom) + outputfrom;
    }

    public GameObject MeshObject
    {
        get { return _meshObj; }
    }

    public RenderMode VizMode
    {
        get { return _vizmode; }
        set { _vizmode = value; }
    } 


    public bool IsVisible
    {
        get { return _isvisible; }
        set { _isvisible = true; }
    }
}

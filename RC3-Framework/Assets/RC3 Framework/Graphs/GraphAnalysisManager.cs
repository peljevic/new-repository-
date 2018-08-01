using System;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RC3.Graphs;
using RC3.WFC;
using RC3.Unity.WFCDemo;


namespace RC3.Graphs
{
    [CreateAssetMenu(menuName = "RC3/WFC Demo/GraphAnalysisManager")]
    public class GraphAnalysisManager : MonoBehaviour
    {
        #region Member Variables

        private ProcessingUtil _graphProcessing = new ProcessingUtil();

        [SerializeField] private TileGraphExtractor _graphExtractor;
        [SerializeField] private TileModelManager _tileModelManager;
        [SerializeField] private SharedAnalysisEdgeGraph _analysisGraph;
        [SerializeField] private SharedDigraph _originalGraph;

        private GraphVisualizer _graphVisualizer;
        private TileModel _tileModel;
        private int _graphviz = 0;


        #endregion

        #region Variables

        private float[] _averageForces;
        private float[] _averageTorques;
        private float _maxForce;
        private float _minForce;
        private float _maxTorque;
        private float _minTorque;

        private int _closureCount = 0;
        private int _componentCount = 0;
        private float _closureRate = 0;

        private List<HashSet<int>> _connectedComponents = new List<HashSet<int>>();
        private float[] _normalizedcomponents;
        private float[] _normalizedcomponentsbysize;

        private List<int> _sources;
        private int[] _depths;
        private int _maxDepth = 0;

        private float[] _normalizeddepths;
        private List<int> _unreachablevertices = new List<int>();
        private List<int> _edgelessvertices = new List<int>();

        #endregion


        private void Awake()
        {
            _averageForces = new float[_originalGraph.VertexObjects.Count];
            _averageTorques = new float[_originalGraph.VertexObjects.Count];

            if (_tileModelManager != null)
            {
                _tileModel = _tileModelManager.TileModel;
            }

            _analysisGraph.Initialize();

            _graphVisualizer = GetComponent<GraphVisualizer>();
        }

        #region GUI methods Changing Visualisation
        // GUI METHODS 
        public void UpdateGraphAnalysis()
        {
            UpdateAnalysis();
            _graphVisualizer.SetVizColors();
        }

        public void ShowComponents()
        {
            _graphVisualizer.VizMode = GraphVisualizer.RenderMode.Components;
            _graphVisualizer.SetVizColors();
        }

        public void ShowDepth()
        {
            _graphVisualizer.VizMode = GraphVisualizer.RenderMode.DepthFromSource;
            _graphVisualizer.SetVizColors();
        }

        public void ShowComponentsSize()
        {

            _graphVisualizer.VizMode = GraphVisualizer.RenderMode.ComponentsSize;
            _graphVisualizer.SetVizColors();
        }

        public void ShowStress()
        {
            _graphVisualizer.VizMode = GraphVisualizer.RenderMode.StressAnalysis;
            _graphVisualizer.SetVizColors();
        }

        #endregion GUI methods

        private void RotateView()
        {
            if (gameObject.GetComponent<ModelDisplay>() == null)
            {
                gameObject.AddComponent<ModelDisplay>();
            }
            else
            {
                Destroy(gameObject.GetComponent<ModelDisplay>());
            }
        }

        /// <summary>
        /// This is empty now
        /// </summary>
        void Start()
        {

#if (false) // Tyson Tests for the Graph
            EdgeGraph testgraph = new EdgeGraph(15);
            ProcessingUtil graphprocessing = new ProcessingUtil();

            for (int i = 0; i < 20; i++)
            {
                testgraph.AddVertex();
            }

            //TestGraph | Build graph
            //first connected component set of edges
            testgraph.AddEdge(1, 2);
            testgraph.AddEdge(2, 3);
            testgraph.AddEdge(3, 1);
            testgraph.AddEdge(3, 5);
            testgraph.AddEdge(5, 6);
            testgraph.AddEdge(6, 4);
            testgraph.AddEdge(4, 5);
            testgraph.AddEdge(7, 6);
            testgraph.AddEdge(3, 8);
            testgraph.AddEdge(8, 1);
            testgraph.AddEdge(6, 9);
            testgraph.AddEdge(9, 4);
            testgraph.AddEdge(8, 10);
            testgraph.AddEdge(10, 11);
            testgraph.AddEdge(11, 12);
            testgraph.AddEdge(12, 10);

            //second component (not connected to first) 
            testgraph.AddEdge(15, 18);
            testgraph.AddEdge(18, 19);
            testgraph.AddEdge(17, 18);

            //TestGraph | Analysis
            int componentcount = 0;
            int closurecount = 0;
            List<HashSet<int>> components = new List<HashSet<int>>();
            _graphprocessing.CountClosures(testgraph, out componentcount, out closurecount, out components);
            float closurerate = (float)closurecount / (float)testgraph.EdgeCount;

            //TestGraph | Debug Print Results
            Debug.Log("TestGraph | Components Count = " + componentcount);
            for (int i = 0; i < components.Count; i++)
            {
                HashSet<int> set = components[i];
                string setstring = string.Join(",", components[i]);
                Debug.Log("TestGraph | ConnectedComponent# " + (i + 1) + " = " + setstring);
            }

            float[] normalizedcomponents = _graphprocessing.RemapComponentsToArray(testgraph, components);
            string normalizedcomponentsstring = string.Join(",", normalizedcomponents);
            Debug.Log("TestGraph | NormalizedComponents = " + normalizedcomponentsstring);
            Debug.Log($"TestGraph | NormalizedComponentsCount =  {normalizedcomponents.ToArray().Length}");
            Debug.Log("TestGraph | Closures Count = " + closurecount);
            Debug.Log("TestGraph | Closures Rate = " + closurerate);
#endif
        }

        void Update()
        {
            KeyPressMethod();

            if (Input.GetKeyDown(KeyCode.Space))
                RotateView();
        }

        private void GetClosures()
        {
            //analyze/get # of closures / strongly connected components
            _closureCount = 0;
            _componentCount = 0;

            _graphProcessing.CountClosures(_analysisGraph.Graph, out _componentCount, out _closureCount, out _connectedComponents);
            _closureRate = (float)_closureCount / (float)_analysisGraph.Graph.EdgeCount;
     
        }

        private void CollectStructureInformation()
        {
            for (int i = 0; i < _originalGraph.VertexObjects.Count; i++)
            {
                var v = _originalGraph.VertexObjects;
              //  _analysisGraph.ForceStress[i] =
                    _averageForces[i] = v[i].JointAvgForce();
              //  _analysisGraph.TorqueStress[i] =
                    _averageTorques[i] = v[i].JointAvgTorque();
            }

            StructureExtremeValues();
        }

        private void CollectStructureInformation0()
        {
            int i = -1;

            foreach (var v in _originalGraph.VertexObjects)
            {
                _analysisGraph.ForceStress[i++] = _averageForces[i++] = v.JointAvgForce();
                _analysisGraph.TorqueStress[i++] = _averageTorques[i++] = v.JointAvgTorque();
            }

            StructureExtremeValues();
        }

        private void StructureExtremeValues()
        {
            _analysisGraph.MaxForce = _maxForce = _averageForces.Max();
            _analysisGraph.MaxTorque = _maxTorque = _averageTorques.Max();

            _minForce = _averageForces.Min();
            _minTorque = _averageTorques.Min();
        }

        private void StoreAnalysis()
        {
            //store analysis in the shared analysis graph scriptable object - VIEW THIS DATA ON A UI CANVAS

            _analysisGraph.ClosuresCount = _closureCount;
            _analysisGraph.ConnectedComponents = _connectedComponents;
            _analysisGraph.ConnectedComponentsCount = _componentCount;

            _analysisGraph.NormalizedComponents = _normalizedcomponents;
            _analysisGraph.NormalizedComponentsBySize = _normalizedcomponentsbysize;

            _analysisGraph.Sources = _sources;
            _analysisGraph.Depths = _depths;
            _analysisGraph.NormalizedDepths = _normalizeddepths;
            _analysisGraph.MaxDepth = _maxDepth;
            _analysisGraph.UnreachableVertices = _unreachablevertices;
            _analysisGraph.EdgelessVertices = _edgelessvertices;

            _analysisGraph.ForceStress = _averageForces;
            _analysisGraph.TorqueStress = _averageTorques;

        }

        private void DebugResults()
        {
            //Debug.Log($"GRAPH ANALYSIS MANAGER | CONNECTED COMPONENTS COUNT {connectedComponents.Count}");

            //Extracted Graph | Debug Print Results
            Debug.Log("Exracted Graph | ComponentsCount = " + _analysisGraph.ConnectedComponentsCount);
            for (int i = 0; i < _connectedComponents.Count; i++)
            {
                HashSet<int> set = _connectedComponents[i];
                string setstring = string.Join(",", _connectedComponents[i]);
                Debug.Log("Exracted Graph | ConnectedComponent# " + (i + 1) + " = " + setstring);
            }

            //Extracted Graph | Debug Print Results

            string forcesString = string.Join(",", _analysisGraph.ForceStress);
            string torquesString = string.Join(",", _analysisGraph.TorqueStress);

            Debug.Log("Exracted Graph | Force applied = " + forcesString);
            Debug.Log("Exracted Graph | Torque applied = " + torquesString);

            string normalizedcomponentsstring = string.Join(",", _analysisGraph.NormalizedComponents);
            string normalizedcomponentsbysizestring = string.Join(",", _analysisGraph.NormalizedComponentsBySize);

            Debug.Log("Exracted Graph | NormalizedComponents = " + normalizedcomponentsstring);
            Debug.Log("Exracted Graph | NormalizedComponentsBySize = " + normalizedcomponentsbysizestring);

            Debug.Log("Exracted Graph | Closures Count = " + _analysisGraph.ClosuresCount);
            Debug.Log("Exracted Graph | Closures Rate = " + _analysisGraph.ClosureRate);

            string sourcesString = string.Join(",", _analysisGraph.Sources);
            string depthsString = string.Join(",", _analysisGraph.Depths);

            Debug.Log("Exracted Graph | Depths = " + depthsString);
            Debug.Log("Exracted Graph | Max Depth = " + _analysisGraph.MaxDepth);
            Debug.Log("Exracted Graph | SourcesCount = " + _analysisGraph.SourcesCount);
            Debug.Log("Exracted Graph | Sources = " + sourcesString);

            string normalizedDepthsString = string.Join(",", _analysisGraph.NormalizedDepths);
            string unreachableVrtsString = string.Join(",", _analysisGraph.UnreachableVertices);
            string edgelessVrtsString = string.Join(",", _analysisGraph.EdgelessVertices);
        }


        private void UpdateAnalysis()
        {
            if (_graphExtractor != null && _tileModelManager != null)
            {
                if (_tileModelManager.Status == CollapseStatus.Complete)
                {
                    _graphExtractor.ExtractSharedEdgeGraph(_analysisGraph);

                    //Extracted Graph | Analysis

                    GetClosures();

                    CollectStructureInformation();

                    //normalized/remapped components to an array for graph coloring
                    _normalizedcomponents = _graphProcessing.RemapComponentsToArray(_analysisGraph.Graph, _connectedComponents);
                    _normalizedcomponentsbysize = _graphProcessing.RemapComponentsSizeToArray(_analysisGraph.Graph, _connectedComponents);
                                    

                    //analyze/get 1) ground support sources, 2) list of vertex depths 3) max depth 
                    _sources = _graphProcessing.GetGroundSources(_analysisGraph.Graph, _analysisGraph.Vertices, 2f);
                    _depths = _graphProcessing.DepthsFromGroundSources(_analysisGraph.Graph, _analysisGraph.Vertices, 2f);
                    _maxDepth = _graphProcessing.MaxDepth(_depths);

                    //analyze/get 1) unreachable vertices, 2) remapped vertex depths between 0,1, 3) edgeless vertices
                    _normalizeddepths = new float[_analysisGraph.Graph.VertexCount];
                    _unreachablevertices = new List<int>();
                    _edgelessvertices = new List<int>();
                    _graphProcessing.RemapGraphDepths(_analysisGraph.Graph, _depths, 0, 1, out _normalizeddepths, out _unreachablevertices, out _edgelessvertices);

                    StoreAnalysis();
                    DebugResults();

                }
            }
            else
            {
                Debug.Log("No Graph Extractor OR WFC Incomplete");
            }
        }

    
        private void UpdateGraphMesh()
        {
            if (_graphVisualizer != null)
            {
                _graphVisualizer.CreateMesh();
            }

            else
            {
                Debug.Log("No Graph Visualizer Component Attached!");
            }
        }


        private void KeyPressMethod()
        {
            if (Input.GetKeyDown(KeyCode.U))
            { 
                UpdateAnalysis();
                UpdateGraphMesh();
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                if (_graphviz < 3)
                {
                    _graphviz++;
                    if (_graphviz == 1)
                    {
                        _graphVisualizer.VizMode = GraphVisualizer.RenderMode.Components;
                    }

                    if (_graphviz == 2)
                    {
                        _graphVisualizer.VizMode = GraphVisualizer.RenderMode.ComponentsSize;
                    }

                    if (_graphviz == 3)
                    {
                        _graphVisualizer.VizMode = GraphVisualizer.RenderMode.StressAnalysis;
                    }

                }
                else
                {
                    _graphviz = 0;
                    _graphVisualizer.VizMode = GraphVisualizer.RenderMode.DepthFromSource;
                }

                _graphVisualizer.SetVizColors();
            }
        }

#region Public Properties ---> Get IEdgeGraph and SharedAnalysisGraph

        public IEdgeGraph Graph
        {
            get { return _analysisGraph.Graph; }
        }

        public SharedAnalysisEdgeGraph AnalysisGraph
        {
            get { return _analysisGraph; }
        }

#endregion

    }
}

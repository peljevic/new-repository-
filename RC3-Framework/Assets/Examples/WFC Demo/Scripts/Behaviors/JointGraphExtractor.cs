using System.Collections.Generic;
using UnityEngine;
using RC3.Graphs;
using RC3.WFC;

namespace RC3.Unity.WFCDemo
{
    public class JointGraphExtractor : MonoBehaviour
    {
        [SerializeField] private SharedDigraph _tileGraph;
        [SerializeField] private GameObject _vertexPrefab;

        private RemapJoints _remap;
        private TileModel _model;
        private TileMap<string> _map;
        private HashSet<string> _labelSet;

        private void Initialize()
        {
            var manager = GetComponent<TileModelManager>();
            _model = manager.TileModel;
            _map = manager.TileMap;

            _remap = GetComponent<RemapJoints>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
                InstantiateVertexObjects();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Digraph Extract()
        {
            if (_model == null)
                Initialize();

            var g0 = _tileGraph.Graph;
            var g1 = new Digraph(g0.VertexCount);
            for (int v0 = 0; v0 < g0.VertexCount; v0++)
            {
                g1.AddVertex();
            }
            var n = _map.TileDegree;

            for (int v0 = 0; v0 < g0.VertexCount; v0++)
            {
                var tile = _model.GetAssigned(v0);

                for (int i = 0; i < n; i++)
                {
                    var label = _map.GetLabel(i, tile);

                    if (label != "0")
                    {
                        int v1 = g0.GetVertexNeighborOut(v0, i);
                        g1.AddEdge(v0, v1);
                    }
                }
            }

            return g1;
        }

        public void InstantiateVertexObjects()
        {
            var g = ExtractJointEdgeGraph();

            for (int i = 0; i < g.VertexCount; i++)
            {
                var vobj = Instantiate(_vertexPrefab, transform);
                vobj.transform.localPosition = _tileGraph.VertexObjects[i].transform.position;
            }
        }



            public EdgeGraph ExtractJointEdgeGraph()
        {
            if (_model == null)
                Initialize();

            var g0 = _tileGraph.Graph;
            var g1 = new EdgeGraph(g0.VertexCount);

            for (int v0 = 0; v0 < g0.VertexCount; v0++)
            {
                g1.AddVertex();
            }

            var n = _map.TileDegree;

            for (int v0 = 0; v0 < g0.VertexCount; v0++)
            {
                var tile = _model.GetAssigned(v0);

                for (int i = 0; i < n; i++)
                {
                    var label = _map.GetLabel(i, tile);

                    if (label != "0")
                    {
                        int v1 = g0.GetVertexNeighborOut(v0, i);
                        if (v0 != v1)
                        {
                            if (!g1.HasEdge(v1, v0) && !g1.HasEdge(v0, v1))
                            {
                                g1.AddEdge(v0, v1);
                            }
                        }
                    }
                }
            }

            return g1;
        }
        
 
        public void ExtractSharedJointEdgeGraph(SharedAnalysisEdgeGraph analysisGraph)
        {
            if (_model == null)
                Initialize();

            var g0 = _tileGraph.Graph;

            analysisGraph.Initialize(g0.VertexCount);
            analysisGraph.VertexObjects = _tileGraph.VertexObjects;

            for (int i = 0; i < g0.VertexCount; i++)
            {
                analysisGraph.Vertices[i] = _tileGraph.VertexObjects[i].transform.position;
            }

            var n = _map.TileDegree;

            for (int v0 = 0; v0 < g0.VertexCount; v0++)
            {
                var tile = _model.GetAssigned(v0);
                int tilenum = (int)tile;
                Vector3 v0position = _tileGraph.VertexObjects[v0].transform.position;

                for (int i = 0; i < n; i++)
                {
                    var label = _map.GetLabel(i, tile);

                    if (label != "0")
                    {
                        int v1 = g0.GetVertexNeighborOut(v0, i);
                        if (v0 != v1)
                        {
                            if (!analysisGraph.Graph.HasEdge(v1, v0) && !analysisGraph.Graph.HasEdge(v0, v1))
                            {
                                analysisGraph.Graph.AddEdge(v0, v1);
                                analysisGraph.LineIndices.Add(v0);
                                analysisGraph.LineIndices.Add(v1);
                            }
                        }
                    }
                }
            }
        }
    }
}

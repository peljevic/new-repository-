using System.Collections.Generic;
using UnityEngine;
using RC3.Graphs;
using RC3.WFC;

namespace RC3.Unity.WFCDemo
{
    /// <summary>
    /// Creates a new graph based on tile labels
    /// </summary>
    public class TileGraphExtractor : MonoBehaviour
    {
        [SerializeField] private SharedDigraph _tileGraph;
        [SerializeField] private string[] _validLabels;

        [SerializeField] private bool _applyTileFilter = false;
        [SerializeField] private List<int> _ignoreTiles;
         
        private TileModel _model;
        private TileMap<string> _map;
        private HashSet<string> _labelSet;

 
        /// <summary>
        /// 
        /// </summary>
        private void Initialize()
        {
            var manager = GetComponent<TileModelManager>();
            _model = manager.TileModel;
            _map = manager.TileMap;
            _labelSet = new HashSet<string>(_validLabels);
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

                if (_applyTileFilter == true && _ignoreTiles.Contains(tile))
                {
                    continue;
                }

                for (int i = 0; i < n; i++)
                {
                    var label = _map.GetLabel(i, tile);

                    if (_labelSet.Contains(label))
                    {
                        int v1 = g0.GetVertexNeighborOut(v0, i);
                        g1.AddEdge(v0, v1);
                    }
                }
            }

            return g1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EdgeGraph ExtractEdgeGraph()
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

                if (_applyTileFilter == true && _ignoreTiles.Contains(tile))
                {
                    continue;
                }

                for (int i = 0; i < n; i++)
                {
                    var label = _map.GetLabel(i, tile);

                    if (_labelSet.Contains(label))
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

  
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void ExtractSharedEdgeGraph(SharedAnalysisEdgeGraph analysisGraph)
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
                if (_applyTileFilter == true && _ignoreTiles.Contains(tilenum))
                {
                    continue;
                }

                Vector3 v0position = _tileGraph.VertexObjects[v0].transform.position;

                for (int i = 0; i < n; i++)
                {
                    var label = _map.GetLabel(i, tile);

                    if (_labelSet.Contains(label))
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

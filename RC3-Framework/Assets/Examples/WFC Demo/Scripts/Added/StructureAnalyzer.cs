using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RC3.Graphs;
using RC3.WFC;

using SpatialSlur.Core;

namespace RC3.Unity.WFCDemo
{
    public class StructureAnalyzer:MonoBehaviour
    {
        [SerializeField] private SharedDigraph _grid;
        private TileSet _tileSet;
        private List<VertexObject> _verts;
        private Digraph _graph;
        private CollapseStatus _status;
        private TileModelManager _manager;

        private int _kinematicTiles = 0;
        private float _kinematicPercent = 0;
        private List<VertexObject> _meshedTiles;

        [Range(0.0f, 10000.0f)]
        [SerializeField] private float MaxForce = 1000.0f;

        [Range(0.0f, 10000.0f)]
        [SerializeField] private float MaxTorque = 1000.0f;

        private void Awake()
        {
            _graph = _grid.Graph;
            _verts = _grid.VertexObjects;

            _manager = GetComponent<TileModelManager>();
            _tileSet = _manager.TileSet;

            _meshedTiles = new List<VertexObject>();

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
                ResetStructureAnalysisChanges();
        }

        public void Analyse()
        {
            AddKinematicToLowest();
            AddJointsToConnected();
            AddGravity();

            Debug.Log("Analyzed!");
        }

        private void ResetStructureAnalysisChanges()
        {
            RemoveGravity();
            RemoveJoints();
        }

        private float MinDistance()
        {
            _meshedTiles.Clear();

            foreach (var v in _verts)
            {
                if (v.Tile != _tileSet[0]) _meshedTiles.Add(v);
            }

            var minDistance = 0.0f;
            minDistance = _meshedTiles.Min(v => v.transform.position.y);

            //Debug.Log("Minimun distance from the ground is ???" + minDistance);
            return minDistance;
        }

        private void AddKinematicToLowest()
        {
            _kinematicPercent = 0.0f;
            int roundN = 0;

            var minDistance = MinDistance();
            var tolerance = 1.0f;

            foreach (var v in _verts)
            {
                v.Body.isKinematic = false;
            }

            foreach (var kinematic in _meshedTiles.Where
                (kinematic => SlurMath.ApproxEquals(kinematic.transform.position.y, minDistance, tolerance)))
            {
                kinematic.Body.isKinematic = true;
                _kinematicTiles++;
            }

           }

        private void CountKinematic()
        {
            foreach (var v in _verts)
            {
                if (v.Body.isKinematic) _kinematicTiles++;
            }
            // Debug.Log("Kinematic Tiles " + _kinematicPercent);
        }

        public void AddJointsToConnected()
        {
            for (int i = 0; i < _verts.Count; i++)
            {
                var v = _verts[i];

                var allNeigh = v.Tile.Labels;

                for (int j = 0; j < allNeigh.Length; j++)
                {
                    var neighbour = _graph.GetVertexNeighborOut(i, j);
                    var vn = _verts[neighbour];

                    if (allNeigh[j] != "0" && v != vn)
                    {
                        var vJoint = v.gameObject.AddComponent<FixedJoint>();
                        vJoint.connectedBody = vn.GetComponent<Rigidbody>();

                        vJoint.breakForce = MaxForce; //Mathf.Infinity;
                        vJoint.breakTorque = MaxTorque; //Mathf.Infinity;

                        v.AddJoints(vJoint);
                    }
                }
            }
        }

        private void AddGravity()
        {
            foreach (var v in _verts)
                v.Body.useGravity = true;
        }

        private void RemoveGravity()
        {
            foreach (var v in _verts)
                v.Body.useGravity = false;
        }

        private void RemoveJoints()
        {
            foreach (var v in _verts)
            {
                for (int i = 0; i < 14; i++)
                {
                    if (v.gameObject.GetComponent<FixedJoint>() != null)
                        Destroy(v.gameObject.GetComponent<FixedJoint>());
                }

            }
        }

        private void SeparateTiles()
        {
            _meshedTiles.Clear();

            for (int i = 0; i < _graph.VertexCount; i++)
            {
                var v = _verts[i];

                if (v.Tile != _tileSet[0])
                {
                    _meshedTiles.Add(v);
                }
            }
        }
    }
}
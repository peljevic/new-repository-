﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RC3.Graphs;
using RC3.WFC;
using RC3.Unity;

using SpatialSlur.Core;

namespace RC3.Unity.WFCDemo
{
    public class TileModelAnalyzer : MonoBehaviour
    {
        public bool _analysisOn = true;

        [SerializeField] private SharedDigraph _grid;
        [SerializeField] private float _neededArea = 30;
        [SerializeField] private float _maxDensity;
        [SerializeField] private float _areaTolerance;
        [SerializeField] public float _allowedDisplacement = 0;
        [SerializeField] private float _maxSpeed = 0;

        private TileSet _tileSet;
        Vector3[] _positions;

        List<VertexObject> _verts;
        private Digraph _graph;
        private CollapseStatus _status;
        private TileModelManager _manager;

        [Range(0.0f, 10000.0f)]
        [SerializeField] private float MaxForce = 1000.0f;

        [Range(0.0f, 10000.0f)]
        [SerializeField] private float MaxTorque = 1000.0f;

        private List<VertexObject> _meshedTiles;
        private List<VertexObject> _weakTiles;
        private List<VertexObject> _stableTiles;
        private int _kinematicTiles = 0;
        private float _kinematicPercent = 0;

        private float _totalArea =0;
        private float[] _densities;
        

        private void Awake()
        {
            _graph = _grid.Graph;
            _verts = _grid.VertexObjects;

            _manager = GetComponent<TileModelManager>();
            _tileSet = _manager.TileSet;

            _stableTiles = new List<VertexObject>();
            _weakTiles = new List<VertexObject>();
            _meshedTiles = new List<VertexObject>();
            _densities = new float[_verts.Count];

        }

        private void Update()
        {
            if (_analysisOn)    // if (_status == CollapseStatus.Complete) AnalyzeModel(); 
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    Debug.Log("Analyze methods called.");
                    AnalyzeModel();
                    MarkWeakTiles();

                }

                if (Input.GetKeyDown(KeyCode.Z))
                {
                    ResetStructureAnalysisChanges();
                }
            }

            VelocityCheck2();
        }

        private void MarkWeakTiles()
        {
            if (_weakTiles != null)
            {
              //  Debug.Log("weak tiles count " + _weakTiles.Count.ToString());
          
            }
        }

        /// <summary>
        /// Evaluations for the agent to collect
        /// </summary>
        public IEnumerable<VertexObject> WeakTiles
        {
            get { return _weakTiles; }

        }

        /// <summary>
        /// Evaluations for the agent to collect
        /// </summary>
        public IEnumerable<VertexObject> StableTiles
        {
            get { return _stableTiles; }

        }

        public float KinematicTilePercent 
        {
            get { return _kinematicPercent * 100f; }
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

        public void AnalyzeModel()
        {
            PositionSaver();
            //AreaAnalyzer();
            //CountAllDensities();
            // SunExposureAnalysis(); //TODO fix
            StructureAnalyzer();
        }

        #region Average Sun Exposure

        private void SunExposureAnalysis()
        {

        }

        private void CompareSunExposure()
        {
            var collisions = 0;

            foreach(var v in _verts)
            {
                collisions += v.SunCollisions;
            }
        }

        #endregion

        #region Area Analyzer 

        private bool AreaAnalyzer()
        {
            return SlurMath.ApproxEquals(AreaDeviation(), _neededArea, _areaTolerance);              
        }

        private float TotalArea()
        {
            _totalArea = 0;

            foreach (var v in _verts)
            {
                _totalArea += v.Tile.Area;
            }
            Debug.Log("Total Volume " + _totalArea);
            return _totalArea;
        }

        private float AreaDeviation()
        {
            return TotalArea() - _neededArea;
        }

        #endregion Area Analyzer 

        #region Density Analyzer 

        private void CountAllDensities()
        {           
            for (int i = 0; i < _verts.Count; i++)
            {
                _densities[i] = CountNeighbourhoodDensity(i);
                //Debug.Log("Density is " + _densities[i]);

                if (_densities[i] > _maxDensity)
                    _weakTiles.Add(_verts[i]);
                        // Debug.Log("Density on vertex " + i + "is too high");
            }
        }

        private float CountNeighbourhoodDensity(int vertex)
        {
            int neighCounter = 0;
            var neigh = _graph.GetVertexNeighborsOut(vertex);

            foreach (var v in neigh)
            {
                if (_verts[v].Tile != _tileSet[0])
                {
                    neighCounter++;
                }
            }
            float neighDensity = (float)neighCounter / 14.0f;
            Debug.Log("Density is " + neighDensity.ToString());

            return neighDensity;
        }
        
        #endregion Density Analyzer 

        #region Structure Analyzer

        private void StructureAnalyzer()
        {
            //AddKinematicToLowest();
            AddJointsToConnected();
            AddGravity();
            //VelocityNewCheck();
            //CheckVelocity();
            //VelocityCheck2();
            
        }

        private void ResetStructureAnalysisChanges()
        {
            RemoveGravity();
            RemoveJoints();
            RestoreInitialPositions();
        }

        private float MinDistance()
        {
            _meshedTiles.Clear();

            foreach(var v in _verts)
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

            foreach (var kinematic in _meshedTiles.Where(kinematic => SlurMath.ApproxEquals(kinematic.transform.position.y, minDistance, tolerance)))
            {
                kinematic.Body.isKinematic = true;
                _kinematicTiles++;
            }

            //_kinematicPercent = (float)_kinematicTiles /(float) _meshedTiles.Count;
            //roundN = (int)(_kinematicPercent * 100);
            //Debug.Log("Non-empty tiles count is " + _meshedTiles.Count.ToString());
            //CountKinematic();
            //Debug.Log("Kinematic Tile in % " + roundN);
        }

        private void CountKinematic()
        {
            foreach(var v in _verts)
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

                        vJoint.breakForce = Mathf.Infinity; ;
                        vJoint.breakTorque = Mathf.Infinity; 

                        v.AddJoints(vJoint);
                    }
                }
            }
        }

        void VelocityNewCheck()
        {
            for (int i = 0; i < _verts.Count; i++)
            {
                var v = _verts[i];
                //var _speed = v.Velocity;
                var speed = v.GetComponent<Rigidbody>().velocity;
                var _speed = v.Body.velocity.magnitude;
                Debug.Log("the speed is " + _speed);
                var zeroVector = new Vector3(0,0,0);

                if(v.Body==null)
                {
                    Debug.Log("YOU DONT HAVE A RIGIDBODY!!!!");
                }


                if(speed != zeroVector)
                {
                   // Debug.Log("Bigger than 0");
                }

                if (_speed > _maxSpeed && v.Tile != _tileSet[0])
                {
                  //  Debug.Log("the speed is " + _speed);
                    _weakTiles.Add(v);
                }
            }
        }


        private void CheckVelocity()
        {
            foreach (var v in _verts)
            {
                var _displacement = v.Body.velocity;
                Debug.Log(_displacement);

                if (_displacement == Vector3.zero)
                {
                   // Debug.Log("Displacement is 0");
                    //_weakTiles.Add(v);
                    //_displacementValues[v.Tile.Index] = _displacement;
                    // v.GetComponent<Renderer>().material.color = Color.red;
                }

                if (_displacement != Vector3.zero)
                {
                  //  Debug.Log("Displacement is NOT 0");
                }
                //else if (_displacement == _allowedDisplacement)
                //{
                //    _stableTiles.Add(v);
                //}
            
            }
        }


        void VelocityCheck2()
        {


            var maxv = _verts.Select(v => v.Body.velocity.sqrMagnitude).Max();

           // Debug.Log($"Max velocity = {maxv}");
        }



        private void PositionSaver()
        {
            _positions = new Vector3[_graph.VertexCount];
            
           for(int i=0; i<_graph.VertexCount; i++)
            {
                _positions[i] = _verts[i].transform.position;
            }
        }

        private void RestoreInitialPositions()
        {
            for (int i = 0; i < _graph.VertexCount; i++)
            {
                _verts[i].transform.position = _positions[i];
            }
        }

        private float MaxVelocity(VertexObject vertex)
        {
            var _XmeshDisplacement = vertex.GetComponent<Rigidbody>().velocity.x;
            var _YmeshDisplacement = vertex.GetComponent<Rigidbody>().velocity.y;
            var _ZmeshDisplacement = vertex.GetComponent<Rigidbody>().velocity.z;

            var maxValue = Mathf.Max(_XmeshDisplacement, _YmeshDisplacement, _ZmeshDisplacement) * 1000;
            //Debug.Log(maxValue);

            return maxValue;
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

        private IEnumerable<Vector3> GetVertexPositions()
        {
            var gCreator = GetComponent<TruncatedOctahedralGraphCreator>();
            var countX = gCreator.CountX;
            var countY = gCreator.CountY;
            var countZ = gCreator.CountZ;

            for (int z = 0; z < countX; z++)
            {
                for (int y = 0; y < countY; y++)
                {
                    for (int x = 0; x < countX; x++)
                        yield return new Vector3(x, y, z);
                }
            }

            for (int z = 0; z < countZ; z++)
            {
                for (int y = 0; y < countY; y++)
                {
                    for (int x = 0; x < countX; x++)
                        yield return new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
                }
            }
        }

        private List<Vector3> CreateVertexObjects()
        {
            List<Vector3> originalPos = new List<Vector3>();

            foreach (var p in GetVertexPositions())
            {
                originalPos.Add(p * 2.0f);
            }
            return originalPos;
        }

        private void ResetPositions()
        {
            var position = CreateVertexObjects();

            for(int i =0; i<_graph.VertexCount; i++)
            {
                var vertex = _verts[i];
                vertex.transform.position = position[i];
               
            }
        }

        #endregion Structure Analyzer
    }
}
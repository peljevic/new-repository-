using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RC3.Graphs;
using System;

using SpatialSlur.Core;


namespace RC3.Unity.WFCDemo
{
    public class TileTypeCounter : MonoBehaviour
    {
        #region OnGuiGetters

        public int[] CounterFull
        {
            get { return _counts; }
        }

        public int CurrentArea
        {
            get { return _totalArea; }
        }

        public int PositionsAvailable
        {
            get {return _remainingPositions; }
        }

        public int GroundSupport
        {
            get { return _tilesTouchingGround; }
        }

        public int OpenPositionsOnGround
        {
            get { return (_positionsAvailableToTouchGround - _tilesTouchingGround); }
        }

        public float PositionsOnGroundPercent
        {
            get { return ((float)_tilesTouchingGround/(float)_positionsAvailableToTouchGround); }
        }

        public int RemainingPositions
        {
            get { return _remainingPositions; }
        }

        public int MaxAreaToFill
        {
            get { return _remainingPositions * _maxAreaTile; }
        }

        #endregion

        [SerializeField] private SharedDigraph _sharedGraph;
        [SerializeField] private int _neededArea = 3146;

        private List<VertexObject> _verts;
        private TileSet _tileSet;
        private float _min = 0;
        private int _maxAreaTile = 0;
        private Digraph _graph;

        //gui featured
        private int _countRemainingDefault;
        private int _totalArea = 0;
        private int _remainingPositions;
        private int _tilesTouchingGround = 0;
        private int _positionsAvailableToTouchGround = 0;
        private int[] _counts;
        private int[] _densities;

        float _resetValueMin;
        int _resetGroundPositions;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
                CountReset();
        }

        private void Awake()
        {
            _graph = _sharedGraph.Graph;
            _verts = _sharedGraph.VertexObjects;
            _remainingPositions = _verts.Count;
            _positionsAvailableToTouchGround = _countRemainingDefault;
            _tileSet = GetComponent<TileModelManager>().TileSet;
            _counts = new int[_tileSet.Count];
            _densities = new int[_verts.Count];

            LowestPosition();
           // _maxAreaTile = _tileSet.Select(t => t.Area).Max();
           // Debug.Log($"Max area tile is {_maxAreaTile}");
        }

        public int[] Densities()
        {
            StoreDensities();
            return _densities;
        }

        public int[] NeighPopulation()
        {
            StoreDensities();
            return _densities;
        }

        private void StoreDensities()
        {
            for(int i =0; i<_verts.Count; i++)
            {
                _densities[i] = _verts[i].NeighbourPopulation; 
            }
        }

        public void AreaTrackKeeper(int i)
        {
            var area = _tileSet[i].Area;
            _totalArea += area;
            _neededArea -= area;
        }

        public void Count(int tileIndex, int vertexPosition)
        {          
            _counts[tileIndex]++;

            //Update Density
            if (tileIndex != 0)
            {               
                var neighbourhood = _graph.GetVertexNeighborsOut(vertexPosition);
                foreach (var n in neighbourhood)
                {                    
                    _verts[n].AddedNeighbour();
                }
            }
            
            //Debug.Log("Count type " + i + " is " + _counts[i]);
            AreaTrackKeeper(tileIndex);
            //Debug.Log("Area total " + _totalArea.ToString());
            _remainingPositions--;
           // Debug.Log("There are " + _remainingPositions.ToString() + " places left to fill.");
            CheckMinPosition(tileIndex, vertexPosition);

        }

        private void UpdateDensity()
        {

        }

        public TileSet TileSet
        {
            get { return _tileSet; }
        }
        
        public int TotalVertices
        {
            get { return _verts.Count; }  
        }

        public int ReturnCount (int tileIndex)
        {
            return _counts[tileIndex];
        }

        private void LowestPosition()
        {
            _min = _verts.Min(v => v.transform.position.y);
            var tolerance = 1.0f;

            foreach (var v in _verts.Where(v=> SlurMath.ApproxEquals(v.transform.position.y, _min, tolerance)))
            {
               _positionsAvailableToTouchGround++;           
            }

            _resetValueMin = _min;
            _resetGroundPositions = _positionsAvailableToTouchGround;
            //Debug.Log("lowest is " + _min);
            //Debug.Log("Positions on the ground " + _positionsAvailableToTouchGround);
        }

        private void CheckMinPosition(int tileIndex, int vertexPosition)
        { 
            var tolerance = 1.0f;

            if (SlurMath.ApproxEquals(_verts[vertexPosition].transform.position.y, _min, tolerance) && (tileIndex != 0))
            {
                _verts[vertexPosition].Body.isKinematic = true;
                _tilesTouchingGround++;
            }

            //Debug.Log("Touching ground " + _tilesTouchingGround);
        }

        public void CountReset()
        {
            // _positionsAvailableToTouchGround = 0;
            _remainingPositions = _verts.Count-1;
            _positionsAvailableToTouchGround = _resetGroundPositions;
            _neededArea = 3146;
            _totalArea = 0;

            for (int i = 0; i < _counts.Length; i++)
            {
                _counts[i] = 0;
            }

            _tilesTouchingGround = 0;
            _min = _resetValueMin;

            foreach(var v in _verts)
            {
               v.Body.isKinematic = false;
            }
                       
        }
    }
}
         
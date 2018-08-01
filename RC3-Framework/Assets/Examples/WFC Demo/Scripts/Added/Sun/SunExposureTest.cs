﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RC3.Graphs;

namespace RC3.Unity.WFCDemo
{
    public class SunExposureTest : MonoBehaviour
    {
        [SerializeField] private SharedDigraph _tileGraph;
        private Digraph _graph;
        private List<VertexObject> _verts;
        private float _exposureResult = 0;
        public bool sunExposureDone = false;

        public GameObject sunSimulator;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                Debug.Log("Sun Exposure Test is triggered!");
                KinematicJoints();
                sunSimulator.GetComponent<SunAnalysis>().StartSimulation();
                sunExposureDone = true;
            }
        }

        public void StartSunSimulation()
        {
            Debug.Log("Sun Exposure Test is triggered!");
            KinematicJoints();
            sunSimulator.GetComponent<SunAnalysis>().StartSimulation();
        }


        public float SunExposureCount
        {
            set { _exposureResult = value; }
            get { return _exposureResult; }
        }


        private void KinematicJoints()
        {
            _graph = _tileGraph.Graph;
            _verts = _tileGraph.VertexObjects;

            foreach (var v in _verts)
            {
                v.Body.isKinematic = true;
            }
        }

    }
}

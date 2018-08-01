using System.Linq;
using UnityEngine;
using RC3.WFC;

namespace RC3.Unity.WFCDemo
{
    public class StructureGraphExtractor : MonoBehaviour
    {
        [SerializeField] private SharedDigraph _tileGraph;
        [SerializeField] private TileGraphExtractor _graphExtractor;
        [SerializeField] private TileModelManager _tileModelManager;
        [SerializeField] private SharedAnalysisEdgeGraph _analysisGraph;
       
        private TileModel _tileModel;

        private float[] _averageForces;
        private float[] _averageTorques;
        private float _maxForce;
        private float _minForce;
        private float _maxTorque;
        private float _minTorque;

        private void Awake()
        {

            if (_tileModelManager != null)
            {
                _tileModel = _tileModelManager.TileModel;
            }

            _analysisGraph.Initialize();
        }

        public void CollectStructureInformation()
        {
            for (int i = 0; i < _tileGraph.VertexObjects.Count; i++)
            {
                var v = _tileGraph.VertexObjects[i];
                _analysisGraph.ForceStress[i] = _averageForces[i] = v.JointAvgForce();
                _analysisGraph.TorqueStress[i] = _averageTorques[i] = v.JointAvgTorque();
            }

        }

        private void StructureExtremeValues()
        {
            _analysisGraph.MaxForce = _maxForce = _averageForces.Max();
            _analysisGraph.MaxTorque = _maxTorque = _averageTorques.Max();

            _minForce = _averageForces.Min();
            _minTorque = _averageTorques.Min();
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RC3.Unity.WFCDemo
{
    public class FillBar : MonoBehaviour
    {
        [SerializeField] private GameObject sunSimulator;
        private Transform fill;
        private Image img;
        private SunExposureTest _test;
        private SunAnalysis _analysis;
        private bool _triger;

        void Awake()
        {
            img = GetComponent<Image>();
            _test =sunSimulator.GetComponent<SunExposureTest>();
            _analysis = sunSimulator.GetComponent<SunAnalysis>();
            
        }

        void Start()
        {
            _triger = _test.sunExposureDone;
        }
        void DisplaySunExposure()
        {
            float filling = _analysis.SunExposureAnalysisCount;
            transform.GetComponent<Slider>().value = Mathf.Abs(filling);
        }

        void Update()
        {
           
            if (_triger)
            DisplaySunExposure();
        }
    }
}

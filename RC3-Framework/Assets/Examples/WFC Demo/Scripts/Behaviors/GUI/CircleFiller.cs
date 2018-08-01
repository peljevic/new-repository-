using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RC3.Unity.WFCDemo
{
    public class CircleFiller : MonoBehaviour
    {
        [SerializeField] private Image _circle;
        [SerializeField] private float _percentage;
        [SerializeField] private RectTransform _position;


        private void Awake()
        {
            _circle = GetComponent<Image>();
        }

        private void Start()
        {
            CirclePercentage();
        }

        public float FillingPercent
        {
            get { return _percentage; }
            set { _percentage = value; }
        }

        private void CirclePercentage()
        {
            _circle.fillAmount = _percentage;
        }
    }
}

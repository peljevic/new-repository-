﻿
/*
 * Notes
 */

using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using SpatialSlur.Core;
using RC3.WFC;

namespace RC3.Unity.WFCDemo
{
    /// <summary>
    /// 
    /// </summary>
    public class ProbabilisticTileSelector : MonoBehaviour, ITileSelector
    {
        [SerializeField] private TileTypeCounter _counter;
        [SerializeField] private TileSet _tileSet;
        [SerializeField] private int _seed;

        private ProbabilitySelector _selector;
        private double[] _weights;

      
        private void Awake()
        {
            _weights = GetTileWeights().ToArray();
            ValidateWeights();

            _selector = new ProbabilitySelector(_weights, new System.Random(_seed));
        }


        /// <summary>
        /// 
        /// </summary>
        private void ValidateWeights()
        {
            var min = _weights.Min();

            if (min <= 0.0)
                throw new ArgumentOutOfRangeException("Weights must be positive and non-zero!");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IEnumerable<double> GetTileWeights()
        {
            for (int i = 0; i < _tileSet.Count; i++)
                yield return _tileSet[i].Weight; // TODO assign actual weights
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public int Select(TileModel model, int position)
        {
            var d = model.GetDomain(position);
            _selector.SetWeights(GetModifiedWeights(d)); // update the weights in the selector

            var nextTile = _selector.Next();

            _counter.Count(nextTile, position);

            if (nextTile >= _tileSet.Count)
            {
                Debug.Log("Next tile out of range");
                return 0;
            }

            return nextTile;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        private IEnumerable<double> GetModifiedWeights(ReadOnlySet<int> domain)
        {
            for (int i = 0; i < _weights.Length; i++)
                yield return domain.Contains(i) ? _weights[i] : 0.0;
        }
    }
}
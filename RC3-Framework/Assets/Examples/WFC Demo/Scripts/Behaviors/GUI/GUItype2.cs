using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

namespace RC3.Unity.WFCDemo
{
    public class GUItype2 : MonoBehaviour
    {
        [SerializeField] private SharedAnalysisEdgeGraph _analysisGraph;
        [SerializeField] private GUISkin mySkin;
        [SerializeField] private TileTypeCounter _counter;
        [SerializeField] private GameObject _prefabCircleBar;
        [SerializeField] private GameObject _tileModel;
        [SerializeField] private GameObject _barSprite;
        [SerializeField] private GameObject _barParent;
        private TileSet _tileSet;
        private List<GameObject> _circles;
        private List<GameObject> _barStats;

        public bool graphOn = false;
        //connectedComponents;
        //normalizedcomponents;
        //normalizedcomponentsbysize;
        //componentCount;
        //sources;
        //depths;
        //normalizeddepths;
        float maxDepth;
        int unreachableverticesCount;
        int edgelessverticesCount;
        int closureCount = 0;

        private int _vertCount;
        private int _unassigned;
        private int[] _counts;
        private int _totalArea = 0;
        private int _neededArea = 3146;
        private int _tilesTouchingGround = 0;
        private int _stabilityPercent = 0;

        public Texture _fillingTexture;
        public SpriteRenderer _spriteFill;

        private List<float> _stats;

        private void Awake()
        {
            _tileSet = _tileModel.GetComponent<TileModelManager>().TileSet;
            _circles = new List<GameObject>();
            _barStats = new List<GameObject>();
            _vertCount = _counter.RemainingPositions;
        }

        private void Start()
        {
            TakeTileCounter();
           
            InstantiateCircle();

            InstatiateBars();

          //  UpdateTilePicture();
        }

        private void Update()
        {
            TakeRemainingStats();
            UpdateFill();

            UpdateFillOnBar();


        }

        void TakeTileCounter()
        {
            if (_counts == null)
            {
                _counts = _counter.CounterFull;
            }
        }

        void TakeRemainingStats()
        {
            _unassigned = _counter.RemainingPositions;
        
            _totalArea = _counter.CurrentArea;
       
            _tilesTouchingGround = _counter.GroundSupport;

            _stabilityPercent = (int)(_counter.PositionsOnGroundPercent*100);

        }

        public void AnalysisInformation()
        {
            closureCount = _analysisGraph.ClosuresCount;
            //_analysisGraph.ConnectedComponents = connectedComponents;
            //_analysisGraph.NormalizedComponents = normalizedcomponents;
            //_analysisGraph.NormalizedComponentsBySize = normalizedcomponentsbysize;
            //_analysisGraph.ConnectedComponentsCount = componentCount;
            //_analysisGraph.Sources = sources;
            //_analysisGraph.Depths = depths;
            //_analysisGraph.NormalizedDepths = normalizeddepths;
            maxDepth = _analysisGraph.MaxDepth;
            unreachableverticesCount = _analysisGraph.UnreachableVertices.Count;
            edgelessverticesCount = _analysisGraph.EdgelessVerticesCount;

            graphOn = true;
        }

        void TakeStats()
        {
            if(_stats==null)
            {
                _stats.Add(_unassigned);
                _stats.Add(_totalArea);
                _stats.Add(_tilesTouchingGround);
                _stats.Add(_stabilityPercent);
            }
        }

        public void ResetValues()
        {
            for (int i = 0; i < _counts.Length; i++)
            {
                _counts[i] = 0;
            }

            UpdateFill();

            _counts = _counter.CounterFull;
            _totalArea = 0;
            _tilesTouchingGround = 0;
            _stabilityPercent = 0;

            TakeRemainingStats();
        }

        private void InstantiateCircle()
        {
            for (int i = 0; i < _counts.Length; i++)
            {

                if (i % 2 == 0)
                {
                    var bar = Instantiate(_prefabCircleBar);
                    bar.transform.position = new Vector3(Screen.width - 110, Screen.height - (30 * i) - 35);
                   // bar.transform.position = new Vector3(Screen.width - 120, Screen.height - (35 * i) -35);
                    bar.transform.parent = gameObject.transform;
                    _circles.Add(bar);
                }

                else
                {
                    var bar = Instantiate(_prefabCircleBar);
                    bar.transform.position = new Vector3(Screen.width - 45, 5 + (30 * i));
                   // bar.transform.position = new Vector3(Screen.width - 50, 15 + (35 * i));
                    bar.transform.parent = gameObject.transform;
                    _circles.Add(bar);
                }

            }
        }

        private void UpdateTilePicture()
        {
            for (int i = 0; i < _tileSet.Count; i++)
            {
                    var sprite = Instantiate(_spriteFill, transform.parent = _circles[i].transform);
                    sprite.GetComponent<SpriteRenderer>().sprite = _tileSet[i].Sprite;
             
            }
        }

        private void InstantiateCircle2()
        {
            for (int i = 0; i < _counts.Length; i++)
            {

                if (i % 2 == 0)
                {
                    var bar = Instantiate(_prefabCircleBar);
                    bar.transform.SetParent(gameObject.transform, false);
                   // bar.transform = new RectTransform(Screen.width - 120, Screen.height - (35 * i) - 35);
                    _circles.Add(bar);
                }

                else
                {
                    var bar = Instantiate(_prefabCircleBar);
                    bar.transform.position = new Vector3(Screen.width - 50, 15 + (35 * i));
                    bar.transform.parent = gameObject.transform;
                    _circles.Add(bar);
                }

            }
        }

        private void UpdateFill()
        {
            for (int i = 0; i < _tileSet.Count; i++)
            {
                if (_counts[i] > 0)
                {
                    var image = _circles[i].GetComponent<Image>();

                    image.fillAmount = (float)_counts[i] / (float)_counter.TotalVertices * 3.60f;
                    _circles[i].GetComponentInChildren<Text>().text = _counts[i].ToString();

                    if (image.GetComponentInChildren<SpriteRenderer>().sprite == null)
                    { image.GetComponentInChildren<SpriteRenderer>().sprite = _tileSet[i].Sprite; }

                    //var sprite = Instantiate(_spriteFill, transform.parent = _circles[i].transform);
                    //sprite.GetComponent<Image>().sprite = _tileSet[i].Sprite;
                }
              
            }
        }

        void InstatiateBars()
        {
           
                var bar = Instantiate(_barSprite);               
                bar.transform.parent =_barParent.transform;
                bar.transform.position = _barParent.transform.position;
                _barStats.Add(bar);

            var bar2 = Instantiate(_barSprite);
            bar2.transform.parent = _barParent.transform;
            bar2.transform.position = _barParent.transform.position + new Vector3(0,0,0);
            _barStats.Add(bar2);
          
        }

        void UpdateFillOnBar()
        {
            var bar2 = _barStats[1].GetComponent<Image>();
            bar2.fillAmount = (float)_vertCount /((float)_unassigned +1);

            var bar = _barStats[0].GetComponent<Image>();
            bar.fillAmount = ((float)_totalArea / (float)_neededArea);

            //var bar1 = _barStats[1].GetComponent<Image>();
            //bar1.fillAmount = _tilesTouchingGround * 0.1f;

            //var bar2 = _barStats[2].GetComponent<Image>();
            //bar2.fillAmount = 0.1f*_stabilityPercent;
        }


        private void OnGUI()
        {
            GUI.skin = mySkin;

            if (!_fillingTexture)
            {
                Debug.LogError("Assign a Texture in the inspector.");
                return;
            }

            GUI.Label(new Rect(new Vector2(10, 350 + 20), new Vector2(250, 100)), $"Total area: {_totalArea}");
            GUI.Label(new Rect(new Vector2(10, 350 + 80), new Vector2(250, 100)), $"Base tiles: {_tilesTouchingGround}");
            GUI.Label(new Rect(new Vector2(10, 350 + 60), new Vector2(250, 100)), $"Base tile percentage: {_stabilityPercent}%");
            GUI.Label(new Rect(new Vector2(10, 350 + 40), new Vector2(250, 100)), $"Not yet assigned: {_unassigned}");
            GUI.Label(new Rect(new Vector2(10, 350 + 100), new Vector2(250, 100)), $"Max density: {_counter.Densities().Max()}");
            GUI.Label(new Rect(new Vector2(10, 350 + 120), new Vector2(250, 100)), $"Min density: {_counter.Densities().Min()}");

            if (graphOn)
            {
                GUI.Label(new Rect(new Vector2(10, 350 + 180), new Vector2(250, 100)), $"Closures on the graph {closureCount}");
                GUI.Label(new Rect(new Vector2(10, 350 + 200), new Vector2(250, 100)), $"Max depth {maxDepth}");
                GUI.Label(new Rect(new Vector2(10, 350 + 140), new Vector2(250, 100)), $"Unreachable vertices {unreachableverticesCount}");
                GUI.Label(new Rect(new Vector2(10, 350 + 160), new Vector2(250, 100)), $"Edgeless vertices {edgelessverticesCount}");
            }
        }
    }
}
          
           
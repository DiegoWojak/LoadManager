using Assets.Source.Managers.Components;

using Assets.Source.Utilities;
using Assets.Source.Utilities.Helpers;
using System;
using System.Collections;

using UnityEngine;

using UnityEngine.Playables;

using static Assets.Source.Managers.SceneLoaderManager;

namespace Assets.Source.Managers
{
    public class GameStarterManager : LoaderBase<GameStarterManager>
    {

        public event Action OnCameraChangeRequiered;

        [Space(10)]
        [Header("Light Source")]
        public GameObject FromAbove;
        public GameObject FromScreenPlayer;

        [Header("Material PostProcessing")]
        public Material FromDistance;
        public Material FromClose;
        public Material FromReading;
        public Material FromWater;
        public Material FromRGBGlasses;

        private PostProcessEffect _camComponent;

        bool bufferisWatering = false;
        bool bufferIsReading = false;

        [Space(10)]
        [Header("Effect Table")]
        [SerializeField]
        private System.Collections.Generic.List<EffectDictionary> d_Effect;

        [SerializeField]
        private ApplyEffectFunctionsHelper Helper;
        public EffectsManagerComponent _EffectsComponent { private set; get; }

        public PlayableDirector Director;

        [SerializeField]
        public bool GameBegin { get; private set; }

        public Vector3 PointToSpawmSaved { get; private set; }
        public Quaternion RotationToSpawmSaved { get; private set; }


        [SerializeField]
        private GameObject MainCamera;
        [SerializeField]
        private GameObject Player;
        [SerializeField]
        private TextWritingBehavior _textWritingBehavior;
        public override void Init()
        {
            GameEvents.Instance.onPlayerFallingOffScreen += RespawntoCheckPoint;
            GameEvents.Instance.onCheckPointEnter += SaveNewPosition;
            
            isLoaded = true;

        }

        public void CameraDistanceChange(bool _distanceZero) {
            
        }

        public void CameraFluidChange(bool isWatering) {
           
        }

        public void CameraReadingChange(bool isReading) {
            
        }


        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }          

        [ContextMenu("Let the player Move")]
        public void StartGame() {
            
            
        }


        public void PreStartGame() {
            GameObject _go = GameObject.Find("STARTPOINT");
            SaveNewPosition("", _go.transform.position, _go.transform.rotation);
        }

        private void BlockInteraction() {
            UIManager.Instance.RequestOpenUI(this, false);
        }

        public void AllowInteraction() {
            UIManager.Instance.RequestCloseUI(this);
            GameBegin = true;
        }


        void RespawntoCheckPoint() {
            
        }

        void RespawntoCheckPointForce(bool force = false)
        {
           
        }

        void SaveNewPosition(string id, Vector3 position, Quaternion rotation)
        {
            PointToSpawmSaved = position;
            RotationToSpawmSaved = rotation;
        }
       

        public void CompletetLevel() {
            UIManager.Instance.RequestOpenUI(this, true, (resu) => {
                var _status = UIManager.Instance.ShowStatus();

                SceneInformation _scene = SceneLoaderManager.Instance.Scenes[SceneLoaderManager.Instance.SceneTarget];

                _status.RequestWriteMessage(_scene.FinalMessageOnGameEnd,_scene.SomeRanking ,() => {
                    StartCoroutine(DelayTime(2, () => {
                        UIManager.Instance.RequestCloseUI(this, (bo) => {
                            UIManager.Instance.CloseStatus();
                        });

                        GameBegin = false;
                        GameSoundMusicManager.Instance.StopBackgroundMusic();
                        SceneLoaderManager.Instance.LoadNextLevel();
                    }));
                });
            });
        }

        IEnumerator DelayTime(float time ,Action _do) {
            yield return time;
            _do.Invoke();
        }
    }
}

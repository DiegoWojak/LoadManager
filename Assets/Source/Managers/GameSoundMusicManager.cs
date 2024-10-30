

using Assets.Source.Utilities.Events;
using Assets.Source.Utilities.Helpers;
using Assets.Source.Utilities.Helpers.Gizmo;
#if FMOD_ENABLE
using FMODUnity;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Assets.Source.Managers
{
    [Serializable]
    public class GameSoundMusicManager : LoaderBase<GameSoundMusicManager>
    {
        public float delayBetweenFootStepSound;
#if FMOD_ENABLE
        public Dictionary<PredefinedSounds, EventReference> SoundDictionary;
        public Dictionary<PredefinedMusics, EventReference> MusicsDictionary;
        public AudioIntensityComponent _audioIntensityController { get; private set; }
        [SerializeField]
        private StudioEventEmitter _audioEmiter;
        public override void Init()
        {
            if (_audioEmiter == null)
            {
                var go = GameObject.Find("BackgroundMusic");
                if (go != null) _audioEmiter = go.GetComponent<StudioEventEmitter>();
            }
            _audioIntensityController = new AudioIntensityComponent(_audioEmiter);
            StartCoroutine(
                SoundInitialization(
                    () => {
                        isLoaded = true;
                    }
                )
             );
        }
#endif


        IEnumerator SoundInitialization(Action Callback)
        {
            yield return InitDictionary();

            Callback?.Invoke();

        }

        public void PlaySoundByPredefinedKey(PredefinedSounds _key)
        {
#if FMOD_ENABLE
            RuntimeManager.PlayOneShot(SoundDictionary[_key]);
#endif
        }


        IEnumerator InitDictionary()
        {
#if FMOD_ENABLE
            SoundDictionary = new Dictionary<PredefinedSounds, EventReference>();
            
            var _sound = RuntimeManager.PathToEventReference("event:/UI/Okay");
            EvaluateEventRef(ref _sound, PredefinedSounds.ComputerTurning);

            MusicsDictionary = new Dictionary<PredefinedMusics, EventReference>();

            var _music = RuntimeManager.PathToEventReference("event:/Music/Introduction");
            if (!_music.IsNull) {
                MusicsDictionary.Add(PredefinedMusics.Introduction,_music);
            }

            _music = RuntimeManager.PathToEventReference("event:/Music/NIvel1");
            if (!_music.IsNull)
            {
                MusicsDictionary.Add(PredefinedMusics.Nivel1, _music);
            }
#endif
            yield return null;
        }


        public void PlayPlayerFootStep()
        {
            if (!isLoaded) return;

#if FMOD_ENABLE
             {
                if (!SoundDictionary[PredefinedSounds.PlayerFootStep].IsNull)
                {
                    FMOD.Studio.EventInstance e = RuntimeManager.CreateInstance(SoundDictionary[PredefinedSounds.PlayerFootStep]);
                    e.set3DAttributes(RuntimeUtils.To3DAttributes((Vector3)My3DHandlerPlayer.Instance?.Character.Motor.InitialTickPosition));

                    e.start();
                    e.release();//Release each event instance immediately, there are fire and forget, one-shot instances. 
                }
            }
#endif
        }

        public void StartBackgroundMusic()
        {
            int level = SceneLoaderManager.Instance.SceneTarget;

#if FMOD_ENABLE
            var key = SceneLoaderManager.Instance.Scenes[level].MusicBackgroundURL;
            _audioEmiter.EventReference = MusicsDictionary[key];
            _audioIntensityController.CleanAudioIntensity();
            _audioEmiter.Play();
#endif
        }

        public void StopBackgroundMusic()
        {
#if FMOD_ENABLE
            _audioEmiter.Stop();
#endif
        }


        private void EvaluateEventRef(ref object _obj, PredefinedSounds _key)
        {
#if FMOD_ENABLE
            EventReference _sound = _obj as EventReference;
            if (_sound.IsNull)
            {
#if UNITY_EDITOR
                string _msg = DebugUtils.GetMessageFormat($"Sound not Found for {_sound} ", 0);
                Debug.Log(_msg);
#endif
            }
            else
            {
#if UNITY_EDITOR
                string _msg = DebugUtils.GetMessageFormat($"Found Sounds for {_sound} ", 2);
                Debug.Log(_msg);
#endif
                SoundDictionary.Add(_key, _sound);
            }
        }
#endif

        }

        public enum PredefinedSounds
        {
            PlayerFootStep,
            PlayerJump,
            PlayerDash,
            ComputerTurning,
            ComputerInteracting,
            ComputerClose,
            OpenDoor,
            CloseDoor,
            NpcMaleInteract,
            NewItem,
            Checkpoint,
            FallingFromVoid
        }

        [Serializable]
        public enum PredefinedMusics
        {
            Introduction,
            Nivel1
        }

    }
}

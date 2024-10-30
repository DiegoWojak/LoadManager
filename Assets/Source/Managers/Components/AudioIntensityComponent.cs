#if FMOD_ENABLE
using FMODUnity;

namespace Assets.Source.Utilities.Events
{
    public class AudioIntensityComponent
    {
        protected StudioEventEmitter _audioEmiter;

        public AudioIntensityComponent(StudioEventEmitter audioEmiter)
        {
            GameEvents.Instance.onAudioItensityController += SetAudioIntensity;
            _audioEmiter = audioEmiter;
        }



        public virtual void SetAudioIntensity(string id, AudioIntensityController _c )
        {
            _audioEmiter.SetParameter(_c.parameter, _c.value);

            _c.PostAction?.Invoke();
        }

        public virtual void CleanAudioIntensity() {
            _audioEmiter.SetParameter("Intensity", 0);
            _audioEmiter.SetParameter("Progression", 0);
        }

    }



}

#endif
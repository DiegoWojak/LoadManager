﻿

using Assets.Source.Managers;
using UnityEngine;

namespace Assets.Source.Utilities.Events
{
    public class CheckPointController : MonoBehaviour
    {
        string id;
        private void Start()
        {
            var _te = GetComponent<TriggerArea>();
            id = _te.id;
            _te.RelatedActionOnEnter = delegate {
                GameEvents.Instance?.OnComponentWithTriggerEnter(this, id);
#if FMOD_ENABLE
                GameSoundMusicManager.Instance.PlaySoundByPredefinedKey(PredefinedSounds.Checkpoint);
#endif
                gameObject.SetActive(false);
            };
        }
    }
}

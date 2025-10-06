
using System;
using System.Collections;

using UnityEngine;

namespace Assets.Source.Managers
{
    public class GameStarterManager : LoaderBase<GameStarterManager>
    {

        public event Action OnCameraChangeRequiered;
        
        [SerializeField]
        public bool GameBegin { get; private set; }
        [SerializeField]
        private GameObject MainCamera;
        [SerializeField]
        private GameObject Player;
        
        public override void Init()
        {
                       
            isLoaded = true;
        }

              

        public void CompletetLevel() {
        
        }

        IEnumerator DelayTime(float time ,Action _do) {
            yield return time;
            _do.Invoke();
        }
    }
}

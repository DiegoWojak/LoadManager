
using Assets.Source.Managers;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Assets.Source.Utilities
{
    public class GameEvents: LoaderBase<GameEvents>
    {
        public override void Init()
        {
            
            isLoaded = true;
        }

        public event Action<string> onComputerTriggerEnter;
        public event Action<string> onComputerTriggerExit;
        public event Action<string> onDoorTriggerEnter;
        public event Action<string> onDoorTriggerExit;
        public event Action<string> onNpcTriggerEnter;
        public event Action<string> onNpcTriggerExit;
        public event Action onPlayerFallingOffScreen;
        public event Action<string,Vector3, Quaternion> onCheckPointEnter;
        public event Action<MonoBehaviour> OnInteract;
        
    }   
}


using System;
using UnityEngine;

namespace Assets.Source.Utilities.Events
{
    public class TriggerArea : MonoBehaviour
    {
        public string id;
        public Action<string> RelatedActionOnEnter;
        public Action<string> RelatedActionOnLeave;
        [SerializeField]
        private bool userPlayerTag = true;
        private GameObject Target;
        private void Awake()
        {
            Target = GameObject.FindGameObjectWithTag("Player");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == Target.name) {
                Debug.Log($"Gameobject {other.name} has entered to Trigger Area with ID {id} ");
                RelatedActionOnEnter?.Invoke(id);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name == Target.name)
            {
                Debug.Log($"Gameobject {other.name} is Leaving the Trigger Area with ID {id}");
                RelatedActionOnLeave?.Invoke(id);
            }
        }
    }
}

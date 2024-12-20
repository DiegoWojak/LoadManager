﻿#if UNITY_EDITOR
using Assets.Source.Utilities.Helpers.Gizmo;
#endif

using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;
using UnityEngine;

namespace Assets.Source
{
    public class LoaderManager : MonoBehaviour
    {
        [SerializeField]
        List<MonoBehaviour> Dependencies;

        [SerializeField]
        Queue<IInitiable> _stacks = new Queue<IInitiable>();

        public bool isEverythingLoaded = false;

        [SerializeField]
        private LoadingVisualSetup loadingVisualSetup;

        public static Action OnEverythingLoaded;

        public static LoaderManager Instance { get { return _instance; } }
        private static LoaderManager _instance;
        private void OnEnable()
        {
            _stacks = new Queue<IInitiable>();
            for (int i = 0; i < Dependencies.Count; i++) {
                _stacks.Enqueue(Dependencies[i] as IInitiable);
            }
            isEverythingLoaded = false;
            StartCoroutine(CoroutineLoad());
        }

        private void Awake()
        {
            if (Instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                return;
            }
            else { 
                Destroy(gameObject);
            }

        }

        private string messasge = string.Empty;
        private IEnumerator CoroutineLoad() {

            while (_stacks.Count > 0)
            {
                var loader = _stacks.Dequeue();
                loadingVisualSetup.SetText(_stacks.Count, Dependencies.Count);

                loader.Init();

                // Wait until the loader has finished loading
                while (!loader.IsLoaded())
                {
#if UNITY_EDITOR
                    Debug.Log("Loading");
#endif
                    yield return new WaitForSeconds(0.5f);
                }
#if UNITY_EDITOR
                messasge = DebugUtils.GetMessageFormat($"Loaded  <-- {(loader as MonoBehaviour).name}", 1);
                Debug.Log(messasge);
#endif
                yield return new WaitForSeconds(0.5f);
            }
#if UNITY_EDITOR
            messasge = DebugUtils.GetMessageFormat($"Everything Loaded", 1);
            Debug.Log(messasge);
#endif
            loadingVisualSetup.OnLoaded();
            isEverythingLoaded = true;
            OnEverythingLoaded?.Invoke();
        }


        public void EnqueueProcess(Action Pre, IEnumerator ProcessToLoad, Action Post) {
            StartCoroutine(MainEnumator(Pre,ProcessToLoad,Post));
        }

        private IEnumerator MainEnumator(Action pre, IEnumerator _pr , Action Post) { 
            loadingVisualSetup.LoadingPage.SetActive(true);
            pre?.Invoke();

            while (_pr.MoveNext())
            {
                if (_pr.Current is float progress)
                {
                    loadingVisualSetup.SetText(progress);
                }
                yield return _pr.Current;
            }

            Post?.Invoke();
            loadingVisualSetup.LoadingPage.SetActive(false);
        }
    }

    

    [Serializable]
    public struct LoadingVisualSetup {
        public TextMeshProUGUI Text;
        public GameObject LoadingPage;

        public void SetText(int stackSize, int totalSize) {
            float percent = ((float)(totalSize-stackSize) / totalSize * 100);
            Text.text = $"Loading: {percent:F0}%";
        }

        public void SetText(float  percent)
        {
            Text.text = $"Loading: {percent:F0}%";
        }

        public void OnLoaded() { 
            LoadingPage.SetActive(false);
        }
    }
}

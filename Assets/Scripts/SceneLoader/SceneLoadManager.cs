using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InventorySystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace SceneLoader
{
    public class SceneLoadManager : MonoBehaviour
    {
        [SerializeField] private string sceneName;
        [SerializeField] private Slider loadingSlider;

        private void Start()
        {
            LoadScene(sceneName);
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadAsync(sceneName));
        }

        private IEnumerator LoadAsync(string levelName)
        {
            yield return new WaitForSeconds(Random.Range(0.3f, 1.2f));

            float targetProgress = 0;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName);

            while (!asyncLoad.isDone)
            {
                targetProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                loadingSlider.value = Mathf.Lerp(loadingSlider.value, targetProgress, Time.deltaTime * 5);
                yield return null;
            }
            
            Scene loadedScene = SceneManager.GetSceneByName(levelName);
            ActivateAllDynamicInterfaces(loadedScene);
        }

        public void ActivateAllDynamicInterfaces(Scene scene)
        {
            StartCoroutine(ActivateAndDeactivateInterfaces(scene));
        }

        private IEnumerator ActivateAndDeactivateInterfaces(Scene scene)
        {
            DynamicInterface[] dynamicInterfaces = scene.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<DynamicInterface>(true))
                .ToArray();

            List<Transform> transforms = new List<Transform>();

            foreach (var dynamicInterface in dynamicInterfaces)
            {
                Transform parent = dynamicInterface.transform.parent;

                while (parent)
                {
                    if (!parent.gameObject.activeSelf) transforms.Add(parent);

                    parent.gameObject.SetActive(true);
                    parent = parent.parent;
                }

                dynamicInterface.gameObject.SetActive(true);
                transforms.Add(dynamicInterface.transform);
            }

            yield return new WaitForEndOfFrame();

            foreach (var objectTransform in transforms)
            {
                objectTransform.gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(Random.Range(0.3f, 1.2f));
        }
    }
}
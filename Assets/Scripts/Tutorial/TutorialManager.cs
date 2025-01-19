using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject[] tutorialPages;
        [SerializeField] private TMP_Text pagesCounter;
        private int currentPage = 0;
        private bool hasBeenAccessed;

        private void Awake()
        {
            if (currentPage == tutorialPages.Length - 1)
            {
                nextButton.interactable = false;
            }

            if (currentPage == 0)
            {
                previousButton.interactable = false;
            }

            UpdatePageCounter();
        }

        private void Start()
        {
            nextButton.onClick.AddListener(NextPage);
            previousButton.onClick.AddListener(PreviousPage);
            closeButton.onClick.AddListener(CloseTutorial);
            hasBeenAccessed = PlayerPrefs.GetInt("TutorialAccessed", 0) == 1;
            
            if (hasBeenAccessed) CloseTutorial();
            
            PlayerPrefs.SetInt("TutorialAccessed", 1);
        }

        private void NextPage()
        {
            tutorialPages[currentPage].SetActive(false);
            currentPage++;
            tutorialPages[currentPage].SetActive(true);
            previousButton.interactable = true;
            if (currentPage == tutorialPages.Length - 1)
            {
                nextButton.interactable = false;
            }

            UpdatePageCounter();
        }

        private void PreviousPage()
        {
            tutorialPages[currentPage].SetActive(false);
            currentPage--;
            tutorialPages[currentPage].SetActive(true);
            nextButton.interactable = true;
            if (currentPage == 0)
            {
                previousButton.interactable = false;
            }

            UpdatePageCounter();
        }

        private void CloseTutorial()
        {
            transform.parent.gameObject.SetActive(false);
        }

        private void UpdatePageCounter()
        {
            pagesCounter.text = $"{currentPage + 1}/{tutorialPages.Length}";
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

namespace KawaiiKiller.Cinematics
{
    public class ComicPage : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private string pageTitle;
        [SerializeField] private List<ComicVignette> vignettes;

        [Header("UI")]
        [SerializeField] private TMP_Text titleText;

        public int VignetteCount => vignettes != null ? vignettes.Count : 0;

        private void Awake()
        {
            if (titleText != null)
                titleText.text = pageTitle;
        }

        public void Initialize()
        {
            if (vignettes == null) return;

            foreach (var v in vignettes)
            {
                v.Reset();
            }
        }

        public void SetActive(bool state)
        {
            gameObject.SetActive(state);
        }

        public void FadeIn(int index, float duration)
        {
            if (index < 0 || index >= vignettes.Count) return;
            vignettes[index].FadeIn(duration);
        }

        public void CompleteFadeIn(int index)
        {
            if (index < 0 || index >= vignettes.Count) return;
            vignettes[index].CompleteFadeIn();
        }

        public void FadeOutAll(float duration)
        {
            if (vignettes == null) return;

            foreach (var v in vignettes)
            {
                v.FadeOut(duration);
            }
        }

        public void ShowAll()
        {
            if (vignettes == null) return;

            foreach (var v in vignettes)
            {
                v.Show();
            }
        }
    }
}
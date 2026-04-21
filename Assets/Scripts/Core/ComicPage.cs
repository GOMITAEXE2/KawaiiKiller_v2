using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Linq;

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
            // Auto-populate vignettes if list is empty
            if (vignettes == null || vignettes.Count == 0)
            {
                vignettes = GetComponentsInChildren<ComicVignette>(true)
                    .OrderBy(v => v.name)
                    .ToList();
                
                if (vignettes.Count > 0)
                {
                    Debug.Log($"[ComicPage] {gameObject.name} auto-populated {vignettes.Count} vignettes (Sorted by Name).");
                }
            }

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

        public Tween FadeIn(int index, float duration)
        {
            if (index < 0 || index >= vignettes.Count) return null;
            return vignettes[index].FadeIn(duration);
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
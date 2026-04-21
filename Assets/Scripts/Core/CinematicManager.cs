using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KawaiiKiller.Cinematics;

public class CinematicManager : MonoBehaviour
{
    [System.Serializable]
    private class RoundCinematic
    {
        public int roundAfter;
        public List<ComicPanelSystem.ComicPanel> panels;
    }

    [Header("Intro Cinematic")]
    [SerializeField] private List<ComicPanelSystem.ComicPanel> introPanels;

    [Header("Round Cinematics")]
    [SerializeField] private ComicPanelSystem comicSystem;
    [SerializeField] private List<RoundCinematic> cinematics;

    public void PlayIntroCinematic(Action onComplete)
    {
        if (introPanels == null || introPanels.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(PlayIntroRoutine(onComplete));
    }

    private IEnumerator PlayIntroRoutine(Action onComplete)
    {
        if (comicSystem != null)
        {
            yield return StartCoroutine(comicSystem.Play(introPanels, onComplete));
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
            onComplete?.Invoke();
        }
    }

    public void PlayRoundCinematic(int completedRound, Action onComplete)
    {
        if (comicSystem == null)
        {
            onComplete?.Invoke();
            return;
        }

        RoundCinematic entry = cinematics?.Find(c => c.roundAfter == completedRound);
        if (entry == null || entry.panels == null || entry.panels.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(PlayRoundRoutine(entry.panels, onComplete));
    }

    public bool HasCinematicForRound(int roundNumber)
    {
        if (introPanels != null && introPanels.Count > 0 && roundNumber == 1)
            return true;

        if (cinematics == null) return false;

        RoundCinematic entry = cinematics.Find(c => c.roundAfter == roundNumber);
        return entry != null && entry.panels != null && entry.panels.Count > 0;
    }

    private IEnumerator PlayRoundRoutine(List<ComicPanelSystem.ComicPanel> panels, Action onComplete)
    {
        yield return StartCoroutine(comicSystem.Play(panels, onComplete));
    }
}
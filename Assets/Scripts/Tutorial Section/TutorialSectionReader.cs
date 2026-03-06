using System;
using System.Collections;
using UnityEngine;

public class TutorialSectionReader : MonoBehaviour
{
    private bool _hasMoved, _hasVaulted, _hasJumped, _hasSteeredBoat;
    [SerializeField] private float revealSpeed = 1f;
    [SerializeField] private CanvasGroup[] canvasGroups;

    public static bool TutorialComplete;

    private void OnEnable()
    {
        TutorialComplete = false;
    }

    private void OnDestroy()
    {
        
    }

    private void Start()
    {
        foreach (var group in canvasGroups) group.alpha = 0f;

        // TODO: Match to the Game Manager to trigger on game start
        StartCoroutine(InputReaderRoutine());
    }

    private IEnumerator InputReaderRoutine()
    {
        yield return new WaitUntil(() => _hasMoved);
        yield return new WaitUntil(() => RevealContent(0));
        
        yield return new WaitUntil(() => _hasVaulted);
        yield return new WaitUntil(() => RevealContent(1));
        
        yield return new WaitUntil(() => _hasJumped);
        yield return new WaitUntil(() => RevealContent(2));
        
        yield return new WaitUntil(() => _hasSteeredBoat);
        yield return new WaitUntil(() => RevealContent(3));
        
        TutorialCompleted();

        // Cycle through the canvas groups and reduce their transparency
        var amount = canvasGroups.Length;
        while (!FadeContent(amount)) // Wait until the last group has finished fading
        {
            for (int i = 0; i < amount; i++) FadeContent(i);
            yield return null;
        }
        
        // Destroy the tutorial section
        Destroy(gameObject);
    }

    private bool RevealContent(int id, float multiplier = 1f)
    {
        var group = canvasGroups[id];
        group.alpha += (Time.deltaTime * revealSpeed * multiplier); // Update alpha amount

        // Check if the alpha group has been fully revealed
        return group.alpha < 1f;
    }
    
    private bool FadeContent(int id, float multiplier = -1f)
    {
        var group = canvasGroups[id];
        group.alpha += (Time.deltaTime * revealSpeed * multiplier); // Update alpha amount

        // Check if the alpha group has disappeared
        return group.alpha > 0f;
    }

    private void TutorialCompleted()
    {
        TutorialComplete = true;
        Debug.Log("Tutorial Completed");
    }
}

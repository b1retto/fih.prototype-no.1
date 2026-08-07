using UnityEngine;
using System.Collections.Generic;

public class BGMScript : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;

    private List<int> availableIndices;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        availableIndices = new List<int>();
        for (int i = 0; i < audioClips.Length; i++)
        {
            availableIndices.Add(i);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PlayBGM();
        }
    }

    public void PlayBGM()
    {
        if (audioClips == null || audioClips.Length == 0)
        {
            Debug.LogWarning("No audio clips assigned!");
            return;
        }

        if (availableIndices.Count == 0)
        {
            for (int i = 0; i < audioClips.Length; i++)
            {
                availableIndices.Add(i);
            }
        }

        int randomPos = Random.Range(0, availableIndices.Count);
        int randomIndex = availableIndices[randomPos];

        availableIndices.RemoveAt(randomPos);

        Debug.Log($"Song no. {randomIndex}");

        if (audioSource)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(audioClips[randomIndex]);
        }
    }
}
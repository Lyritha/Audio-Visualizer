using UnityEngine;
using TMPro;
using System;
using System.Collections;
using Newtonsoft.Json;

public class Lyrics : MonoBehaviour
{
    [Serializable]
    private enum TextPos
    {
        Center,
        Left,
        Right
    }

    [Serializable]
    private class TextFragmentWrapper
    {
        public TextFragment[] fragments;
    }

    [Serializable]
    private struct TextFragment
    {
        [TextArea] public string text;
        public float timeInSong;
        public TextPos pos;
    }

    [SerializeField] private float fadeTime = 0.5f;

    [SerializeField] private TextFragment[] textFragments;

    [SerializeField] private GameObject textFieldPrefabLeft;
    [SerializeField] private GameObject textFieldPrefabCenter;
    [SerializeField] private GameObject textFieldPrefabRight;

    [SerializeField] private int currentFragmentIndex = 0;

    private float currentTextTime = 0;
    private float previousTextTime = 0;
    private GameObject currentTextOBJ = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start() => LoadLyrics();

    public void StartLyrics()
    {
        if (textFragments != null) StartCoroutine(DisplayText());
    }

    private void LoadLyrics()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Lyrics");
        if (jsonFile == null)
        {
            Debug.LogError("Could not find JSON file in Resources folder.");
            return;
        }

        textFragments = JsonConvert.DeserializeObject<TextFragmentWrapper>(jsonFile.text).fragments;
    }

    private IEnumerator DisplayText()
    {
        // save the time of the current fragment to get the difference of the new fragment
        previousTextTime = currentTextTime;

        // increment index
        currentFragmentIndex++;

        if (currentFragmentIndex >= textFragments.Length)
        {
            yield return new WaitForSeconds(2);
            if (currentTextOBJ != null) StartCoroutine(FadeOut(currentTextOBJ, fadeTime));
            yield return null;
        }

        if (currentFragmentIndex < textFragments.Length)
        {
            // get the time of the current fragment
            currentTextTime = textFragments[currentFragmentIndex].timeInSong;
 
            // wait for the delay between text
            yield return new WaitForSeconds(currentTextTime - previousTextTime - fadeTime);

            // fade out the previous text, only if there is a previous text
            if (currentTextOBJ != null) StartCoroutine(FadeOut(currentTextOBJ, fadeTime));

            yield return new WaitForSeconds(fadeTime);

            // fade in the text
            StartCoroutine(FadeIn(CreateOBJ(), fadeTime));

            // go to next cycle
            StartCoroutine(DisplayText());

            yield return null;
        }

        yield return null;
    }

    private TMP_Text CreateOBJ()
    {
        GameObject objectToInstance = textFragments[currentFragmentIndex].pos switch
        {
            TextPos.Left => textFieldPrefabLeft,
            TextPos.Center => textFieldPrefabCenter,
            TextPos.Right => textFieldPrefabRight,
            _ => textFieldPrefabCenter
        };

        // spawn a new text Object, and set its parent
        currentTextOBJ = Instantiate(objectToInstance);
        currentTextOBJ.transform.SetParent(transform, false);

        // set the text of the text object
        TMP_Text text = currentTextOBJ.GetComponent<TMP_Text>();
        text.text = textFragments[currentFragmentIndex].text;

        return text;
    }

    private IEnumerator FadeIn(TMP_Text text, float delay)
    {
        float time = 0;

        Color startColor = new(1, 1, 1, 0);
        Color endColor = new(1, 1, 1, 1);

        // lerping
        while (time < delay)
        {
            text.color = Color.Lerp(startColor, endColor, time / delay);

            time += Time.deltaTime;

            yield return null;
        }

        text.color = endColor;

        yield return null;
    }

    private IEnumerator FadeOut(GameObject textOBJ, float delay)
    {
        TMP_Text text = textOBJ.GetComponent<TMP_Text>();

        float time = 0;

        Color startColor = new(1, 1, 1, 1);
        Color endColor = new(1, 1, 1, 0);

        // lerping
        while (time < delay)
        {
            text.color = Color.Lerp(startColor, endColor, time / delay);

            time += Time.deltaTime;

            yield return null;
        }

        // destroy the object
        Destroy(textOBJ);

        yield return null;
    }
}

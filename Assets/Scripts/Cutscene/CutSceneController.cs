using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutSceneController : MonoBehaviour
{
    [SerializeField] private Image[] frames;
    [SerializeField] private float frameDuration = 2f;
    [SerializeField] private Image fadeImage; // siyah ekran
    [SerializeField] private float fadeDuration = 0.5f;


    public void StartCutScene()
    {
        StartCoroutine(PlayCutScene());
    }

    IEnumerator PlayCutScene()
    {
        fadeImage.color = new Color(0, 0, 0, 0);

        for (int i = 0; i < frames.Length; i++)
        {
            yield return StartCoroutine(Fade(0f, 1f));

            foreach (var f in frames)
                f.gameObject.SetActive(false);

            frames[i].gameObject.SetActive(true);

            yield return StartCoroutine(Fade(1f, 0f));

            yield return new WaitForSeconds(frameDuration);
        }

        yield return StartCoroutine(Fade(0f, 1f));
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float t = 0f;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, endAlpha, t / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, endAlpha);
    }
}
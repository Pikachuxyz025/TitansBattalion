using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public GameObject blackoutSqaure;
    public CanvasGroup[] UiElements;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(FadeOutSquare(true, 2));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(FadeOutSquare(false, 2));
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            FadeIn();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            FadeOut();
        }
    }

    public IEnumerator FadeOutSquare(bool fadeToBlack, int fadespeed)
    {
        Color objectColor = blackoutSqaure.GetComponent<Image>().color;
        float fadeAmount;

        if (fadeToBlack)
        {
            while (blackoutSqaure.GetComponent<Image>().color.a < 1)
            {
                fadeAmount = objectColor.a + (fadespeed * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                blackoutSqaure.GetComponent<Image>().color = objectColor;
                yield return null;
            }
        }
        else
        {
            while (blackoutSqaure.GetComponent<Image>().color.a > 0)
            {
                fadeAmount = objectColor.a - (fadespeed * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                blackoutSqaure.GetComponent<Image>().color = objectColor;
                yield return null;
            }
        }
    }

    void FadeIn()
    {
        StartCoroutine(FadeCanvasGroup(UiElements, 1, 5f, true));
    }
    void FadeOut()
    {
        StartCoroutine(FadeCanvasGroup(UiElements, 0, 5f, false));
    }
    public void ChangeScene(string scene)
    {
        //SceneManager.LoadScene(scene);
    }
    public IEnumerator FadeCanvasGroup(CanvasGroup[] cg, float end, float lerptime, bool backorForth)
    {
        float _timeStartedLerping = Time.time, timeSinceStarted = Time.time - _timeStartedLerping, percentageComplete = timeSinceStarted / lerptime;
        if (backorForth)
        {
            float start = cg[0].alpha;
            while (true)
            {
                timeSinceStarted = Time.time - _timeStartedLerping;
                percentageComplete = timeSinceStarted / lerptime;

                float currentValue = Mathf.Lerp(start, end, percentageComplete);

                cg[0].alpha = currentValue;
                if (percentageComplete >= 1)
                    break;
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(.1f);
            start = cg[1].alpha;
            _timeStartedLerping = Time.time;
            while (true)
            {
                timeSinceStarted = Time.time - _timeStartedLerping;
                percentageComplete = timeSinceStarted / 2f;

                float currentValue = Mathf.Lerp(start, end, percentageComplete);

                cg[1].alpha = currentValue;
                if (percentageComplete >= 1)
                    break;
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(.05f);
            _timeStartedLerping = Time.time;
            while (true)
            {
                timeSinceStarted = Time.time - _timeStartedLerping;
                percentageComplete = timeSinceStarted / 1f;

                float currentValue = Mathf.Lerp(start, end, percentageComplete);

                cg[2].alpha = currentValue;
                cg[3].alpha = currentValue;
                cg[4].alpha = currentValue;
                if (percentageComplete >= 1)
                {
                    cg[3].GetComponent<Button>().enabled = true;
                    cg[4].GetComponent<Button>().enabled = true;
                    break;
                }
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            cg[3].GetComponent<Button>().enabled = false;
            cg[4].GetComponent<Button>().enabled = false;
            float start = cg[0].alpha;
            while (true)
            {
                timeSinceStarted = Time.time - _timeStartedLerping;
                percentageComplete = timeSinceStarted / 1f;

                float currentValue = Mathf.Lerp(start, end, percentageComplete);

                cg[1].alpha = currentValue;
                cg[2].alpha = currentValue;
                cg[3].alpha = currentValue;
                cg[4].alpha = currentValue;
                if (percentageComplete >= 1)
                    break;
                yield return new WaitForEndOfFrame();
            }
            while (true)
            {
                timeSinceStarted = Time.time - _timeStartedLerping;
                percentageComplete = timeSinceStarted / lerptime;

                float currentValue = Mathf.Lerp(start, end, percentageComplete);

                cg[0].alpha = currentValue;
                if (percentageComplete >= 1)
                    break;
                yield return new WaitForEndOfFrame();
            }
        }
    }
    public void FormIncrease(int index)
    {
        UiElements[index].GetComponent<Transform>().localScale = Vector2.Lerp(UiElements[index].GetComponent<Transform>().localScale, new Vector2(1f, 1f), 1f);
    }
    public void FormDecrease(int index)
    {
        UiElements[index].GetComponent<Transform>().localScale = Vector2.Lerp(UiElements[index].GetComponent<Transform>().localScale, new Vector2(.8f, .8f), 1f);
    }
}
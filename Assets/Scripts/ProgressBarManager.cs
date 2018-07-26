using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarManager : MonoBehaviour {

    public GameObject progressPanel;
    [SerializeField] public AnimatedProgressbar progressbar;
    public Text progressText;

    [SerializeField] public float speed;

    private void Start()
    {
    }

    public void SetProgressVisible(bool visible)
    {
        progressPanel.SetActive(visible);
    }
	
    public void SetProgress(float oldProgress, float newProgress)
    {
        StartCoroutine(Progress(oldProgress, newProgress));
    }

    private IEnumerator Progress(float oldProgress, float newProgress)
    {
        float fillAmount = oldProgress;

        while (fillAmount < newProgress)
        {
            fillAmount += speed;
            progressbar.FillAmount = fillAmount;
            yield return null;
        }
    }

    private void OnGUI()
    {
        progressText.text = DRUtil.progressString;
    }
}

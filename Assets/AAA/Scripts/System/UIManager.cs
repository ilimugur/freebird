using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
	public CanvasGroup Hud;
	public CanvasGroup StartScreen;
	public CanvasGroup GameOverScreen;
	public CanvasGroup LevelCompleteScreen;
	public ProgressBar ProgressBar;
	public TMP_Text ScoreText;
	public TMP_Text GoodJobText;

	public TMP_Text GameOverScreenScoreText;
	public TMP_Text GameOverScreenBestScoreText;

	public TMP_Text GameOverScreenRankText;
	public TMP_Text GameOverScreenNextRankText;

	public TMP_Text CurrentLevelText;
	public TMP_Text NextLevelText;

	public TMP_Text ScoreTypeText;

	public Animator FingerTappingAnimator;
	public Coroutine FingerTappingCoroutine;
	public Coroutine UiMessageHidingCoroutine;

	void Awake()
	{
		Hud.gameObject.SetActive(true);
		StartScreen.gameObject.SetActive(true);
		GameOverScreen.gameObject.SetActive(true);
		LevelCompleteScreen.gameObject.SetActive(true);

		ScoreTypeText.gameObject.SetActive(false);
		GoodJobText.gameObject.SetActive(false);
		FingerTappingAnimator.gameObject.SetActive(false);

		HideGameHud(0);
		HideStartScreen(0);
		HideGameOverScreen(0);
		HideLevelCompleteScreen(0);
		ProgressBar.Progress = 0;
	}

	public void ShowGameHud(float duration)
	{
		Hud.DOFade(1, duration);
		Hud.interactable = true;
		Hud.blocksRaycasts = true;
	}
	public void HideGameHud(float duration)
	{
		Hud.DOFade(0, duration);
		Hud.interactable = false;
		Hud.blocksRaycasts = false;
	}

	public void ShowStartScreen(float duration)
	{
		StartScreen.DOFade(1, duration);
		StartScreen.interactable = true;
		StartScreen.blocksRaycasts = true;
	}
	public void HideStartScreen(float duration)
	{
		StartScreen.DOFade(0, duration);
		StartScreen.interactable = false;
		StartScreen.blocksRaycasts = false;
	}

	public void ShowGameOverScreen(float duration)
	{
		GameOverScreen.DOFade(1, duration);
		GameOverScreen.interactable = true;
		GameOverScreen.blocksRaycasts = true;
	}
	public void HideGameOverScreen(float duration)
	{
		GameOverScreen.DOFade(0, duration);
		GameOverScreen.interactable = false;
		GameOverScreen.blocksRaycasts = false;
	}

	public void ShowLevelCompleteScreen(float duration)
	{
		LevelCompleteScreen.DOFade(1, duration);
		LevelCompleteScreen.interactable = true;
		LevelCompleteScreen.blocksRaycasts = true;
	}
	public void HideLevelCompleteScreen(float duration)
	{
		LevelCompleteScreen.DOFade(0, duration);
		LevelCompleteScreen.interactable = false;
		LevelCompleteScreen.blocksRaycasts = false;
	}

	public void ContinueGame()
	{
		HideGameOverScreen(0.1f);
		ShowGameHud(0.1f);
		EventManager.Instance.TriggerEvent(Constants.EVENT_GAME_CONTINUE_AFTER_GAME_OVER);
	}

	public void RestartGame()
	{
		HideGameOverScreen(0.1f);
		StartLevel();
	}

	public void RestartLevel()
	{
		HideGameOverScreen(0.1f);
		ShowGameHud(0.1f);
		EventManager.Instance.TriggerEvent(Constants.EVENT_LEVEL_RESTART);
	}

	public void StartNextLevel()
	{
		HideStartScreen(0.1f);
		HideLevelCompleteScreen(0.1f);
		ShowGameHud(0.1f);
		EventManager.Instance.TriggerEvent(Constants.EVENT_LEVEL_START_NEXT);
	}

	public void StartLevel()
	{
		EventManager.Instance.TriggerEvent(Constants.EVENT_LEVEL_START);
		
		EventManager.Instance.StartListening(Constants.EVENT_UI_MESSAGE, OnUiMessage);
		EventManager.Instance.StartListening(Constants.EVENT_LEVEL_COMPLETED, OnLevelComplete);
		EventManager.Instance.StartListening(Constants.EVENT_SET_PROGRESSBAR, (float value) => OnSetProgressBar(value));
		EventManager.Instance.StartListening(Constants.EVENT_SET_PROGRESSBAR, (int value) => OnSetProgressBar(value));
		EventManager.Instance.StartListening(Constants.EVENT_RESET_PROGRESSBAR, OnResetProgressBar);
		EventManager.Instance.StartListening(Constants.EVENT_GAME_OVER, OnGameOver);
		
		HideStartScreen(0.1f);
		ShowGameHud(0.1f);
	}

	public void OnUiMessage(string text)
	{
		GoodJobText.text = text;
		GoodJobText.transform.localScale = Vector3.one * 0.1f;
		GoodJobText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutElastic);
	}

	public IEnumerator HideUiMessage(float delay=3f)
	{
		yield return new WaitForSeconds(delay);
		GoodJobText.transform.DOScale(0.1f,0.3f).SetEase(Ease.InElastic).OnComplete(() =>
		{
			GoodJobText.text = "";
			GoodJobText.transform.localScale = Vector3.one;
		});
	}

	public void OnResetProgressBar()
	{
		ProgressBar.ResetProgress();
	}

	private void OnSetProgressBar(float value)
	{
		ProgressBar.Progress = value;
	}

	private void OnLevelComplete()
	{
		HideGameHud(0.1f);
		ShowLevelCompleteScreen(0.1f);
	}

	

	public void OnGameOver()
	{
		
		HideGameHud(0.1f);
		ShowGameOverScreen(0.1f);
	}

	private string GetSuffix(int value)
	{
		var lastDigit = (int)(10f * ((value / 10f) - (value / 10)));
		switch (lastDigit)
		{
			case 1: return "st";
			case 2: return "nd";
			case 3: return "rd";
			default: return "th";
		}
	}

	public void ShowFingerTapping()
	{
		if (FingerTappingCoroutine == null)
		{
			FingerTappingCoroutine = StartCoroutine(ShowFingerTappingCo());
		}
	}

	private IEnumerator ShowFingerTappingCo()
	{
		FingerTappingAnimator.gameObject.SetActive(true);
		yield return new WaitForSeconds(2.5f);
		FingerTappingAnimator.gameObject.SetActive(false);
		FingerTappingCoroutine = null;
	}


}

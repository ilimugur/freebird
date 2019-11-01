using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
	public Image Bar;
	public Image Background;
	public Image Frame;
	public Vector3 EmptyPosition;
	public Vector3 FullPosition;

	private float _progress;
	public float Progress
	{
		get
		{
			return _progress;
		}
		set
		{
			_progress = Mathf.Clamp01(value);
			
			Bar.rectTransform.DOLocalMoveX(EmptyPosition.x + _progress * (FullPosition - EmptyPosition).x,0.5f).SetEase(Ease.InOutCubic);
		}
	}

	void Start()
	{
		//Bar.GetComponent<Image>().material = GameManager.Instance.GameParameters.ProgressBarBarMaterial;
		//Background.GetComponent<Image>().material = GameManager.Instance.GameParameters.ProgressBarBackgroundMaterial;
		//Frame.GetComponent<Image>().material = GameManager.Instance.GameParameters.ProgressBarFrameMaterial;

	}

	public void ResetProgress()
	{
		_progress = 0;

		Bar.rectTransform.DOLocalMoveX(EmptyPosition.x + _progress * (FullPosition - EmptyPosition).x, 0).SetEase(Ease.InOutCubic);
	}

	public void SetBarColor(Color32 color)
	{
		Bar.color = color;
	}

	public void SetBackgroundColor(Color32 color)
	{
		Background.color = color;
	}

}

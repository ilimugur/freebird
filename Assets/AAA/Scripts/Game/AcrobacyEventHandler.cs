using UnityEngine;
using System.Collections;

public class AcrobacyEventHandler : MonoBehaviour
{
	public float LoopEventComboDistance = 5f;
	public float VerticalFlightTimeRequirement = 2f;
	public float LevelFlightTimeRequirement = 2f;

	private int _loopEventComboCount = 0;
	private AcrobacyEvent _lastLoopEvent;

	private AcrobacyEvent _verticalStanceStartEvent;
	private AcrobacyEvent _levelFlightStartEvent;

	// Use this for initialization
	void Start()
	{
		EventManager.Instance.StartListening(Constants.EVENT_ACROBACY_COMPLETE_LOOP, OnCompleteLoop);
		EventManager.Instance.StartListening(Constants.EVENT_ACROBACY_START_VERTICAL_STANCE, OnStartVerticalStance);
		EventManager.Instance.StartListening(Constants.EVENT_ACROBACY_END_VERTICAL_STANCE, OnEndVerticalStance);

		EventManager.Instance.StartListening(Constants.EVENT_ACROBACY_START_LEVEL_FLIGHT, OnStartLevelFlight);
		EventManager.Instance.StartListening(Constants.EVENT_ACROBACY_END_LEVEL_FLIGHT, OnEndLevelFlight);
	}

	public void OnCompleteLoop(AcrobacyEvent ev)
	{
		if (_lastLoopEvent != null && Vector3.Distance(ev.Position, _lastLoopEvent.Position) <= LoopEventComboDistance)
		{
			_loopEventComboCount++;
			_lastLoopEvent = ev;
			EventManager.Instance.TriggerEvent(Constants.EVENT_UI_MESSAGE, "LOOP! x" + _loopEventComboCount);
			EventManager.Instance.TriggerEvent(Constants.EVENT_INCREMENT_SCORE, _loopEventComboCount * Constants.ScoreBonusPerLoopCombo);
			EventManager.Instance.TriggerEvent(Constants.EVENT_GAIN_FUEL, _loopEventComboCount * Constants.FuelGainPerLoopCombo);
		}
		else
		{
			_loopEventComboCount = 1;
			_lastLoopEvent = ev;
			EventManager.Instance.TriggerEvent(Constants.EVENT_UI_MESSAGE, "LOOP!");
			EventManager.Instance.TriggerEvent(Constants.EVENT_INCREMENT_SCORE, _loopEventComboCount * Constants.ScoreBonusPerLoopCombo);
			EventManager.Instance.TriggerEvent(Constants.EVENT_GAIN_FUEL, _loopEventComboCount * Constants.FuelGainPerLoopCombo);
		}
	}

	public void OnStartVerticalStance(AcrobacyEvent ev)
	{
		_verticalStanceStartEvent = ev;
	}

	public void OnEndVerticalStance(AcrobacyEvent ev)
	{
		if (_verticalStanceStartEvent != null)
		{
			var time = Time.time - _verticalStanceStartEvent.Time;
			if (time >= VerticalFlightTimeRequirement)
			{
				var intTime = (int)time;
				EventManager.Instance.TriggerEvent(Constants.EVENT_UI_MESSAGE, "CLIMB " + intTime + "s");
				EventManager.Instance.TriggerEvent(Constants.EVENT_INCREMENT_SCORE, Constants.ScoreBonusPerSecondInVerticalStance);
				EventManager.Instance.TriggerEvent(Constants.EVENT_GAIN_FUEL, Constants.FuelGainPerVerticalStancePerSecond);
			}
			_verticalStanceStartEvent = null;
		}
	}

	public void OnStartLevelFlight(AcrobacyEvent ev)
	{
		_levelFlightStartEvent = ev;
	}

	public void OnEndLevelFlight(AcrobacyEvent ev)
	{
		if (_levelFlightStartEvent != null)
		{
			var time = Time.time - _levelFlightStartEvent.Time;
			if (time >= LevelFlightTimeRequirement)
			{
				var intTime = (int)time;
				EventManager.Instance.TriggerEvent(Constants.EVENT_UI_MESSAGE, "STRAIGHT " + intTime + "s");
				EventManager.Instance.TriggerEvent(Constants.EVENT_INCREMENT_SCORE, Constants.ScoreBonusPerSecondInHorizontalStance);
				EventManager.Instance.TriggerEvent(Constants.EVENT_GAIN_FUEL, Constants.FuelGainPerHorizontalStancePerSecond);
			}
			_levelFlightStartEvent = null;
		}
	}
}

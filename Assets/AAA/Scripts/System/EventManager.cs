using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : Singleton<EventManager>
{
	private Dictionary<string, UnityEvent> eventDictionary;
	private Dictionary<string, UnityEventInt> eventDictionaryInt;
	private Dictionary<string, UnityEventFloat> eventDictionaryFloat;
	private Dictionary<string, UnityEventString> eventDictionaryString;
	private Dictionary<string, UnityEventAcrobacyEvent> eventDictionaryAcrobacyEvent;

	public class UnityEventInt : UnityEvent<int> {}
	public class UnityEventFloat : UnityEvent<float> {}
	public class UnityEventString : UnityEvent<string> { }
	public class UnityEventAcrobacyEvent : UnityEvent<AcrobacyEvent> { }

	public bool LogTriggersToConsole = true;
	public bool LogListenersToConsole = true;

	void Awake()
	{
		Init();
	}

	void Init()
	{
		if (eventDictionary == null) eventDictionary = new Dictionary<string, UnityEvent>();
		if (eventDictionaryInt == null) eventDictionaryInt = new Dictionary<string, UnityEventInt>();
		if (eventDictionaryFloat == null) eventDictionaryFloat = new Dictionary<string, UnityEventFloat>();
		if (eventDictionaryString == null) eventDictionaryString = new Dictionary<string, UnityEventString>();
		if (eventDictionaryAcrobacyEvent == null) eventDictionaryAcrobacyEvent = new Dictionary<string, UnityEventAcrobacyEvent>();
	}

	public void StartListening(string eventName, UnityAction listener)
	{
		LogListenerRegistered(eventName, listener);
		UnityEvent thisEvent = null;
		if (eventDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.AddListener(listener);
		}
		else
		{
			thisEvent = new UnityEvent();
			thisEvent.AddListener(listener);
			eventDictionary.Add(eventName, thisEvent);
		}
	}

	public void StartListening(string eventName, UnityAction<int> listener) 
	{
		LogListenerRegistered(eventName, listener);
		UnityEventInt thisEvent = null;
		if (eventDictionaryInt.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.AddListener(listener);
		}
		else
		{
			thisEvent = new UnityEventInt();
			thisEvent.AddListener(listener);
			eventDictionaryInt.Add(eventName, thisEvent);
		}
	}

	public void StartListening(string eventName, UnityAction<float> listener)
	{
		LogListenerRegistered(eventName, listener);
		UnityEventFloat thisEvent = null;
		if (eventDictionaryFloat.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.AddListener(listener);
		}
		else
		{
			thisEvent = new UnityEventFloat();
			thisEvent.AddListener(listener);
			eventDictionaryFloat.Add(eventName, thisEvent);
		}
	}

	public void StartListening(string eventName, UnityAction<string> listener)
	{
		LogListenerRegistered(eventName, listener);
		UnityEventString thisEvent = null;
		if (eventDictionaryString.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.AddListener(listener);
		}
		else
		{
			thisEvent = new UnityEventString();
			thisEvent.AddListener(listener);
			eventDictionaryString.Add(eventName, thisEvent);
		}
	}

	public void StartListening(string eventName, UnityAction<AcrobacyEvent> listener)
	{
		LogListenerRegistered(eventName, listener);
		UnityEventAcrobacyEvent thisEvent = null;
		if (eventDictionaryAcrobacyEvent.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.AddListener(listener);
		}
		else
		{
			thisEvent = new UnityEventAcrobacyEvent();
			thisEvent.AddListener(listener);
			eventDictionaryAcrobacyEvent.Add(eventName, thisEvent);
		}
	}

	public void StopListening(string eventName, UnityAction listener)
	{
		LogListenerUnregistered(eventName, listener);
		UnityEvent thisEvent = null;
		if (eventDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.RemoveListener(listener);
		}
	}

	public void StopListening(string eventName, UnityAction<int> listener)
	{
		LogListenerUnregistered(eventName, listener);
		UnityEventInt thisEvent = null;
		if (eventDictionaryInt.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.RemoveListener(listener);
		}
	}
	public void StopListening(string eventName, UnityAction<float> listener)
	{
		LogListenerUnregistered(eventName, listener);
		UnityEventFloat thisEvent = null;
		if (eventDictionaryFloat.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.RemoveListener(listener);
		}
	}
	public void StopListening(string eventName, UnityAction<string> listener)
	{
		LogListenerUnregistered(eventName, listener);
		UnityEventString thisEvent = null;
		if (eventDictionaryString.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.RemoveListener(listener);
		}
	}
	public void StopListening(string eventName, UnityAction<AcrobacyEvent> listener)
	{
		LogListenerUnregistered(eventName, listener);
		UnityEventAcrobacyEvent thisEvent = null;
		if (eventDictionaryAcrobacyEvent.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.RemoveListener(listener);
		}
	}

	public void TriggerEvent(string eventName)
	{
		LogEventTriggered(eventName);
		UnityEvent thisEvent = null;
		if (eventDictionary.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.Invoke();
		}
	}

	public void TriggerEvent(string eventName, int value)
	{
		LogEventTriggered(eventName,value);
		UnityEventInt thisEvent = null;
		if (eventDictionaryInt.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.Invoke(value);
		}
	}

	public void TriggerEvent(string eventName, float value)
	{
		LogEventTriggered(eventName,value);
		UnityEventFloat thisEvent = null;
		if (eventDictionaryFloat.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.Invoke(value);
		}
	}

	public void TriggerEvent(string eventName, string value)
	{
		LogEventTriggered(eventName,value);
		UnityEventString thisEvent = null;
		if (eventDictionaryString.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.Invoke(value);
		}
	}
	public void TriggerEvent(string eventName, AcrobacyEvent value)
	{
		LogEventTriggered(eventName, value);
		UnityEventAcrobacyEvent thisEvent = null;
		if (eventDictionaryAcrobacyEvent.TryGetValue(eventName, out thisEvent))
		{
			thisEvent.Invoke(value);
		}
	}

	private void LogEventTriggered(string name, object value=null)
	{
		if(LogTriggersToConsole)
			Debug.LogFormat("EventManager.TriggerEvent {0} ({1})",name,value!=null?value.ToString():"");
	}

	private void LogListenerRegistered(string name, object listener)
	{
		if (LogListenersToConsole)
			Debug.LogFormat("EventManager.StartListening {0} ({1})", name, listener.ToString());
	}

	private void LogListenerUnregistered(string name, object listener)
	{
		if (LogListenersToConsole)
			Debug.LogFormat("EventManager.StopListening {0} ({1})", name, listener.ToString());
	}
}
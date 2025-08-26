using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace jb5n {
	public class AnimEventProcessor : MonoBehaviour {
		[System.Serializable]
		public struct EventElement {
			public string label;
			public UnityEvent unityEvent;
		}

		public List<EventElement> animEvents = new List<EventElement>();
		private Dictionary<string, UnityEvent> eventsDict = new Dictionary<string, UnityEvent>();

		private void Awake() {
			foreach (EventElement element in animEvents) {
				if (eventsDict.ContainsKey(element.label)) {
					Debug.LogError("AnimEventProcessor on " + gameObject.name + " has duplicate event labels " + element.label);
					continue;
				}
				eventsDict.Add(element.label, element.unityEvent);
			}
		}

		public void CallAnimEvent(string eventLabel) {
			if (!eventsDict.ContainsKey(eventLabel)) {
				Debug.LogError("AnimEventProcessor on " + gameObject.name + " called with unset event label " + eventLabel);
				return;
			}
			eventsDict[eventLabel].Invoke();
		}
	}
}

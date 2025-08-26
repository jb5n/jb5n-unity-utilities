using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace jb5n {

	[RequireComponent(typeof(TMP_Text))]
	public class TextTyper : MonoBehaviour {

		private class CharacterFadeData {
			public string character;
			public float fadePercent = 0f;
			public bool isRichTextTag = false;

			public CharacterFadeData(char character) {
				this.character = character.ToString();
			}

			public CharacterFadeData(string richTextTag) {
				character = richTextTag;
				isRichTextTag = true;
			}
		}

		// If empty, we will use whatever text is set in the TMP_Text component
		[TextArea(3, 10)]
		public string scriptToTypeOut;
		public float charactersPerSecond = 2f;
		public UnityEvent<TextTyper> onTextTypingComplete;

		[Header("Fading")]
		public bool fadeCharactersIn = false;
		public float characterFadeDurationSeconds = 1f;
		public bool useFadeAnimCurve = false;
		public AnimationCurve fadeAnimCurve;

		[Header("Formatting")]
		// If false, you will have to call StartTypingText() manually somewhere
		public bool playOnAwake = false;
		// If true, we will stop typing when paused or slow typing when in slomo
		public bool affectedByTimescale = true;
		// Determines whether the alignment of the text should consider characters that haven't yet been typed. If false, this
		// can cause text to jitter as newlines are introduced, or if the text is center- or right-aligned.
		public bool alignHiddenCharacters = true;
		// If true, whitespace will not count toward our charactersPerSecond limit
		public bool skipTypingWhitespace = true;
		// If true, punctuation will not count toward our charactersPerSecond limit
		public bool skipTypingPunctuation = true;

		private TMP_Text _textRenderer = null;
		private Coroutine _activeTypingCoroutine = null;

		void Awake() {
			_textRenderer = GetComponent<TMP_Text>();
			if (scriptToTypeOut.Length == 0 && _textRenderer.text.Length > 0) {
				scriptToTypeOut = _textRenderer.text;
				_textRenderer.text = "";
			}

			if (playOnAwake) {
				StartTypingText();
			}
		}

		[ContextMenu("Start Typing Text")]
		public void StartTypingText() {
			if (!_textRenderer) {
				Debug.LogError(gameObject.name + " failed to start typing using TextTyper. No TMP_Text component found.");
				return;
			}

			if (_activeTypingCoroutine != null) {
				StopCoroutine(_activeTypingCoroutine);
			}
			_activeTypingCoroutine = StartCoroutine(TypingTextCoroutine());
		}

		public void ResetText() {
			if (_activeTypingCoroutine != null) {
				StopCoroutine(_activeTypingCoroutine);
			}
			_textRenderer.text = "";
		}

		private IEnumerator TypingTextCoroutine() {
			const string hiddenOpeningTag = "<color=#FFFFFF00>";
			const string colorClosingTag = "</color>";

			float charactersToTypeCounter = 0f; // Tracks how many characters we are allowed to type this frame
			float characterFadeRate = 1f / characterFadeDurationSeconds;

			string completedCharacters = "";
			List<CharacterFadeData> typingCharacters = new List<CharacterFadeData>();

			int scriptTypingIndex = 0;

			while (scriptTypingIndex < scriptToTypeOut.Length || (fadeCharactersIn && typingCharacters.Count > 0)) {
				float deltaTime = affectedByTimescale ? Time.deltaTime : Time.fixedDeltaTime;
				charactersToTypeCounter += deltaTime * charactersPerSecond;

				while (charactersToTypeCounter >= 1f && scriptTypingIndex < scriptToTypeOut.Length) {
					int curTypingIndex = scriptTypingIndex;
					char typedCharacter = scriptToTypeOut[scriptTypingIndex++];

					// Identifies if we have a rich text tag. Add it to the fade data so it is positioned properly but we won't color it
					// TODO cannot correctly handle pre-existing color rich text
					if (!IsIndexEscaped(scriptToTypeOut, curTypingIndex) && typedCharacter == '<') {
						if (TryExtractRichTextTag(out int richTextEnd, out string richTextTag, scriptToTypeOut, curTypingIndex)) {
							if (fadeCharactersIn) {
								typingCharacters.Add(new CharacterFadeData(richTextTag));
							}
							else {
								completedCharacters += typedCharacter;
							}
							scriptTypingIndex = richTextEnd;
							continue;
						}
					}

					if (fadeCharactersIn) {
						typingCharacters.Add(new CharacterFadeData(typedCharacter));
					}
					else {
						completedCharacters += typedCharacter;
					}

					if (skipTypingWhitespace && Char.IsWhiteSpace(typedCharacter)) {
						continue;
					}
					if (skipTypingPunctuation && Char.IsPunctuation(typedCharacter)) {
						continue;
					}

					charactersToTypeCounter -= 1f;
				}

				string fadingCharacters = UpdateFadingCharacters(ref typingCharacters, ref completedCharacters, deltaTime * characterFadeRate, colorClosingTag);

				_textRenderer.text = completedCharacters + fadingCharacters;
				if (alignHiddenCharacters) {
					_textRenderer.text += hiddenOpeningTag + scriptToTypeOut.Substring(scriptTypingIndex) + colorClosingTag;
				}

				yield return null;
			}

			onTextTypingComplete.Invoke(this);
		}

		// Returns true if we have a backslash preceding the index that would make it escaped. Correctly handles double/triple/etc. backslashes.
		private bool IsIndexEscaped(string inputString, int index) {
			if (index <= 0) {
				return false;
			}

			// Replace rather than remove so index is still valid
			return inputString.Replace("\\\\", "  ")[index - 1] == '\\';
		}

		private bool TryExtractRichTextTag(out int richTextEnd, out string richTextTag, string inputString, int startIndex) {
			richTextEnd = -1;
			richTextTag = "";
			for (int i = startIndex; i < inputString.Length; i++) {
				if (inputString[i] == '>' && !IsIndexEscaped(inputString, i)) {
					richTextEnd = i + 1;
					richTextTag = inputString.Substring(startIndex, richTextEnd - startIndex);
					return true;
				}
			}

			return false;
		}

		private string UpdateFadingCharacters(ref List<CharacterFadeData> typingCharacters, ref string completedCharacters, float fadeIncrease, string colorClosingTag) {
			if (!fadeCharactersIn) {
				return "";
			}

			Color baseColor = _textRenderer.color;

			string formattedFadingCharacters = "";
			int lastIndexToRemove = -1;
			for (int i = 0; i < typingCharacters.Count; i++) {
				typingCharacters[i].fadePercent += fadeIncrease;
				if (typingCharacters[i].fadePercent >= 1f) {
					completedCharacters += typingCharacters[i].character;
					lastIndexToRemove = i;
				}
				else {
					if (useFadeAnimCurve) {
						baseColor.a = fadeAnimCurve.Evaluate(typingCharacters[i].fadePercent);
					}
					else {
						baseColor.a = typingCharacters[i].fadePercent;
					}

					if (typingCharacters[i].isRichTextTag) {
						formattedFadingCharacters += typingCharacters[i].character;
					}
					else {
						formattedFadingCharacters += "<color=#" + ColorUtility.ToHtmlStringRGBA(baseColor) + ">" + typingCharacters[i].character + colorClosingTag;
					}
				}
			}

			if (lastIndexToRemove != -1) {
				typingCharacters.RemoveRange(0, lastIndexToRemove + 1);
			}

			return formattedFadingCharacters;
		}
	}
}

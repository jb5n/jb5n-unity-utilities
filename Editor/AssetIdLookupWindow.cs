using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace jb5n.editor {
	/// <summary>
	/// Editor window that resolves an asset id back to the asset it points at. Handles Unity GUIDs,
	/// pasted serialized references, instance ids, and Mirror's uint asset ids (which are a hash of the
	/// prefab GUID and therefore only resolvable by hashing every candidate GUID and matching).
	/// The reverse direction is supported too: drop an asset in to read all of its ids.
	/// </summary>
	public sealed class AssetIdLookupWindow : EditorWindow {
		private const string SearchControlName = "AssetIdLookupField";

		private static readonly Regex GuidPattern = new Regex("[0-9a-fA-F]{32}", RegexOptions.Compiled);
		private static readonly Regex FileIdPattern = new Regex(@"fileID:\s*(-?\d+)", RegexOptions.Compiled);
		private static readonly Regex TrailingLocalIdPattern = new Regex(@"[/\\](-?\d+)\s*$", RegexOptions.Compiled);
		private static readonly Regex AssetIdFieldPattern = new Regex(@"_?assetId:\s*(\d+)", RegexOptions.Compiled);

		private string _searchInput = string.Empty;
		private string _status = string.Empty;
		private MessageType _statusType = MessageType.None;
		private string _resolvedPath = string.Empty;
		private string _resolvedGuid = string.Empty;
		private string _resolvedMirrorAssetId = string.Empty;
		private Object _resolvedAsset;
		private readonly List<string> _extraMatches = new List<string>();
		private Object _reverseAsset;
		private Vector2 _scroll;

		[MenuItem("Tools/jb5n/Asset Id Lookup %#a")]
		private static void Open() {
			AssetIdLookupWindow window = GetWindow<AssetIdLookupWindow>();
			window.titleContent = new GUIContent("Asset Id Lookup");
			window.minSize = new Vector2(360f, 260f);
			window.Show();
		}

		private void OnGUI() {
			_scroll = EditorGUILayout.BeginScrollView(_scroll);

			EditorGUILayout.LabelField("Find Asset By Id", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox(
				"Accepts a Unity GUID, a pasted reference such as {fileID: 2100000, guid: abc..., type: 3}, " +
				"a \"guid/localFileId\" pair, a Mirror uint asset id such as 3032245156, or an instance id.",
				MessageType.None);

			GUI.SetNextControlName(SearchControlName);
			_searchInput = EditorGUILayout.TextField("Asset Id", _searchInput);

			bool submitted = Event.current.type == EventType.KeyDown
				&& (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
				&& GUI.GetNameOfFocusedControl() == SearchControlName;

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Look Up") || submitted) {
				if (submitted) {
					Event.current.Use();
				}

				Lookup(_searchInput);
			}

			if (GUILayout.Button("Paste & Look Up")) {
				_searchInput = EditorGUIUtility.systemCopyBuffer;
				GUI.FocusControl(null);
				Lookup(_searchInput);
			}

			if (GUILayout.Button("Clear")) {
				_searchInput = string.Empty;
				ClearResult();
				GUI.FocusControl(null);
			}

			EditorGUILayout.EndHorizontal();

			if (!string.IsNullOrEmpty(_status)) {
				EditorGUILayout.HelpBox(_status, _statusType);
			}

			if (_resolvedAsset != null || !string.IsNullOrEmpty(_resolvedPath)) {
				DrawResult();
			}

			EditorGUILayout.Space();
			DrawReverseLookup();

			EditorGUILayout.EndScrollView();
		}

		private void DrawResult() {
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Result", EditorStyles.boldLabel);

			using (new EditorGUI.DisabledScope(true)) {
				EditorGUILayout.ObjectField("Asset", _resolvedAsset, typeof(Object), false);
				EditorGUILayout.TextField("Path", _resolvedPath);
				EditorGUILayout.TextField("Type", _resolvedAsset != null ? _resolvedAsset.GetType().Name : "<unloaded>");
				EditorGUILayout.TextField("GUID", _resolvedGuid);
				EditorGUILayout.TextField("Mirror Asset Id", _resolvedMirrorAssetId);
			}

			EditorGUILayout.BeginHorizontal();

			using (new EditorGUI.DisabledScope(_resolvedAsset == null)) {
				if (GUILayout.Button("Ping")) {
					EditorGUIUtility.PingObject(_resolvedAsset);
				}

				if (GUILayout.Button("Select")) {
					Selection.activeObject = _resolvedAsset;
					EditorGUIUtility.PingObject(_resolvedAsset);
				}

				if (GUILayout.Button("Open")) {
					AssetDatabase.OpenAsset(_resolvedAsset);
				}
			}

			using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_resolvedPath))) {
				if (GUILayout.Button("Copy Path")) {
					EditorGUIUtility.systemCopyBuffer = _resolvedPath;
				}
			}

			EditorGUILayout.EndHorizontal();

			if (_extraMatches.Count == 0) {
				return;
			}

			EditorGUILayout.HelpBox("More than one asset hashes to this id. Other matches:", MessageType.Warning);
			foreach (string extraPath in _extraMatches) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(extraPath, EditorStyles.miniLabel);
				if (GUILayout.Button("Ping", EditorStyles.miniButton, GUILayout.Width(48f))) {
					EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(extraPath));
				}

				EditorGUILayout.EndHorizontal();
			}
		}

		private void DrawReverseLookup() {
			EditorGUILayout.LabelField("Get Ids From Asset", EditorStyles.boldLabel);
			_reverseAsset = EditorGUILayout.ObjectField("Asset", _reverseAsset, typeof(Object), true);

			if (_reverseAsset == null) {
				return;
			}

			if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(_reverseAsset, out string guid, out long localId)) {
				EditorGUILayout.HelpBox("That object is not a saved asset (scene instances have no asset GUID).", MessageType.Warning);
				return;
			}

			bool hasMirrorId = MirrorAssetIds.TryConvert(guid, out uint mirrorAssetId);

			using (new EditorGUI.DisabledScope(true)) {
				EditorGUILayout.TextField("GUID", guid);
				EditorGUILayout.TextField("Local File Id", localId.ToString());
				EditorGUILayout.TextField("Mirror Asset Id", hasMirrorId ? mirrorAssetId.ToString() : "<n/a>");
				EditorGUILayout.TextField("Path", AssetDatabase.GetAssetPath(_reverseAsset));
			}

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Copy GUID")) {
				EditorGUIUtility.systemCopyBuffer = guid;
			}

			if (GUILayout.Button("Copy GUID/LocalId")) {
				EditorGUIUtility.systemCopyBuffer = guid + "/" + localId;
			}

			using (new EditorGUI.DisabledScope(!hasMirrorId)) {
				if (GUILayout.Button("Copy Mirror Id")) {
					EditorGUIUtility.systemCopyBuffer = mirrorAssetId.ToString();
				}
			}

			EditorGUILayout.EndHorizontal();
		}

		private void Lookup(string rawInput) {
			ClearResult();

			string input = rawInput != null ? rawInput.Trim() : string.Empty;
			if (string.IsNullOrEmpty(input)) {
				SetStatus("Enter an asset id to look up.", MessageType.Info);
				return;
			}

			Match guidMatch = GuidPattern.Match(input);
			if (guidMatch.Success) {
				LookupByGuid(guidMatch.Value, input);
				return;
			}

			if (TryParseNumericId(input, out uint numericId, out int signedId, out bool fitsSignedInt)) {
				LookupByNumericId(numericId, signedId, fitsSignedInt);
				return;
			}

			SetStatus("Could not find a 32-character GUID or a numeric id in that input.", MessageType.Error);
		}

		private void LookupByGuid(string guid, string fullInput) {
			string path = AssetDatabase.GUIDToAssetPath(guid);
			if (string.IsNullOrEmpty(path)) {
				SetStatus("No asset in this project has GUID " + guid + ".", MessageType.Warning);
				return;
			}

			SetResolved(path, AssetDatabase.LoadMainAssetAtPath(path));

			long localId = ParseLocalFileId(fullInput);
			if (localId != 0L) {
				Object subAsset = FindSubAsset(path, localId);
				if (subAsset != null) {
					_resolvedAsset = subAsset;
					SetStatus("Resolved sub-asset \"" + subAsset.name + "\" (fileID " + localId + ").", MessageType.Info);
					return;
				}

				SetStatus("Found the asset, but no object inside it has fileID " + localId + ". Showing the main asset.",
					MessageType.Warning);
				return;
			}

			SetStatus(_resolvedAsset != null
				? "Resolved to " + path
				: "GUID maps to \"" + path + "\", but the asset could not be loaded.",
				_resolvedAsset != null ? MessageType.Info : MessageType.Warning);
		}

		/// <summary>
		/// A bare number is ambiguous. Mirror asset ids are by far the most common thing worth looking up, so
		/// try those first (in both their uint and signed-hash spellings) and only fall back to instance ids.
		/// </summary>
		private void LookupByNumericId(uint numericId, int signedId, bool fitsSignedInt) {
			List<string> matches = FindAssetsWithMirrorAssetId(numericId);
			if (matches.Count > 0) {
				SetResolved(matches[0], AssetDatabase.LoadMainAssetAtPath(matches[0]));
				for (int index = 1; index < matches.Count; index++) {
					_extraMatches.Add(matches[index]);
				}

				SetStatus("Mirror asset id " + numericId + " → " + matches[0]
					+ (MirrorAssetIds.UsingMirrorImplementation
						? string.Empty
						: "\n(Mirror not found in this project; used the equivalent Guid.GetHashCode mapping.)"),
					MessageType.Info);
				return;
			}

			if (fitsSignedInt) {
				Object asset = EditorUtility.EntityIdToObject(signedId);
				if (asset != null) {
					SetResolved(AssetDatabase.GetAssetPath(asset), asset);
					SetStatus(string.IsNullOrEmpty(_resolvedPath)
						? "Resolved instance id " + signedId + " to a scene or in-memory object."
						: "Resolved instance id " + signedId + " to " + _resolvedPath, MessageType.Info);
					return;
				}
			}

			SetStatus("No asset GUID in this project hashes to Mirror asset id " + numericId
				+ ", and it does not match any loaded instance id."
				+ "\nIf the asset lives in a package or was deleted, its GUID is no longer in the AssetDatabase.",
				MessageType.Warning);
		}

		/// <summary>
		/// Mirror asset ids are one-way (a hash of the prefab GUID), so the only way back is to hash every
		/// candidate GUID and compare. Prefabs are checked first since that is what Mirror assigns ids to.
		/// </summary>
		private static List<string> FindAssetsWithMirrorAssetId(uint assetId) {
			List<string> matches = new List<string>();

			foreach (string guid in AssetDatabase.FindAssets("t:Prefab")) {
				if (MirrorAssetIds.TryConvert(guid, out uint candidate) && candidate == assetId) {
					matches.Add(AssetDatabase.GUIDToAssetPath(guid));
				}
			}

			if (matches.Count > 0) {
				return matches;
			}

			foreach (string path in AssetDatabase.GetAllAssetPaths()) {
				string guid = AssetDatabase.AssetPathToGUID(path);
				if (MirrorAssetIds.TryConvert(guid, out uint candidate) && candidate == assetId) {
					matches.Add(path);
				}
			}

			return matches;
		}

		/// <summary>
		/// Accepts "3032245156", "0xB4C3D5E4", a negative signed spelling of the same hash, or a pasted
		/// "_assetId: 3032245156" line straight out of a prefab's YAML.
		/// </summary>
		private static bool TryParseNumericId(string input, out uint numericId, out int signedId, out bool fitsSignedInt) {
			numericId = 0u;
			signedId = 0;
			fitsSignedInt = false;

			string candidate = input;
			Match assetIdField = AssetIdFieldPattern.Match(input);
			if (assetIdField.Success) {
				candidate = assetIdField.Groups[1].Value;
			}

			candidate = candidate.Trim();
			if (candidate.EndsWith("u", System.StringComparison.OrdinalIgnoreCase)) {
				candidate = candidate.Substring(0, candidate.Length - 1);
			}

			bool parsed;
			if (candidate.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase)) {
				parsed = uint.TryParse(candidate.Substring(2), System.Globalization.NumberStyles.HexNumber,
					System.Globalization.CultureInfo.InvariantCulture, out numericId);
			}
			else if (uint.TryParse(candidate, out numericId)) {
				parsed = true;
			}
			else if (int.TryParse(candidate, out int negative)) {
				// GetHashCode returns a signed int; accept that spelling of the same bits.
				numericId = unchecked((uint)negative);
				parsed = true;
			}
			else {
				parsed = false;
			}

			if (!parsed) {
				return false;
			}

			fitsSignedInt = numericId <= int.MaxValue;
			signedId = fitsSignedInt ? (int)numericId : 0;
			return true;
		}

		private static long ParseLocalFileId(string input) {
			Match fileIdMatch = FileIdPattern.Match(input);
			if (fileIdMatch.Success && long.TryParse(fileIdMatch.Groups[1].Value, out long fromBlob)) {
				return fromBlob;
			}

			Match trailingMatch = TrailingLocalIdPattern.Match(input);
			if (trailingMatch.Success && long.TryParse(trailingMatch.Groups[1].Value, out long fromPair)) {
				return fromPair;
			}

			return 0L;
		}

		private static Object FindSubAsset(string path, long localId) {
			// LoadAllAssetsAtPath is not valid for scenes; a scene has no sub-assets to walk anyway.
			if (path.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase)) {
				return null;
			}

			IEnumerable<Object> candidates = AssetDatabase.LoadAllAssetsAtPath(path);
			foreach (Object candidate in candidates) {
				if (candidate == null) {
					continue;
				}

				if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(candidate, out string _, out long candidateId)
					&& candidateId == localId) {
					return candidate;
				}
			}

			return null;
		}

		private void SetResolved(string path, Object asset) {
			_resolvedPath = path ?? string.Empty;
			_resolvedAsset = asset;
			_resolvedGuid = string.IsNullOrEmpty(_resolvedPath) ? string.Empty : AssetDatabase.AssetPathToGUID(_resolvedPath);
			_resolvedMirrorAssetId = MirrorAssetIds.TryConvert(_resolvedGuid, out uint mirrorAssetId)
				? mirrorAssetId.ToString()
				: string.Empty;
		}

		private void SetStatus(string message, MessageType type) {
			_status = message;
			_statusType = type;
		}

		private void ClearResult() {
			_status = string.Empty;
			_statusType = MessageType.None;
			_resolvedPath = string.Empty;
			_resolvedGuid = string.Empty;
			_resolvedMirrorAssetId = string.Empty;
			_resolvedAsset = null;
			_extraMatches.Clear();
		}
	}

	/// <summary>
	/// Maps a Unity GUID to Mirror's uint asset id. Mirror exposes NetworkIdentity.AssetGuidToUint publicly
	/// specifically so callers do not hardcode the mapping, so bind to it by reflection when it is present.
	/// That keeps this package usable in projects that do not have Mirror, where the documented equivalent
	/// -- (uint)guid.GetHashCode() -- is used instead.
	/// </summary>
	internal static class MirrorAssetIds {
		private static readonly Regex ExactGuidPattern = new Regex("^[0-9a-fA-F]{32}$", RegexOptions.Compiled);

		private static System.Func<System.Guid, uint> _converter;
		private static bool _usingMirrorImplementation;

		public static bool UsingMirrorImplementation {
			get {
				EnsureConverter();
				return _usingMirrorImplementation;
			}
		}

		public static bool TryConvert(string unityGuid, out uint assetId) {
			assetId = 0u;
			if (string.IsNullOrEmpty(unityGuid) || !ExactGuidPattern.IsMatch(unityGuid)) {
				return false;
			}

			EnsureConverter();
			assetId = _converter(new System.Guid(unityGuid));
			return true;
		}

		private static void EnsureConverter() {
			if (_converter != null) {
				return;
			}

			MethodInfo method = ResolveMirrorMethod();
			if (method != null) {
				_converter = (System.Func<System.Guid, uint>)System.Delegate.CreateDelegate(
					typeof(System.Func<System.Guid, uint>), method);
				_usingMirrorImplementation = true;
				return;
			}

			_converter = guid => unchecked((uint)guid.GetHashCode());
			_usingMirrorImplementation = false;
		}

		private static MethodInfo ResolveMirrorMethod() {
			System.Type identityType = System.Type.GetType("Mirror.NetworkIdentity, Mirror");
			if (identityType == null) {
				foreach (Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
					identityType = assembly.GetType("Mirror.NetworkIdentity", false);
					if (identityType != null) {
						break;
					}
				}
			}

			if (identityType == null) {
				return null;
			}

			MethodInfo method = identityType.GetMethod(
				"AssetGuidToUint",
				BindingFlags.Public | BindingFlags.Static,
				null,
				new[] { typeof(System.Guid) },
				null);

			return method != null && method.ReturnType == typeof(uint) ? method : null;
		}
	}
}

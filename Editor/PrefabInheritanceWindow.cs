using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace jb5n.editor {
	// Shows a prefab's variant chain up to its base and the variants derived from it, as a top-down graph.
	public class PrefabInheritanceWindow : EditorWindow {
		public GameObject target;
		public int parentDepth = 10;
		public int childDepth = 3;
		public float zoom = 1f;

		private const float MinZoom = 0.25f;
		private const float MaxZoom = 2.5f;

		private float NodeWidth => 100f * zoom;
		private float NodeHeight => 116f * zoom;
		private float GapX => 16f * zoom;
		private float GapY => 36f * zoom;
		private const float Padding = 24f;

		private class Node {
			public GameObject go;
			public string path;
			public List<Node> children = new();
			public int hiddenChildren;
			public float column;
			public Rect rect;
		}

		// prefab path -> prefabs that directly reference it (variants and nested users)
		private Dictionary<string, List<string>> dependents;
		private readonly Dictionary<string, List<string>> variants = new();
		private Vector2 scroll;
		private GUIStyle labelStyle;

		[MenuItem("Assets/Prefab Inheritance Tree", false, 2000)]
		private static void OpenFromAssets() {
			Open(Selection.activeObject as GameObject);
		}

		[MenuItem("Assets/Prefab Inheritance Tree", true)]
		private static bool ValidateOpenFromAssets() {
			return Selection.activeObject is GameObject go && PrefabUtility.IsPartOfPrefabAsset(go);
		}

		[MenuItem("Tools/jb5n/Prefab Inheritance Tree")]
		private static void OpenFromTools() {
			Open(Selection.activeObject as GameObject);
		}

		private static void Open(GameObject go) {
			PrefabInheritanceWindow window = GetWindow<PrefabInheritanceWindow>();
			window.titleContent = new GUIContent("Prefab Inheritance");
			window.SetTarget(go);
			window.Show();
		}

		private void OnEnable() {
			EditorApplication.projectChanged += ClearCache;
			Selection.selectionChanged += Repaint;
			// Default cache of 50 makes previews flicker on big graphs
			AssetPreview.SetPreviewTextureCacheSize(256);
		}

		private void OnDisable() {
			EditorApplication.projectChanged -= ClearCache;
			Selection.selectionChanged -= Repaint;
		}

		private void SetTarget(GameObject go) {
			if (go != null && !PrefabUtility.IsPartOfPrefabAsset(go)) {
				// Scene instance: use the prefab it came from
				GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
				go = root != null ? PrefabUtility.GetCorrespondingObjectFromSource(root) : null;
			}
			target = go != null ? go.transform.root.gameObject : null;
			scroll = Vector2.zero;
		}

		private void ClearCache() {
			dependents = null;
			variants.Clear();
			Repaint();
		}

		private void OnGUI() {
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			GameObject newTarget = (GameObject)EditorGUILayout.ObjectField(target, typeof(GameObject), false, GUILayout.Width(220));
			if (newTarget != target) {
				SetTarget(newTarget);
			}
			EditorGUIUtility.labelWidth = 80;
			parentDepth = Mathf.Max(0, EditorGUILayout.IntField("Parent Depth", parentDepth, GUILayout.Width(120)));
			childDepth = Mathf.Max(0, EditorGUILayout.IntField("Child Depth", childDepth, GUILayout.Width(120)));
			EditorGUIUtility.labelWidth = 0;
			GUILayout.FlexibleSpace();
			zoom = GUILayout.HorizontalSlider(zoom, MinZoom, MaxZoom, GUILayout.Width(80));
			if (GUILayout.Button($"{zoom * 100:0}%", EditorStyles.toolbarButton, GUILayout.Width(44))) {
				zoom = 1f;
			}
			if (GUILayout.Button("Refresh", EditorStyles.toolbarButton)) {
				ClearCache();
			}
			EditorGUILayout.EndHorizontal();

			if (target == null) {
				EditorGUILayout.HelpBox("Pick a prefab.", MessageType.Info);
				return;
			}

			List<Node> parents = new();
			GameObject current = target;
			for (int i = 0; i < parentDepth; i++) {
				GameObject parent = PrefabUtility.GetCorrespondingObjectFromSource(current);
				if (parent == null) {
					break;
				}
				parents.Insert(0, new Node { go = parent, path = AssetDatabase.GetAssetPath(parent) });
				current = parent;
			}
			bool moreParents = PrefabUtility.GetCorrespondingObjectFromSource(current) != null;

			Node root = BuildTree(target, AssetDatabase.GetAssetPath(target), 0);
			float leafX = 0;
			int rows = 0;
			Layout(root, parents.Count, ref leafX, ref rows);
			for (int i = 0; i < parents.Count; i++) {
				parents[i].rect = new Rect(root.rect.x, RowY(i), NodeWidth, NodeHeight);
			}

			Rect view = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
			float contentWidth = leafX * (NodeWidth + GapX) - GapX + Padding * 2;
			float contentHeight = RowY(rows + 1) - GapY + Padding;
			float offsetX = Mathf.Max(0, (view.width - contentWidth) / 2);

			// Wheel zooms around the cursor
			Event wheel = Event.current;
			if (wheel.type == EventType.ScrollWheel && view.Contains(wheel.mousePosition)) {
				float newZoom = Mathf.Clamp(zoom * (wheel.delta.y > 0 ? 0.9f : 1.1f), MinZoom, MaxZoom);
				Vector2 mouse = wheel.mousePosition - view.position;
				scroll = (scroll + mouse) * (newZoom / zoom) - mouse;
				zoom = newZoom;
				wheel.Use();
				Repaint();
			}

			EditorGUI.DrawRect(view, EditorGUIUtility.isProSkin ? new Color(0.16f, 0.16f, 0.16f) : new Color(0.7f, 0.7f, 0.7f));
			scroll = GUI.BeginScrollView(view, scroll, new Rect(0, 0, contentWidth, contentHeight));
			GUI.BeginGroup(new Rect(offsetX, 0, contentWidth, contentHeight));

			if (moreParents) {
				GUI.Label(new Rect(root.rect.x, 0, NodeWidth, Padding), "...", labelStyle);
			}
			for (int i = 0; i < parents.Count; i++) {
				DrawEdge(parents[i].rect, i + 1 < parents.Count ? parents[i + 1].rect : root.rect);
			}
			DrawEdges(root);
			foreach (Node parent in parents) {
				DrawNode(parent);
			}
			DrawNodes(root);

			GUI.EndGroup();
			GUI.EndScrollView();

			// Drag empty space to pan
			if (Event.current.type == EventType.MouseDrag && view.Contains(Event.current.mousePosition)) {
				scroll -= Event.current.delta;
				Event.current.Use();
				Repaint();
			}
			if (AssetPreview.IsLoadingAssetPreviews()) {
				Repaint();
			}
		}

		private float RowY(int row) {
			return Padding + row * (NodeHeight + GapY);
		}

		private Node BuildTree(GameObject go, string path, int depth) {
			Node node = new() { go = go, path = path };
			List<string> children = GetVariants(path);
			if (depth >= childDepth) {
				node.hiddenChildren = children.Count;
				return node;
			}
			foreach (string child in children) {
				node.children.Add(BuildTree(AssetDatabase.LoadAssetAtPath<GameObject>(child), child, depth + 1));
			}
			return node;
		}

		// Leaves take consecutive columns, parents center over their children
		private void Layout(Node node, int row, ref float leafX, ref int rows) {
			rows = Mathf.Max(rows, node.hiddenChildren > 0 ? row + 1 : row);
			if (node.children.Count == 0) {
				node.column = leafX;
				leafX++;
			}
			else {
				foreach (Node child in node.children) {
					Layout(child, row + 1, ref leafX, ref rows);
				}
				node.column = (node.children[0].column + node.children[node.children.Count - 1].column) / 2;
			}
			node.rect = new Rect(Padding + node.column * (NodeWidth + GapX), RowY(row), NodeWidth, NodeHeight);
		}

		private void DrawEdges(Node node) {
			foreach (Node child in node.children) {
				DrawEdge(node.rect, child.rect);
				DrawEdges(child);
			}
			if (node.hiddenChildren > 0) {
				DrawEdge(node.rect, HiddenRect(node));
			}
		}

		private void DrawEdge(Rect from, Rect to) {
			if (Event.current.type != EventType.Repaint) {
				return;
			}
			float midY = from.yMax + GapY / 2;
			Handles.color = EditorGUIUtility.isProSkin ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.3f, 0.3f, 0.3f);
			Handles.DrawAAPolyLine(2f,
				new Vector3(from.center.x, from.yMax), new Vector3(from.center.x, midY),
				new Vector3(to.center.x, midY), new Vector3(to.center.x, to.y));
		}

		private void DrawNodes(Node node) {
			DrawNode(node);
			foreach (Node child in node.children) {
				DrawNodes(child);
			}
			if (node.hiddenChildren > 0) {
				Rect rect = HiddenRect(node);
				if (GUI.Button(rect, $"+{node.hiddenChildren} more", EditorStyles.miniButton)) {
					target = node.go;
				}
			}
		}

		private Rect HiddenRect(Node node) {
			return new Rect(node.rect.center.x - 40, node.rect.yMax + GapY, 80, 20);
		}

		private void DrawNode(Node node) {
			if (labelStyle == null) {
				labelStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperCenter, wordWrap = true };
			}
			labelStyle.fontSize = Mathf.RoundToInt(9 * zoom);

			Rect rect = node.rect;
			Event e = Event.current;
			if (e.type == EventType.Repaint) {
				Color fill = EditorGUIUtility.isProSkin ? new Color(0.24f, 0.24f, 0.24f) : new Color(0.85f, 0.85f, 0.85f);
				Color outline = node.go == target ? new Color(0.25f, 0.6f, 1f)
					: Selection.activeObject == node.go ? new Color(0.8f, 0.8f, 0.8f)
					: new Color(0.1f, 0.1f, 0.1f);
				Handles.DrawSolidRectangleWithOutline(rect, fill, outline);
				if (node.go == target) {
					Handles.DrawSolidRectangleWithOutline(new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2), Color.clear, outline);
				}

				Texture thumb = AssetPreview.GetAssetPreview(node.go);
				if (thumb == null) {
					thumb = AssetPreview.GetMiniThumbnail(node.go);
				}
				GUI.DrawTexture(new Rect(rect.x + 10 * zoom, rect.y + 6 * zoom, rect.width - 20 * zoom, rect.width - 20 * zoom), thumb, ScaleMode.ScaleToFit);
			}
			// Text gets unreadable below half size, so leave just the thumbnails
			if (zoom >= 0.5f) {
				GUI.Label(new Rect(rect.x + 4, rect.y + rect.width - 10 * zoom, rect.width - 8, rect.yMax - (rect.y + rect.width - 10 * zoom)),
					new GUIContent(node.go.name, node.path), labelStyle);
			}

			if (e.type != EventType.MouseDown || !rect.Contains(e.mousePosition)) {
				return;
			}
			if (e.button == 0 && e.clickCount == 2) {
				AssetDatabase.OpenAsset(node.go);
			}
			else if (e.button == 0) {
				Selection.activeObject = node.go;
				EditorGUIUtility.PingObject(node.go);
			}
			else if (e.button == 1) {
				GenericMenu menu = new();
				menu.AddItem(new GUIContent("Focus"), false, () => target = node.go);
				menu.AddItem(new GUIContent("Open"), false, () => AssetDatabase.OpenAsset(node.go));
				menu.AddItem(new GUIContent("Ping"), false, () => EditorGUIUtility.PingObject(node.go));
				menu.ShowAsContext();
			}
			e.Use();
		}

		private List<string> GetVariants(string path) {
			if (variants.TryGetValue(path, out List<string> result)) {
				return result;
			}
			if (dependents == null) {
				BuildDependents();
			}

			// Dependents also include prefabs that merely nest this one, so confirm each is a direct variant
			result = new();
			if (dependents.TryGetValue(path, out List<string> candidates)) {
				foreach (string candidate in candidates) {
					GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(candidate);
					if (go != null && PrefabUtility.GetPrefabAssetType(go) == PrefabAssetType.Variant
						&& AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(go)) == path) {
						result.Add(candidate);
					}
				}
			}
			result.Sort();
			variants[path] = result;
			return result;
		}

		private void BuildDependents() {
			dependents = new();
			string[] guids = AssetDatabase.FindAssets("t:Prefab");
			try {
				for (int i = 0; i < guids.Length; i++) {
					string path = AssetDatabase.GUIDToAssetPath(guids[i]);
					if (i % 100 == 0) {
						EditorUtility.DisplayProgressBar("Prefab Inheritance", "Scanning prefabs", (float)i / guids.Length);
					}
					foreach (string dependency in AssetDatabase.GetDependencies(path, false)) {
						if (!dependents.TryGetValue(dependency, out List<string> list)) {
							list = new();
							dependents[dependency] = list;
						}
						list.Add(path);
					}
				}
			}
			finally {
				EditorUtility.ClearProgressBar();
			}
		}
	}
}

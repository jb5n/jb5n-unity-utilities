using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace jb5n {
	public abstract class CameraControl : Singleton<CameraControl> {

		public delegate void OnZoom(float newOrthoSize);
		public OnZoom onZoom;

		public float minZoom;
		public float maxZoom;
		[Space]
		public float zoomScrollRate;
		public float zoomLerpRate;
		public bool keepCursorPositionOnZoomIn = true;
		public bool keepCursorPositionOnZoomOut = false;
		[Space]
		public float autofocusPanLerpRate;
		public float autofocusZoomLerpRate;
		public float autofocusBoundsExtension;

		private Camera _mainCam;

		private Vector2 panMousePos;
		private float targetZoom;

		private bool autoFocusing = false;
		private Vector3 autoFocusTarget;

		private void OnEnable() {
			//inputActions = new CameraInputActions();
			//inputActions.Camera.Enable();
		}

		private void OnDisable() {
			//inputActions.Camera.Disable();
		}

		protected override void Awake() {
			base.Awake();
			_mainCam = Camera.main;
			targetZoom = _mainCam.orthographicSize;
		}

		public void SetTargetZoom(float zoom) {
			targetZoom = zoom;
		}

		void LateUpdate() {
			HandlePan();
			HandleZoom();
		}

		private void HandlePan() {
			Vector3 curMousePos = Mouse.current.position.ReadValue();

			if (WasPanPerformedThisFrame()) {
				panMousePos = _mainCam.ScreenToWorldPoint(curMousePos);
			}
			else if (IsPanPressed()) {
				Vector3 panDelta = panMousePos - (Vector2)_mainCam.ScreenToWorldPoint(curMousePos);
				_mainCam.transform.position += panDelta;
				panMousePos = _mainCam.ScreenToWorldPoint(curMousePos);
				autoFocusing = false;
			}

			if (autoFocusing) {
				_mainCam.transform.position = Vector3.Lerp(_mainCam.transform.position, autoFocusTarget, autofocusPanLerpRate * Time.deltaTime * 60f);
			}
		}

		private void HandleZoom() {
			float scrollDelta = GetCameraZoom();
			if (scrollDelta != 0f) {
				autoFocusing = false;
			}
			float deltaZoom = -scrollDelta * zoomScrollRate;
			targetZoom = Mathf.Clamp(targetZoom + deltaZoom, minZoom, maxZoom);

			Vector3 screenMousePos = Mouse.current.position.ReadValue();
			Vector2 mouseWorldPositionBeforeZoom = _mainCam.ScreenToWorldPoint(screenMousePos);

			float curOrthoSize = _mainCam.orthographicSize;
			float newOrthoSize = Mathf.Lerp(curOrthoSize, targetZoom,
				(autoFocusing ? autofocusZoomLerpRate : zoomLerpRate) * Time.deltaTime * 60f);
			bool shouldAdjustCamPos = (newOrthoSize > curOrthoSize && keepCursorPositionOnZoomOut) ||
				(newOrthoSize < curOrthoSize && keepCursorPositionOnZoomIn);

			_mainCam.orthographicSize = newOrthoSize;
			Vector2 mouseWorldPositionAfterZoom = _mainCam.ScreenToWorldPoint(screenMousePos);

			if (shouldAdjustCamPos) {
				Vector2 offset = mouseWorldPositionBeforeZoom - mouseWorldPositionAfterZoom;
				_mainCam.transform.position += (Vector3)offset;
			}

			if (onZoom != null) {
				onZoom.Invoke(_mainCam.orthographicSize);
			}
		}

		public void AutofocusOnTarget(Bounds target) {
			autoFocusing = true;
			autoFocusTarget = target.center;
			autoFocusTarget.z = _mainCam.transform.position.z;

			float targetHeightHalf = target.extents.y;
			float targetWidthHalf = target.extents.x;
			float unboundedZoom = Mathf.Max(targetHeightHalf, targetWidthHalf / _mainCam.aspect) + autofocusBoundsExtension;
			targetZoom = Mathf.Clamp(unboundedZoom, minZoom, maxZoom);
		}

		// Abstract input functions
		public abstract bool WasPanPerformedThisFrame(); // Typically resolve using inputActions.<input>.WasPerformedThisFrame()
		public abstract bool IsPanPressed(); // Typically resolve using inputActions.<input>.IsPressed()
		public abstract float GetCameraZoom(); // inputActions.<scroll wheel>.ReadValue<Vector2>().y

	}
}
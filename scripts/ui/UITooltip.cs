using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UITooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(3, 10)]
    public string tooltipText;
    [Space]
    public RectTransform tooltipPrefab;
    public Vector2 mouseOffset;
    
    private Canvas _rootCanvas;
    protected RectTransform _tooltipObject;
    
    void Awake() {
        _rootCanvas = GetComponentInParent<Canvas>();
    }
	
	void OnDestroy() {
		DisableTooltip();
	}
	
	void OnDisable() {
		DisableTooltip();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_tooltipObject = Instantiate(tooltipPrefab, _rootCanvas.transform);
		OnTooltipOpen();
		StartCoroutine("WhilePointerOver");
	}
	
	protected virtual void OnTooltipOpen() {
		_tooltipObject.GetComponentInChildren<TMPro.TMP_Text>().text = tooltipText;
	}
    
    private IEnumerator WhilePointerOver() {
        while(true) {
            _tooltipObject.position = (Vector2)Input.mousePosition + mouseOffset;
			
			Vector3[] worldCorners = new Vector3[4];
			_tooltipObject.GetWorldCorners(worldCorners);
			Vector3 bottomLeft = worldCorners[0];
			Vector3 topRight = worldCorners[2];

			Vector3[] canvasWorldCorners = new Vector3[4];
			((RectTransform)_rootCanvas.transform).GetWorldCorners(canvasWorldCorners);
			Vector3 canvasTopRight = canvasWorldCorners[2];

			Vector2 boundsOffset = Vector2.zero;
			if (bottomLeft.x < 0) {
				boundsOffset.x = -bottomLeft.x;
			}
			else if(topRight.x > canvasTopRight.x) {
				boundsOffset.x = canvasTopRight.x - topRight.x;
			}
			
			if (bottomLeft.y < 0) {
				boundsOffset.y = -bottomLeft.y;
			}
			else if(topRight.y > canvasTopRight.y) {
				boundsOffset.y = canvasTopRight.y - topRight.y;
			}
			
			_tooltipObject.position += (Vector3)boundsOffset;
            yield return null;
        }
    }

	public void OnPointerExit(PointerEventData eventData) {
        DisableTooltip();
	}
	
	private void DisableTooltip() {
		StopCoroutine("WhilePointerOver");
		if(_tooltipObject != null) {
            Destroy(_tooltipObject.gameObject);
            _tooltipObject = null;
        }
	}
}

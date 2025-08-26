using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIImageTooltip : UITooltip
{
	public Sprite targetSprite;
	
    protected override void OnTooltipOpen() {
		_tooltipObject.GetChild(0).GetComponent<Image>().sprite = targetSprite;
	}
}

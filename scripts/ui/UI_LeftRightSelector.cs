using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// A UI element that has text in the middle flanked by two buttons. Use the buttons to cycle between options for the text
public class UI_LeftRightSelector : MonoBehaviour
{
    [Header("Settings")]
    public string[] selectionOptions;
    [Header("References")]
    public TMP_Text selectionText;
    public Button leftButton;
    public Button rightButton;
    
    private void LeftButtonPressed() {
        
    }
    
    private void RightButtonPressed() {
        
    }
}

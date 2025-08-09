using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

internal class UIManager : MonoBehaviour
{
    [SerializeField] Transform LoseUI;


    private void Awake()
    {
    }
    private void Start()
    {
        TurnOffAllUI();
        SetLoseUI();
        
    }

    private void SetLoseUI()
    {
        var btn = LoseUI.GetComponentInChildren<Button>();
        Debug.Log(btn);
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => GameManager.Instance.GameRestart());        
    }
    public void TurnOffAllUI()
    {
        LoseUI.gameObject.SetActive(false);
    }
    internal void ShowLoseUI()
    {
        LoseUI.gameObject.SetActive(true);
        //throw new NotImplementedException();
    }
}
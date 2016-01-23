using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

public class GameRule : MonoBehaviour {

    public RuleType ruleType;
    public CellAction clickResult;
    public CellAction cascadeEmptyResult;
    public CellAction cascadeOwnAreaResult;
    public CellAction cascadeEnemyAreaResult;
    public CellAction cascadeEnemyCoreResult;
    public bool continueEmptyCascade;
    public bool continueOwnAreaCascade;
    public bool continueEnemyAreaCascade;
    public bool continueEnemyCoreCascade;

    private Text optionLabel;
    private Dropdown ddlClickResult;
    private Dropdown ddlAdjacentEmpty;
    private Dropdown ddlAdjacentOwnArea;
    private Dropdown ddlAdjacentEnemyArea;
    private Dropdown ddlAdjacentEnemyCore;
    private Toggle togEmptyContinue;
    private Toggle togOwnAreaContinue;
    private Toggle togEnemyAreaContinue;
    private Toggle togEnemyCoreContinue;

	// Use this for initialization
	void Start () {
        optionLabel = transform.FindChild("OptionType").GetComponent<Text>();
        optionLabel.text = GetProperName(Enum.GetName(typeof(RuleType), ruleType));

        ddlClickResult = transform.FindChild("ResultsIn").GetComponent<Dropdown>();
        ddlClickResult.options = GetCellActionOptions();

        ddlAdjacentEmpty = transform.FindChild("AdjacentEmpty").GetComponent<Dropdown>();
        ddlAdjacentEmpty.options = GetCellActionOptions();

        ddlAdjacentOwnArea = transform.FindChild("Adjacent_MyArea").GetComponent<Dropdown>();
        ddlAdjacentOwnArea.options = GetCellActionOptions();

        ddlAdjacentEnemyArea = transform.FindChild("Adjacent_EnemyArea").GetComponent<Dropdown>();
        ddlAdjacentEnemyArea.options = GetCellActionOptions();

        ddlAdjacentEnemyCore = transform.FindChild("Adjacent_EnemyCore").GetComponent<Dropdown>();
        ddlAdjacentEnemyCore.options = GetCellActionOptions();

        togEmptyContinue = transform.FindChild("Empty_Continue").GetComponent<Toggle>();        
        togOwnAreaContinue = transform.FindChild("MyArea_Continue").GetComponent<Toggle>();
        togEnemyAreaContinue = transform.FindChild("EnemyArea_Continue").GetComponent<Toggle>();
        togEnemyCoreContinue = transform.FindChild("EnemyCore_Continue").GetComponent<Toggle>();
	}
	
    public void UpdateGameOptions()
    {
        clickResult = GetCellAction(ddlClickResult.options[ddlClickResult.value].text);
        cascadeEmptyResult = GetCellAction(ddlAdjacentEmpty.options[ddlAdjacentEmpty.value].text);
        cascadeOwnAreaResult = GetCellAction(ddlAdjacentOwnArea.options[ddlAdjacentOwnArea.value].text);
        cascadeEnemyAreaResult = GetCellAction(ddlAdjacentEnemyArea.options[ddlAdjacentEnemyArea.value].text);
        cascadeEnemyCoreResult = GetCellAction(ddlAdjacentEnemyCore.options[ddlAdjacentEnemyCore.value].text);
        continueEmptyCascade = togEmptyContinue.isOn;
        continueOwnAreaCascade = togOwnAreaContinue.isOn;
        continueEnemyAreaCascade = togEnemyAreaContinue.isOn;
        continueEnemyCoreCascade = togEnemyCoreContinue.isOn;
    }

    List<Dropdown.OptionData> GetCellActionOptions()
    {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

        foreach (string cellAction in Enum.GetNames(typeof(CellAction))){
            options.Add(new Dropdown.OptionData(GetProperName(cellAction)));
        }
        
        return options;
    }

    string GetProperName(string s)
    {
        return Regex.Replace(s, "(\\B[A-Z])", " $1");
    }

    CellAction GetCellAction(string cellAction)
    {
        return (CellAction)Enum.Parse(typeof(CellAction), cellAction.Replace(" ", ""));
    }
}

public enum RuleType
{
    EmptyCell,
    OwnArea,
    OwnCore,
    EnemyArea,
    EnemyCore
}

public enum CellAction
{
    Nothing,
    MakeEmpty,
    MakeOwnArea,
    MakeOwnCore,
    MakeEnemyArea,
    MakeEnemyCore
}

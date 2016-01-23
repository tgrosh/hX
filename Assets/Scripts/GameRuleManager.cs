using UnityEngine;
using System.Collections;

public class GameRuleManager {
    public static GameRuleManager singleton;

    public GameRule ruleEmpty;
    public GameRule ruleOwnArea;
    public GameRule ruleOwnCore;
    public GameRule ruleEnemyArea;
    public GameRule ruleEnemyCore;

    public GameRuleManager()
    {
        singleton = this;
	}
	
}

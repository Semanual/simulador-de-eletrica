using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalManager : MonoBehaviour {
    public static GoalManager Singleton;
    public bool isGoalReached = false;
    BaseGoal[] baseGoals;
    void Awake() {
        if (Singleton != null) {
            Debug.LogError("Há mais de um GeneratorManager nesta cena.");
            Destroy(gameObject);
            return;
        }

        Singleton = this;
        baseGoals = FindObjectsByType<BaseGoal>(FindObjectsSortMode.None);
    }

    public void CheckAllGoals() {
        if (isGoalReached) {
            return;
        }

        foreach (BaseGoal goal in baseGoals) {
            if (!goal.CheckGoal()) {
                return;
            }
        }

        isGoalReached = true;
        Invoke(nameof(NextStage), 2f);
    }

    public void NextStage() {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.buildIndex + 1 >= SceneManager.sceneCountInBuildSettings) {
            return;
        }
        SceneManager.LoadScene(currentScene.buildIndex + 1);
    }
}
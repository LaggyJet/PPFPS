//worked on by - natalie lubahn
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{

    public void resume()
    {
        GameManager.instance.stateResume();
       
    }
    public void restart()
    {
        GameManager.enemyCount = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        PlayerController.spawnHp = 10;
        GameManager.instance.playerScript.updatePlayerUI();
        GameManager.instance.stateResume();
    }
    public void quit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
    public void respawn()
    {
        GameManager.instance.respawnAfterLost();
    }
    public void jumpToggle()
    {
        GameManager.instance.canJump = !GameManager.instance.canJump;
    }    
    
    //FOR TITLE SCREEN
    public void newGame()
    {
        SceneManager.LoadScene("Build Scene");
        DataPersistenceManager.Instance.NewGame();
    }
    public void loadGame()
    {
        SceneManager.LoadScene("Build Scene");
        DataPersistenceManager.Instance.LoadGame();
    }
    public void startNewGame()
    {
        GameManager.instance.charSelectionMenu();
    }

    //CLASS SELECTION
    public void wretch()
    {
        newGame();
        GameManager.instance.stateResume();
    }
    public void mage()
    {
        newGame();
        GameManager.instance.stateResume();
    }
    public void archer()
    {
        newGame();
        GameManager.instance.stateResume();
    }
}

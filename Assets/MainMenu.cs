using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //to do list
    //needs:
    //-add a start button to the main menu to start the game
    //-add a quit button to the main menu to quit the game
    //-add a high score display

    //wants:
    //Buttons quickly build into a tower the camera follows into mock up of the game that serves as a scene transition
    //Add a scrolling background

    //If everything else in the project is done
    //Add a tower that builds itself in the background

    public Button startButton;
    public Button quitButton;
    public List<Score> scores = new List<Score>();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ReadScores()
    {
        //parse the scores from all score files and add them to the list of scores  and sort the list of scores display the scores
        //if there are no scores, display a message saying there are no scores
        

    }

    public void ParseScoresFromPersistentPath()
    {
        //for each file in the persistent path, parse the scores from the file and add them to the list of scores and sort the list of scores

    }


}

public struct Score
{
    public string name;
    public int score;
    private int level;

    public Score(int _score, int _level, string _name)
    {
        score = _score;
        level = _level;
        name = _name;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private List<Transform> tiles = new List<Transform>();
    [SerializeField] private GameObject activeTilePrefab;
    [SerializeField] private GameObject deactiveTilePrefab;
    [SerializeField] private float newRowSpeedMod = 0.05f;
    [SerializeField] private int _direction = 1;
    [SerializeField] private int _row;
    [SerializeField] private float _moveDelay = 0.5f;
    [SerializeField] private float _startingMoveDelay;
    [SerializeField] private bool[,] _occupiedTiles = new bool[6,8];
    [SerializeField] private bool _tryingStop;
    [SerializeField] private bool _gameOver;
    [SerializeField] private bool _InMenu = false;
    [SerializeField] private int _score;
    [SerializeField] private int _level;
    [SerializeField] private string _playerName;
    [SerializeField] private InputField _playerNameInput;


    private List<GameObject> _placedTiles = new List<GameObject>();

    private void Start()
    {
        _playerNameInput.gameObject.SetActive(false);
        _startingMoveDelay = _moveDelay;
        MoveTile();
    }

    private void Update()
    {
        if (!_InMenu)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!_gameOver)
                {
                    if (!_tryingStop)
                    {
                        TryPlaceTile();
                    }
                }
                else StartGame();

            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                _playerName = _playerNameInput.text;
                _playerNameInput.text = "";
                _playerNameInput.enabled = false;
                _InMenu = false;
                SaveScore();
                _score = 0;
                StartGame();
            }
        }
        
        if(Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
            
    }
    
    private void StartGame()
    {
        _playerNameInput.gameObject.SetActive(false);
        _gameOver = false;
        _row = 0;
        _moveDelay = _startingMoveDelay;
        for (int i = 0; i < _occupiedTiles.GetLength(0); i++)
        {
            for (int j = 0; j < _occupiedTiles.GetLength(1); j++)
            {
                _occupiedTiles[i, j] = false;
            }  
        }
        foreach (var tile in _placedTiles)
            Destroy(tile.gameObject);
        _placedTiles.Clear();_placedTiles.Clear();
        for (int i = 0; i < 3; i++)
        {
            tiles.Add(Instantiate(activeTilePrefab, new Vector3(i, 0, 0), Quaternion.identity).transform);
        }
        MoveTile();
    }
    
    private void MoveTile()
    {
       StartCoroutine(ApplyTileMove());
    }
    
    public IEnumerator ApplyTileMove()//Moves tiles based on the current direction 
    {
        int targetTile = _direction == 1 ? tiles.Count - 1 : 0;//if the direction is equal to 1, the target tile is the last tile in the list, otherwise the target tile is the first tile in the list

        if (tiles[targetTile].position.x + 2 > _occupiedTiles.GetLength(0) && _direction == 1)//If the row of tiles have moved its width in spaces, switch direction
            _direction = -1;
        else if (tiles[targetTile].position.x-1 < 0)
            _direction = 1;
       
        if(!_tryingStop)//if we're trying to stop then we should just skip to that part
        yield return new WaitForSeconds(_moveDelay);
        
        if (_tryingStop)//if... IT'S TIME TO STOP!!!!!! we help a brotha out and stop this
        {
            _tryingStop = false;//reseting class property to false for the next iteration
            yield break;
        }
        
        for (int i = 0; i < tiles.Count; i++)//change the position of each individual tile in the row
        {
            var pos = tiles[i].position;
            var newPos = new Vector2(pos.x + _direction, pos.y);
            tiles[i].position = newPos;
        }
        
        MoveTile();//begin the next rows movement
    }

    private void TryPlaceTile()
    {
        _tryingStop = true;
        List<Transform> removedTiles = new List<Transform>();
        foreach (var tile in tiles)
        {
            var position = tile.position;
            int x = (int)position.x;
            int y = (int)position.y;
            if (_row != 0)
                if (_occupiedTiles[x, y - 1] == false)
                {
                    removedTiles.Add(tile);
                    continue;
                }

            _placedTiles.Add(Instantiate(deactiveTilePrefab, new Vector3(x, y, 0), Quaternion.identity));
            _occupiedTiles[x,y] = true;
        }

        foreach (var removedTile in removedTiles)
        {
            tiles.Remove(removedTile);
            Destroy(removedTile.gameObject);
        }

        if (tiles.Count > 0)
        {
            _score += (1 * tiles.Count) * _level;
        }

        if (tiles.Count == 0)
            GameOver();
        else
            TryStartNewRound();
    }
    
    private void TryStartNewRound()
    {
        int x = 0;
        _row++;

        if (_row > 2 && tiles.Count > 2)
        {
            while (tiles.Count > 2)
            {
                var obj = tiles[tiles.Count - 1];
                tiles.Remove(obj);
                Destroy(obj.gameObject);
            }
        }
        if (_row > 4 && tiles.Count > 1)
        {
            while (tiles.Count > 1)
            {
                var obj = tiles[tiles.Count - 1];
                tiles.Remove(obj);
                Destroy(obj.gameObject);
            }
        }

        if (_row >= _occupiedTiles.GetLength(1))
        {
            GameOver(true);
            return;
        }
        
        _moveDelay -= newRowSpeedMod;
        foreach (var tile in tiles)
        {
            tile.position = new Vector2(x, _row);
            x++;
        }
        MoveTile();
    }
    
    private void GameOver(bool playerWins = false)
    {
        _tryingStop = true;
        _gameOver = true;
        foreach (var tile in tiles)
            Destroy(tile.gameObject);
        
        tiles.Clear();
        
        if (!playerWins)
        {
            print("Game Over!");
            print("Your score was: " + _score);
            _level = 0;
            PromptPlayerName();
        }
        else
        {
            _level++;
            print("Player Wins!");
        }
        
    }
    
    private void PromptPlayerName()
    {
        _InMenu = true;
        _playerNameInput.gameObject.SetActive(true);
        _playerNameInput.ActivateInputField();

    }

    private void SaveScore()
    {
        string fileName = "/score.dat";
        //check to see if the file already exists
        if (File.Exists(Application.persistentDataPath + fileName))
        {
            //if it does, load the file
            BinaryFormatter binFile = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + fileName, FileMode.Open);
            //create a new list of scores
            List<Score> scores = new List<Score>();
            //load the list of scores from the file
            scores = (List<Score>)binFile.Deserialize(file);
            //close the file
            file.Close();
            //add the new score to the list
            scores.Add(new Score(_score, _level, _playerName));
            //sort the list of scores
            scores.Sort();
            //remove the last score if there are more than 10 scores
            if (scores.Count > 10)
                scores.RemoveAt(scores.Count - 1);
            //save the list of scores to the file
            file = File.Create(Application.persistentDataPath + fileName);
            binFile.Serialize(file, scores);
            file.Close();
        }
        else
        {
            //if it doesn't, create the file
            BinaryFormatter binFile = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + fileName);
            //create a new list of scores
            List<Score> scores = new List<Score>();
            //add the new score to the list
            scores.Add(new Score(_score, _level, _playerName));
            //save the list of scores to the file
            binFile.Serialize(file, scores);
            file.Close();
        }
    }

}

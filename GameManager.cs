using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour  
{
    private Dictionary<int, NetworkObject> _spawnedCharacters = new Dictionary<int, NetworkObject>();

    private static readonly int NO_WINNER = -1;
    private static readonly int MAX_TURN_COUNT = 6;

    private bool isGameOver = false;

    [Networked(OnChanged = nameof(OnPlayingPlayerIdChanged))]
    private int playingPlayerId { get; set; }

    private int playingPlayerIndex = 0;

    private int turnCount = 0;

    public void AddPlayer(int playerId, NetworkObject player)
    {
        if (isGameOver) return;
            _spawnedCharacters.Add(playerId, player);
    }

    public void SetTurn(int playerId)
    {
        if (isGameOver) return;

        if (_spawnedCharacters.TryGetValue(playerId, out NetworkObject player))
        {
            playingPlayerId = playerId;
        }
    }


    public void RemovePlayer(int playerId)
    {
        _spawnedCharacters.Remove(playerId);
    }


    public void PlayTurn(int PlayerId)
    {
        if (isGameOver) return;

        Debug.Log(PlayerId);
        Debug.Log("Playing turn");
        var isAddScoringSucccesfull = AddScoreToPlayer(RollDice(), PlayerId);

        if (isAddScoringSucccesfull)
        {
            turnCount++;
            var winner = GetWinner();
            if ( winner== NO_WINNER)
            {
                ChangeTurn();  
            }
            else
            {
                Rpc_Send_Winner(winner);
            }
        }
    }


    private int RollDice()
    {
        return Random.Range(0, 6);
        Debug.Log("Rolling dice");
    }

    private Boolean AddScoreToPlayer(int score, int PlayerId)
    {
        if (_spawnedCharacters.TryGetValue(PlayerId, out NetworkObject player))
        {
            Debug.Log($"Score Adding to player {PlayerId}");
            var myPlayer = player.GetComponent<MyPlayer>();
            myPlayer.AddScore(score);
            return true;
        }

        return false;
    }


    private int GetWinner()
    {
        if (turnCount >= MAX_TURN_COUNT)
        {
            var lastScore = 0;
            int winnerId = 0;
            foreach (var keyValuePair in _spawnedCharacters)
            {
                var player = keyValuePair.Value.GetComponent<MyPlayer>();
                var score = player.GetScore();
                if (score > lastScore)
                {
                    winnerId = keyValuePair.Key;
                    lastScore = score;
                }
            }

            isGameOver = true;
            return winnerId;
        }

        return NO_WINNER;
    }

    private void ChangeTurn()
    {
        playingPlayerIndex++;
        if (playingPlayerIndex >= _spawnedCharacters.Count)
        {
            playingPlayerIndex = 0;
        }

        playingPlayerId = _spawnedCharacters.ElementAt(playingPlayerIndex).Key;
    }


    public static void OnPlayingPlayerIdChanged(Changed<GameManager> changed)
    {
        var players = FindObjectsOfType<MyPlayer>();
        foreach (var myPlayer in players)
        {
            if (myPlayer.Runner.LocalPlayer.PlayerId == changed.Behaviour.playingPlayerId)
            {
                Debug.Log("It is Your turn");
                break;
            }
            else
            {
                Debug.Log($"It is {changed.Behaviour.playingPlayerId} turn");
                break;
            }
        }
    }

    [Rpc(RpcSources.StateAuthority,RpcTargets.All)]
    private void Rpc_Send_Winner(int winnerId)
    {
        Debug.Log($"Winner is {winnerId}");
        foreach (var myPlayer in FindObjectsOfType<MyPlayer>())
        {
            Debug.Log($"player {myPlayer.Id} score is {myPlayer.GetScore()}");
        }
    }
}
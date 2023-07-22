using System;
using Fusion;
using UnityEngine;


public class MyPlayer : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnScoreChanged))]
    private int score { get; set; }

    

    //
    private int localScore;
    private GameManager _gameManager;


    private void Start()
    {
        if (Runner.IsServer)
        {
            _gameManager = FindObjectOfType<GameManager>();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Runner.LocalPlayer == Object.InputAuthority)
        {
            Rpc_Play_turn(Runner.LocalPlayer.PlayerId);
        }
    }


    public void AddScore(int score)
    {
        this.score += score;
    }


    public int GetScore()
    {
        return score;
    }


    public static void OnScoreChanged(Changed<MyPlayer> changed)
    {
        Debug.Log($"Score changing to {changed.Behaviour.score}");
        changed.Behaviour.localScore = changed.Behaviour.score;
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_Play_turn(int playerId)
    {
      _gameManager.PlayTurn(playerId);
    }
}
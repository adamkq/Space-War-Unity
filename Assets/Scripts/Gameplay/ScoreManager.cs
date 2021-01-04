using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ScoreManager : MonoBehaviour
{
    // Tracks the score of different players/teams in the game.
    // Displays score to the UI

    private static Dictionary<string, int> allScores;
    private static Dictionary<string, int> allTeamCounts;

    [SerializeField] private GameObject textHeader;
    [SerializeField] private GameObject[] textScores;

    private void Start()
    {
        // get names and counts of all teams, init 0
        allScores = new Dictionary<string, int>();
        allTeamCounts = new Dictionary<string, int>();

        foreach(var teamName in Enum.GetValues(typeof(Team)))
        {
            allScores.Add(teamName.ToString(), 0);
            allTeamCounts.Add(teamName.ToString(), 0);
        }

        InitialTeamCount();
    }

    private void Update()
    {
        // score UI is updated here to avoid using a static function
        int index = 0;
        foreach (var teamCount in allTeamCounts)
        {
            // team name, count, score
            string _text = teamCount.Key + "\t\t" + teamCount.Value.ToString() + "\t" + allScores[teamCount.Key];
            GameObject textScore = textScores[index];
            index++;
            textScore.GetComponent<Text>().text = _text;
        }

        // toggle scoreboard display
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            textHeader.SetActive(!textHeader.activeSelf);
            foreach (GameObject textScore in textScores)
            {
                textScore.SetActive(!textScore.activeSelf);
            }
        }
    }

    // called by agents, since only they have scores
    public static void UpdateScore(GameObject go, int dScore)
    {
        Agent _agent = go.GetComponent<Agent>();

        if (!_agent) return;

        // increment scoreboard score
        string teamName = _agent.team.ToString();
        allScores[teamName] += dScore;
    }

    // called by agents
    public static void UpdateCount(GameObject go, bool isBeingDeleted = false)
    {
        Agent _agent = go.GetComponent<Agent>();

        if (!_agent) return;

        allTeamCounts[_agent.team.ToString()] += isBeingDeleted ? -1 : 1;

        if (allTeamCounts[_agent.team.ToString()] < 0) Debug.LogWarning("Team count less than 0");
    }

    private void InitialTeamCount()
    {
        Agent[] _agents = FindObjectsOfType<Agent>();
        foreach (Agent _agent in _agents)
        {
            allTeamCounts[_agent.team.ToString()] += 1;
        }
    }
}

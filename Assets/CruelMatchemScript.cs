using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

public class CruelMatchemScript : MonoBehaviour
{

    public KMAudio Audio;
    public KMBombModule module;
    public KMSelectable[] cards;
    public KMSelectable progressor;
    public Renderer[] cardfronts;
    public Renderer[] progrend;
    public Material[] cardmats;
    public Material[] progmats;
    public TextMesh[] cardlabels;
    public TextMesh counter;
    public TextMesh proglabel;

    private readonly int[] coltable = new int[144] { 9, 91, 97, 8, 66, 44, 76, 84, 0, 57, 112, 45, 86, 73, 87, 71, 95, 129, 52, 135, 69, 92, 106, 16, 131, 130, 40, 142, 109, 42, 125, 18, 99, 89, 74, 31, 78, 113, 108, 55, 20, 136, 98, 68, 61, 90, 123, 39, 5, 29, 35, 116, 38, 72, 107, 88, 111, 114, 50, 25, 139, 34, 110, 132, 138, 4, 27, 115, 26, 23, 43, 53, 2, 37, 94, 77, 11, 137, 117, 120, 81, 70, 79, 47, 121, 21, 7, 59, 102, 105, 60, 12, 64, 143, 119, 63, 122, 15, 28, 93, 96, 67, 56, 141, 127, 49, 33, 1, 134, 3, 62, 101, 83, 32, 80, 85, 10, 24, 46, 103, 19, 128, 17, 100, 30, 51, 13, 75, 22, 118, 14, 41, 126, 104, 140, 54, 133, 82, 58, 6, 65, 124, 36, 48};
    private int[][] positions = new int[10][] { new int[25] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, new int[25], new int[25], new int[25], new int[25], new int[25], new int[25], new int[25], new int[25], new int[25] };
    private int[] pairs = new int[25] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
    private string[][][] cardconfig = new string[2][][] { new string[5][] { new string[5], new string[5], new string[5], new string[5], new string[5] }, new string[5][] { new string[5], new string[5], new string[5], new string[5], new string[5] } };
    private bool[] unavailable = new bool[144];
    private int[] colours = new int[25];
    private List<int>[] moves = new List<int>[] { new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { }, new List<int> { } };
    private bool[] matched = new bool[25];
    private bool wait;
    private int stage = 9;
    private int[] select = new int[2] { -1, -1 };

    private static int moduleIDCounter = 1;
    private int moduleID;
    private bool moduleSolved;

    void Awake()
    {     
        moduleID = moduleIDCounter++;
        pairs.Shuffle();
        for (int i = 0; i < 13; i++)
        {
            colours[2 * i] = Random.Range(0, 144);
            while (unavailable[Array.IndexOf(coltable, colours[2 * i])])
                colours[2 * i] = Random.Range(0, 144);
            int find = Array.IndexOf(coltable, colours[2 * i]);
            unavailable[find] = true;
            unavailable[143 - find] = true;
            unavailable[((find / 12) * 12) + (11 - (find % 12))] = true;
            unavailable[((11 - (find/ 12)) * 12) + (find % 12)] = true;          
            cardfronts[pairs[2 * i]].material = cardmats[colours[2 * i]];
            cardlabels[pairs[2 * i]].text = "BCFLOPSRVWYZ"[colours[2 * i] % 12].ToString();
            cardconfig[0][pairs[2 * i] / 5][pairs[2 * i] % 5] = new string[] { "Blue", "Cyan", "Forest", "Lime", "Orange", "Pink", "Red", "Silver", "Violet", "White", "Yellow", "Bronze"}[colours[2 * i] % 12] + " " + new string[] { "Chevron", "Club", "Coin", "Cup", "Diamond", "Eye", "Heart", "Pentacle", "Shield", "Six point star", "Spade", "Sword" }[colours[2 * i] / 12];
            int pairtype = Random.Range(0, 3);
            if (i < 12)
            {
                switch (pairtype)
                {
                    case 0:
                        colours[(2 * i) + 1] = coltable[143 - find];
                        break;
                    case 1:
                        colours[(2 * i) + 1] = coltable[((find / 12) * 12) + (11 - (find % 12))];
                        break;
                    case 2:
                        colours[(2 * i) + 1] = coltable[((11 - (find / 12)) * 12) + (find % 12)];
                        break;
                }
                cardfronts[pairs[(2 * i) + 1]].material = cardmats[colours[(2 * i) + 1]];
                cardlabels[pairs[(2 * i) + 1]].text = "BCFLOPRSVWYZ"[colours[(2 * i) + 1] % 12].ToString();
                cardconfig[0][pairs[(2 * i) + 1] / 5][pairs[(2 * i) + 1] % 5] = new string[] { "Blue", "Cyan", "Forest", "Lime", "Orange", "Pink", "Red", "Silver", "Violet", "White", "Yellow", "Bronze" }[colours[(2 * i) + 1] % 12] + " " + new string[] { "Chevron", "Club", "Coin", "Cup", "Diamond", "Eye", "Heart", "Pentacle", "Shield", "Six point star", "Spade", "Sword" }[colours[(2 * i) + 1] / 12];
            }
        }
        Debug.LogFormat("[Cruel Match 'em #{0}] The initial configuration of cards is: \n[Cruel Match 'em #{0}] {1}", moduleID, string.Join("\n[Cruel Match 'em #" + moduleID.ToString() + "] ", cardconfig[0].Select(j => string.Join(", ", j)).ToArray()));     
        string[] logmoves = new string[2];
        for (int i = 0; i < 16; i++)
        {
            moves[i].Add(Random.Range(0, 7));
            switch (moves[i][0])
            {
                case 0:
                    int[] r = new int[2] { Random.Range(0, 5), Random.Range(0, 5) };
                    moves[i].Add(r[0]);
                    while (r[0] == r[1] || ((i % 2 == 1) && (moves[i - 1][0] == 0 && r.Contains(moves[i - 1][1]) && r.Contains(moves[i - 1][2]))))
                        r[1] = Random.Range(0, 5);
                    moves[i].Add(r[1]);
                    for (int j = 0; j < 25; j++)
                        if (j / 5 == moves[i][1])
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][moves[i][2] * 5 + (j % 5)];
                        else if (j / 5 == moves[i][2])
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][moves[i][1] * 5 + (j % 5)];
                        else
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][j];
                    logmoves[i % 2] = "Swap rows " + (moves[i][1] + 1).ToString() + " and "  + (moves[i][2] + 1).ToString();
                    break;
                case 1:
                    r = new int[2] { Random.Range(0, 5), Random.Range(0, 5) };
                    moves[i].Add(r[0]);
                    while (r[0] == r[1] || ((i % 2 == 1) && (moves[i - 1][0] == 1 && r.Contains(moves[i - 1][1]) && r.Contains(moves[i - 1][2]))))
                        r[1] = Random.Range(0, 5);
                    moves[i].Add(r[1]);
                    for (int j = 0; j < 25; j++)
                        if (j % 5 == moves[i][1])
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][moves[i][2] + (j / 5) * 5];
                        else if (j % 5 == moves[i][2])
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][moves[i][1] + (j / 5) * 5];
                        else
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][j];
                    logmoves[i % 2] = "Swap columns " + (moves[i][1] + 1).ToString() + " and " + (moves[i][2] + 1).ToString();
                    break;
                case 2:
                    r = new int[2] { Random.Range(0, 5), Random.Range(0, 2) };
                    if(i % 2 == 1)
                        while(moves[i - 1][0] == 2 && Enumerable.SequenceEqual(r, moves[i - 1].Where((x, k) => k > 0)))
                            r = new int[2] { Random.Range(0, 5), Random.Range(0, 2) };
                    moves[i].Add(r[0]);
                    moves[i].Add(r[1]);
                    for (int j = 0; j < 25; j++)
                        if (j / 5 == moves[i][1])
                        {
                            if (r[1] == 0)
                                positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][moves[i][1] * 5 + ((j + 1) % 5)];
                            else
                                positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][moves[i][1] * 5 + ((j + 4) % 5)];
                        }
                        else
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][j];
                    logmoves[i % 2] =  "Shift row " + (moves[i][1] + 1).ToString() + " one space " + (r[1] == 0 ? "left" : "right");
                    break;
                case 3:
                    r = new int[2] { Random.Range(0, 5), Random.Range(0, 2) };
                    if (i % 2 == 1)
                        while (moves[i - 1][0] == 3 && Enumerable.SequenceEqual(r, moves[i - 1].Where((x, k) => k > 0)))
                            r = new int[2] { Random.Range(0, 5), Random.Range(0, 2) };
                    moves[i].Add(r[0]);
                    moves[i].Add(r[1]);
                    for (int j = 0; j < 25; j++)
                        if (j % 5 == moves[i][1])
                        {
                            if (r[1] == 0)
                                positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][(j + 5) % 25];
                            else
                                positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][(j + 20) % 25];
                        }
                        else
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][j];
                    logmoves[i % 2] =  "Shift column " + (moves[i][1] + 1).ToString() + " one space " + (r[1] == 0 ? "up" : "down");
                    break;
                case 4:
                    int s = Random.Range(0, 6);
                    if (i % 2 == 1)
                        while (moves[i - 1][1] == s && moves[i - 1][0] == 5)
                            s = Random.Range(0, 6);
                    moves[i].Add(s);
                    for (int j = 0; j < 25; j++)
                    {
                        positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][j];
                    }
                    switch (moves[i][1])
                    {
                        case 0:
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][0] = positions[i % 2 == 1 ? 9 : i / 2][4];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][4] = positions[i % 2 == 1 ? 9 : i / 2][24];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][24] = positions[i % 2 == 1 ? 9 : i / 2][20];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][20] = positions[i % 2 == 1 ? 9 : i / 2][0];
                            break;
                        case 1:
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][1] = positions[i % 2 == 1 ? 9 : i / 2][9];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][9] = positions[i % 2 == 1 ? 9 : i / 2][23];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][23] = positions[i % 2 == 1 ? 9 : i / 2][15];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][15] = positions[i % 2 == 1 ? 9 : i / 2][1];
                            break;
                        case 2:
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][2] = positions[i % 2 == 1 ? 9 : i / 2][14];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][14] = positions[i % 2 == 1 ? 9 : i / 2][22];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][22] = positions[i % 2 == 1 ? 9 : i / 2][10];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][10] = positions[i % 2 == 1 ? 9 : i / 2][2];
                            break;
                        case 3:
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][3] = positions[i % 2 == 1 ? 9 : i / 2][19];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][19] = positions[i % 2 == 1 ? 9 : i / 2][21];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][21] = positions[i % 2 == 1 ? 9 : i / 2][5];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][5] = positions[i % 2 == 1 ? 9 : i / 2][3];
                            break;
                        case 4:
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][6] = positions[i % 2 == 1 ? 9 : i / 2][8];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][8] = positions[i % 2 == 1 ? 9 : i / 2][18];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][18] = positions[i % 2 == 1 ? 9 : i / 2][16];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][16] = positions[i % 2 == 1 ? 9 : i / 2][6];
                            break;
                        case 5:
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][7] = positions[i % 2 == 1 ? 9 : i / 2][13];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][13] = positions[i % 2 == 1 ? 9 : i / 2][17];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][17] = positions[i % 2 == 1 ? 9 : i / 2][11];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][11] = positions[i % 2 == 1 ? 9 : i / 2][7];
                            break;
                    }
                    logmoves[i % 2] = "Cycle card " + new string[] { "1", "2", "3", "6", "7", "8" }[moves[i][1]] + " anticlockwise";
                    break;
                case 5:
                    s = Random.Range(0, 6);
                    if (i % 2 == 1)
                        while (moves[i - 1][1] == s && moves[i - 1][0] == 4)
                            s = Random.Range(0, 6);
                    moves[i].Add(s);
                    for (int j = 0; j < 25; j++)
                    {
                        positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][j];
                    }
                    switch (moves[i][1])
                    {
                        case 0:
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][0] = positions[i % 2 == 1 ? 9 : i / 2][20];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][4] = positions[i % 2 == 1 ? 9 : i / 2][0];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][24] = positions[i % 2 == 1 ? 9 : i / 2][4];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][20] = positions[i % 2 == 1 ? 9 : i / 2][24];
                            break;
                        case 1:
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][1] = positions[i % 2 == 1 ? 9 : i / 2][15];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][9] = positions[i % 2 == 1 ? 9 : i / 2][1];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][23] = positions[i % 2 == 1 ? 9 : i / 2][9];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][15] = positions[i % 2 == 1 ? 9 : i / 2][23];
                            break;
                        case 2:
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][2] = positions[i % 2 == 1 ? 9 : i / 2][10];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][14] = positions[i % 2 == 1 ? 9 : i / 2][2];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][22] = positions[i % 2 == 1 ? 9 : i / 2][14];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][10] = positions[i % 2 == 1 ? 9 : i / 2][22];
                            break;
                        case 3:
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][3] = positions[i % 2 == 1 ? 9 : i / 2][5];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][19] = positions[i % 2 == 1 ? 9 : i / 2][3];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][21] = positions[i % 2 == 1 ? 9 : i / 2][19];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][5] = positions[i % 2 == 1 ? 9 : i / 2][21];
                            break;
                        case 4:
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][6] = positions[i % 2 == 1 ? 9 : i / 2][16];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][8] = positions[i % 2 == 1 ? 9 : i / 2][6];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][18] = positions[i % 2 == 1 ? 9 : i / 2][8];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][16] = positions[i % 2 == 1 ? 9 : i / 2][18];
                            break;
                        case 5:
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][7] = positions[i % 2 == 1 ? 9 : i / 2][11];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][13] = positions[i % 2 == 1 ? 9 : i / 2][7];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][17] = positions[i % 2 == 1 ? 9 : i / 2][13];
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][11] = positions[i % 2 == 1 ? 9 : i / 2][17];
                            break;
                    }
                    logmoves[i % 2] = "Cycle card " + new string[] { "1", "2", "3", "6", "7", "8" }[moves[i][1]] + " clockwise";
                    break;
                default:
                    r = new int[2] { Random.Range(0, 25), Random.Range(0, 25) };
                    moves[i].Add(r[0]);
                    while (r[0] == r[1] || ((i % 2 == 1) && (moves[i - 1][0] == 6 && r.Contains(moves[i - 1][1]) && r.Contains(moves[i - 1][2]))))
                        r[1] = Random.Range(0, 25);
                    moves[i].Add(r[1]);
                    for (int j = 0; j < 25; j++)
                        if (j == r[0])
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][r[1]];
                        else if (j == r[1])
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][r[0]];
                        else
                            positions[i % 2 == 1 ? (i / 2) + 1 : 9][j] = positions[i % 2 == 1 ? 9 : i / 2][j];
                    logmoves[i % 2] = "Swap cards " + (moves[i][1] + 1).ToString() + " and " + (moves[i][2] + 1).ToString();
                    break;
            }
            if (i % 2 == 1)
                Debug.LogFormat("[Cruel Match 'em #{0}] Move {1} : {2} + {3}", moduleID, (i / 2) + 1, logmoves[0], logmoves[1]);
        }
        for (int i = 0; i < 25; i++)
            cardconfig[1][Array.IndexOf(positions[8], pairs[i]) / 5][Array.IndexOf(positions[8], pairs[i]) % 5] = new string[] { "Blue", "Cyan", "Forest", "Lime", "Orange", "Pink", "Red", "Silver", "Violet", "White", "Yellow", "Bronze" }[colours[i] % 12] + " " + new string[] { "Chevron", "Club", "Coin", "Cup", "Diamond", "Eye", "Heart", "Pentacle", "Shield", "Six point star", "Spade", "Sword" }[colours[i] / 12];
        Debug.LogFormat("[Cruel Match 'em #{0}] The final configuration of cards is: \n[Cruel Match 'em #{0}] {1}", moduleID, string.Join("\n[Cruel Match 'em #" + moduleID.ToString() + "] ", cardconfig[1].Select(j => string.Join(", ", j)).ToArray()));
        module.OnActivate += Activate;
    }

    private void Activate()
    {
        foreach (Renderer prog in progrend)
            prog.material = progmats[0];
        proglabel.text = "START";
        progressor.OnInteract += delegate ()
        {
            if (!wait)
            {
                if (stage == 9)
                {
                    stage--;
                    progressor.AddInteractionPunch(0.2f);
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, progressor.transform);
                    Audio.PlaySoundAtTransform("Cardflip", transform);
                    StartCoroutine(Wait(0.5f, true));
                    for (int i = 0; i < 25; i++)
                        StartCoroutine(Flip(i));
                }
                else if (stage > 0)
                {
                    stage--;
                    progressor.AddInteractionPunch(0.2f);
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, progressor.transform);
                    Audio.PlaySoundAtTransform("Cardshift", transform);
                    StartCoroutine(Wait(1, true));
                    foreach (int c in positions[7 - stage].Where((x, i) => x != positions[8 - stage][i]))
                        StartCoroutine(Move(c, Array.IndexOf(positions[8 - stage], c)));
                    if (stage < 1)
                    {
                        foreach (KMSelectable card in cards)
                        {
                            int c = Array.IndexOf(cards, card);
                            card.OnInteract += delegate { if (!wait && !moduleSolved && !matched[c] && !select.Contains(c)) CardSelect(c); return false; };
                        }
                    }
                }
            }
            return false;
        };
    }

    private IEnumerator Wait(float t, bool p)
    {
        wait = true;
        if (p)
        {
            foreach (Renderer prog in progrend)
                prog.material = progmats[1];
            proglabel.text = "WAIT";
            proglabel.color = new Color32(70, 70, 70, 255);
        }
        yield return new WaitForSeconds(t);
        wait = false;
        if (p)
        {
            foreach (Renderer prog in progrend)
                prog.material = stage > 0 ? progmats[0] : progmats[2];
            proglabel.text = stage > 0 ? "NEXT" : "";
            proglabel.color = new Color32(0, 170, 0, 255);
            counter.text = stage > 0 ? stage.ToString() : "GO";
        }
    }

    private IEnumerator Flip(int card)
    {
        cards[card].transform.localPosition += new Vector3(0, 0.006f, 0);
        for (int i = 0; i < 30; i++)
        {
            cards[card].transform.localEulerAngles += new Vector3(0, 0, 6);
            yield return null;
        }
        cards[card].transform.localPosition -= new Vector3(0, 0.006f, 0);
    }

    private IEnumerator Move(int card, int end)
    {
        float[][] coords = new float[2][] { new float[2] { cards[card].transform.localPosition.x, cards[card].transform.localPosition.z }, new float[2] { new float[] { -0.0418f, -0.0213f, -0.0008f, 0.0197f, 0.0402f }[end % 5], new float[] { 0.0555f, 0.0277f, 0, -0.0272f, -0.0544f }[end / 5] } };
        for (int i = 0; i < 45; i++)
        {
            yield return null;
            cards[card].transform.localPosition = new Vector3(Mathf.Lerp(coords[0][0], coords[1][0], (float)i / 44), 0.001f, Mathf.Lerp(coords[0][1], coords[1][1], (float)i / 44));
        }
        cards[card].transform.localPosition -= new Vector3(0, 0.002f, 0);
    }

    private void CardSelect(int card)
    {
        StartCoroutine(Flip(card));
        Audio.PlaySoundAtTransform("Cardflip", cards[card].transform);
        if (select.All(s => s == -1))
        {
            StartCoroutine(Wait(0.5f, false));
            select[0] = card;
            if (Array.IndexOf(pairs, card) == 24)
                Debug.LogFormat("[Cruel Match 'em #{0}] Joker selected. Strike imminent.", moduleID);
            else
                Debug.LogFormat("[Cruel Match 'em #{0}] Card {1} selected. Expecting card {2}.", moduleID, Array.IndexOf(positions[8], card) + 1, Array.IndexOf(pairs, card) % 2 == 1 ? Array.IndexOf(positions[8], pairs[Array.IndexOf(pairs, card) - 1]) + 1 : Array.IndexOf(positions[8], pairs[Array.IndexOf(pairs, card) + 1]) + 1);
        }
        else
        {
            StartCoroutine(Wait(1.5f, false));
            select[1] = card;
            Debug.LogFormat("[Cruel Match 'em #{0}] Card {1} selected. This is {2}a matching pair.", moduleID, Array.IndexOf(positions[8], card) + 1, Array.IndexOf(pairs, card) / 2 == Array.IndexOf(pairs, select[0]) / 2 ? "" : "not ");
            StartCoroutine(Check());
        }
    }

    private IEnumerator Check()
    {
        yield return new WaitForSeconds(0.5f);
        if (Array.IndexOf(pairs, select[0]) / 2 == Array.IndexOf(pairs, select[1]) / 2)
        {
            matched[select[0]] = true;
            matched[select[1]] = true;
            wait = false;
            if (matched.Where(t => t).Count() == 24)
            {
                moduleSolved = true;
                module.HandlePass();
                counter.text = "GG";
                counter.color = new Color32(0, 255, 0, 255);
            }
        }
        else
        {
            module.HandleStrike();
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(Flip(select[0]));
            StartCoroutine(Flip(select[1]));
        }
        select = new int[2] { -1, -1 };
    }

    //twitch plays
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} start [Flips the cards over] | !{0} next [Performs the next card shuffle] | !{0} pick a1 e5 [Picks the two specified cards at the specified coordinates]";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*start\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (stage != 9)
            {
                yield return "sendtochaterror The cards have already been flipped!";
            }
            else
            {
                progressor.OnInteract();
            }
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*next\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (stage == 9)
            {
                yield return "sendtochaterror Cannot shuffle until the cards have been flipped!";
            }
            else if (stage == 0)
            {
                yield return "sendtochaterror Cannot shuffle since the module is out of shuffles!";
            }
            else if (wait)
            {
                yield return "sendtochaterror Cannot shuffle while the cards are animating!";
            }
            else
            {
                progressor.OnInteract();
            }
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*pick\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (stage != 0)
            {
                yield return "sendtochaterror Cannot pick cards yet because not all shuffles have been performed!";
            }
            else if (wait || !select.Contains(-1))
            {
                yield return "sendtochaterror Cannot pick cards while the cards are animating!";
            }
            else if (parameters.Length > 3)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 3)
            {
                string[] valids = { "a1", "b1", "c1", "d1", "e1", "a2", "b2", "c2", "d2", "e2", "a3", "b3", "c3", "d3", "e3", "a4", "b4", "c4", "d4", "e4", "a5", "b5", "c5", "d5", "e5" };
                if (!valids.Contains(parameters[1].ToLower()))
                {
                    yield return "sendtochaterror The specified coordinate '" + parameters[1] + "' is invalid!";
                    yield break;
                }
                if (!valids.Contains(parameters[2].ToLower()))
                {
                    yield return "sendtochaterror The specified coordinate '" + parameters[2] + "' is invalid!";
                    yield break;
                }
                if (parameters[1].EqualsIgnoreCase(parameters[2]))
                {
                    yield return "sendtochaterror The coordinate of the first picked card cannot be the same as the second picked card!";
                    yield break;
                }
                if (matched[positions[8][Array.IndexOf(valids, parameters[1].ToLower())]] || matched[positions[8][Array.IndexOf(valids, parameters[2].ToLower())]])
                {
                    yield return "sendtochaterror One of these cards has already been matched!";
                    yield break;
                }
                if (select[0] != -1)
                {
                    StartCoroutine(Flip(select[0]));
                    select[0] = -1;
                    while (select[0] != -1 || select[1] != -1) { yield return true; yield return new WaitForSeconds(0.1f); }
                }
                cards[positions[8][Array.IndexOf(valids, parameters[1].ToLower())]].OnInteract();
                if (Array.IndexOf(pairs, select[0]) == 24)
                {
                    yield return "strike";
                }
                while (wait) { yield return true; yield return new WaitForSeconds(0.1f); }
                cards[positions[8][Array.IndexOf(valids, parameters[2].ToLower())]].OnInteract();
                if (Array.IndexOf(pairs, select[0]) / 2 != Array.IndexOf(pairs, select[1]) / 2)
                {
                    yield return "strike";
                }
                else if ((Array.IndexOf(pairs, select[0]) / 2 == Array.IndexOf(pairs, select[1]) / 2) && matched.Where(t => t).Count() == 24)
                {
                    yield return "solve";
                }
            }
            else if (parameters.Length == 2)
            {
                string[] valids = { "a1", "b1", "c1", "d1", "e1", "a2", "b2", "c2", "d2", "e2", "a3", "b3", "c3", "d3", "e3", "a4", "b4", "c4", "d4", "e4", "a5", "b5", "c5", "d5", "e5" };
                if (!valids.Contains(parameters[1].ToLower()))
                {
                    yield return "sendtochaterror The specified coordinate '" + parameters[1] + "' is invalid!";
                }
                else
                {
                    yield return "sendtochaterror Please specify another coordinate of a card to pick!";
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the coordinates of the cards you wish to pick!";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (wait) { yield return true; yield return new WaitForSeconds(0.1f); }
        int end = stage;
        for (int i = 0; i < end; i++)
        {
            progressor.OnInteract();
            while (wait) { yield return true; yield return new WaitForSeconds(0.1f); }
        }
        if (select[0] != -1)
        {
            for (int i = 0; i < 25; i++)
            {
                if (Array.IndexOf(pairs, select[0]) / 2 == Array.IndexOf(pairs, i) / 2)
                {
                    cards[positions[8][i]].OnInteract();
                    while (select[0] != -1 || select[1] != -1) { yield return true; yield return new WaitForSeconds(0.1f); }
                    break;
                }
            }
        }
        for (int j = 0; j < 25; j++)
        {
            for (int i = 0; i < 25; i++)
            {
                if ((Array.IndexOf(pairs, positions[8][j]) / 2 == Array.IndexOf(pairs, positions[8][i]) / 2) && matched[positions[8][j]] != true && matched[positions[8][i]] != true && i != j)
                {
                    cards[positions[8][j]].OnInteract();
                    while (wait) { yield return true; yield return new WaitForSeconds(0.1f); }
                    cards[positions[8][i]].OnInteract();
                    while (select[0] != -1 || select[1] != -1) { yield return true; yield return new WaitForSeconds(0.1f); }
                    break;
                }
            }
        }
        while (!moduleSolved) { yield return true; yield return new WaitForSeconds(0.1f); }
    }
}
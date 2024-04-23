using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System;

public class Manager : MonoBehaviour
{
    [SerializeField] private GameObject square;
    private List<GameObject> squares = new List<GameObject>();
    [SerializeField] public TMP_Text evalText;
    [SerializeField] public TMP_Text desc;
    public List<int> FULLTABLE = new List<int>();
    public List<int> goodMoves = new List<int>();
    [Serializable]
    public class table
    {
        public List<int> Os = new List<int>();
        public List<int> Xs = new List<int>();
    }
    [Serializable]
    public class trios
    {
        public string type;
        public List<int> coordinates = new List<int>();
        public trios(int f, int s, int t)
        {
            coordinates.Add(f);
            coordinates.Add(s);
            coordinates.Add(t);
        }
        public List<int> extended = new List<int>();
    }
    [Serializable]
    public class duos
    {
        public List<int> coordinates = new List<int>();
        public string type;
        public duos(int start, int end, string t)
        {
            coordinates.Add(start);
            coordinates.Add(end);
            type = t;
        }
        public List<int> extended = new List<int>();
    }
    [Serializable]
    public class Fours
    {
        public string type;
        public List<int> coordinates = new List<int>();
        public Fours(int f, int s, int t, int c)
        {
            coordinates.Add(f);
            coordinates.Add(s);
            coordinates.Add(t);
            coordinates.Add(c);
        }
        public List<int> extended = new List<int>();
    }
    public class Fives
    {
        public string type;
        public List<int> coordinates = new List<int>();
        public Fives(int f, int s, int t, int c, int p)
        {
            coordinates.Add(f);
            coordinates.Add(s);
            coordinates.Add(t);
            coordinates.Add(c);
            coordinates.Add(p);
        }
    }
    [SerializeReference]
    public List<duos> PlayerDuos = new List<duos>();

    [SerializeReference]
    public List<duos> AiDuos = new List<duos>();
    [SerializeReference]
    public List<trios> PlayerTrios = new List<trios>();
    [SerializeReference]
    public List<trios> AiTrios = new List<trios>();
    [SerializeReference]
    public List<Fours> PlayerFours = new List<Fours>();
    [SerializeReference]
    public List<Fours> AiFours = new List<Fours>();
    [SerializeReference]
    public List<Fives> PlayerFives = new List<Fives>();
    [SerializeReference]
    public List<Fives> AiFives = new List<Fives>();
    [SerializeReference]
    public table currentPosition = new table();
    public int win = 0;
    public int baseDepth;
    public List<int> line = new List<int>();
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i <= 7; i++)
        {
            for (int j = 0; j <= 7; j++)
            {
                var a = Instantiate(square, new Vector3(-3.5f + j, 3.5f - i, 0), Quaternion.identity);                        //stworzenie kwadratów oraz tablicy z wszystkimi ruchami
                a.GetComponent<squares>().changeFieldName((i + 1) * 10 + (j + 1));
                FULLTABLE.Add((i + 1) * 10 + (j + 1));
                squares.Add(a);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void aiMove(int result)                //ruch ai sprawdzanie żeby nie było zajęte i żeby dwa razy nie wpisać do tablicy
    {
        // result=UnityEngine.Random.Range(0,63);
        // while(squares[result].GetComponent<squares>().isUsed==true)
        // {
        //     result=UnityEngine.Random.Range(0,63);
        // }
        foreach (GameObject G in squares)
        {
            if (G.GetComponent<squares>().getFieldName() == result)
            {
                G.GetComponent<squares>().putX();
            }
        }

    }
    public void aiMoveCheck(int m, bool isPlayer)
    {
        changeCurrentPosition(m, isPlayer);
    }

    public int evalPosition()                         //ocena pozycji
    {
        int ev = 0;
        if(PlayerFives.Count>0)
        {
            ev+=2000;
        }
        if(AiFives.Count>0)
        {
            ev-=2000;
        }
        foreach (duos d in PlayerDuos)
        {
            ev += 2;
        }
        foreach (duos d in AiDuos)
        {
            ev -= 1;
        }
        foreach (trios t in PlayerTrios)
        {
            ev += 10;
        }
        foreach (trios t in AiTrios)
        {
            ev -= 5;
        }
        // foreach(int x in currentPosition.Xs)
        // {
        //     List<int>temp=checkAllAdjecent(x);
        //     foreach(int t in temp)
        //     {
        //         if(currentPosition.Os.Contains(t))
        //         {
        //             ev--;
        //         }
        //     }
        // }
        ev += 4*checkUnblockedPairs(true);
        ev -= checkUnblockedPairs(false);
        ev += 10*checkUnblockedTrios(true);
        ev -= 9*checkUnblockedTrios(false);
        ev += 100*checkUnblockedFours(true);
        ev -= 90*checkUnblockedFours(false);
        return ev;
    }

    public List<int> filterAllAdjacent(List<int> fields, bool player, bool ai)  //ta funkcja zwraca wolne pola z wymienionych
    {
        List<int> toDel = new List<int>();
        if (player)
        {
            foreach (int i in fields)
            {
                if (currentPosition.Os.Contains(i))
                {
                    toDel.Add(i);
                }
            }
        }
        if (ai)
        {
            foreach (int i in fields)
            {
                if (currentPosition.Xs.Contains(i))
                {
                    toDel.Add(i);
                }
            }
        }
        foreach (int i in toDel)
        {
            fields.Remove(i);
        }
        return fields;
    }

    public List<int> checkAllAdjecent(int pos)
    {
        List<int> adj = new List<int>();                                        //tworzy 1 liste gotowa do wysłania zwraca wszystkie pola do okoła
        int col = 0, row = 0;
        int tempPos = 0;
        row = pos / 10;         //ustawia polozenie punktu
        col = pos - (row * 10);
        for (int l = 0; l <= 2; l++)           //sprawdza kolumny
        {
            for (int m = 0; m <= 2; m++)       //sprawdza rzędy
            {
                if (col == 1 && l == 0)                  //skipy skrajne
                {
                    continue;
                }
                if (col == 8 && l == 2)
                {
                    continue;
                }
                if (row == 1 && m == 0)
                {
                    continue;
                }
                if (row == 8 && m == 2)
                {
                    continue;
                }
                if (l == 1 && m == 1)
                {
                    continue;
                }
                tempPos = pos - 1 + l - 10 + 10 * m;
                adj.Add(tempPos);
            }
        }
        return adj;
    }
    public List<int> checkAdjecent(int pos, bool isPlayer)   //zwaraca sąsiadujące symbole tego samego typu (co stand star platinum)
    {
        List<int> adj = new List<int>();                                        //tworzy 2 listy jedna która mieli wszystkie pola po kolei druga to już gotowa do wysłania
        List<int> adjFinal = new List<int>();
        int col = 0, row = 0;
        int tempPos = 0;
        row = pos / 10;         //ustawia polozenie punktu
        col = pos - (row * 10);
        for (int l = 0; l <= 2; l++)           //sprawdza kolumny
        {
            for (int m = 0; m <= 2; m++)       //sprawdza rzędy
            {
                if (col == 1 && l == 0)                  //skipy skrajne
                {
                    continue;
                }
                if (col == 8 && l == 2)
                {
                    continue;
                }
                if (row == 1 && m == 0)
                {
                    continue;
                }
                if (row == 8 && m == 2)
                {
                    continue;
                }
                if (l == 1 && m == 1)
                {
                    continue;
                }
                tempPos = pos - 1 + l - 10 + 10 * m;
                adj.Add(tempPos);
            }
        }
        if (isPlayer)
        {
            foreach (int i in adj)
            {
                if (currentPosition.Os.Contains(i))
                {
                    adjFinal.Add(i);
                }
            }
            return adjFinal;
        }
        else
        {
            foreach (int i in adj)
            {
                if (currentPosition.Xs.Contains(i))
                {
                    adjFinal.Add(i);
                }
            }

            return adjFinal;
        }
    }

    public int checkDuos(int num, bool isPlayer)              // tworzy liste tymczasowo dodanych nowych par
    {                                                       // jezeli ruch ma byc cofniety to robi to minimax
        List<int> adjacentNum = checkAdjecent(num, isPlayer);    // sprawdza dla podanego pola wszystkie pary - trójki - wygrane
        List<duos> tempDuos = new List<duos>();
        if (isPlayer)
        {
            foreach (int i in adjacentNum)
            {
                if (num == i - 1 || num == i + 1)
                {
                    var d = new duos(num, i, "row");
                    d.coordinates.Sort();
                    if (!PlayerDuos.Any(t => t.coordinates.SequenceEqual(d.coordinates)))
                    {
                        PlayerDuos.Add(d);
                        tempDuos.Add(d);
                        d.extended.Add(d.coordinates[0] - 1);
                        d.extended.Add(d.coordinates[1] + 1);
                        // Debug.Log(d.coordinates[0]+" "+d.coordinates[1]+" "+isPlay.ToString());
                    }
                }
                if (num == i - 10 || num == i + 10)
                {
                    var d = new duos(num, i, "c");
                    d.coordinates.Sort();
                    if (!PlayerDuos.Any(t => t.coordinates.SequenceEqual(d.coordinates)))
                    {
                        PlayerDuos.Add(d);
                        tempDuos.Add(d);
                        d.extended.Add(d.coordinates[0] - 10);
                        d.extended.Add(d.coordinates[1] + 10);
                        // Debug.Log(d.coordinates[0]+" "+d.coordinates[1]+" "+isPlay.ToString());
                    }
                }
                if (num == i - 11 || num == i + 11)
                {
                    var d = new duos(num, i, "l");
                    d.coordinates.Sort();
                    if (!PlayerDuos.Any(t => t.coordinates.SequenceEqual(d.coordinates)))
                    {
                        PlayerDuos.Add(d);
                        tempDuos.Add(d);
                        d.extended.Add(d.coordinates[0] - 11);
                        d.extended.Add(d.coordinates[1] + 11);
                        // Debug.Log(d.coordinates[0]+" "+d.coordinates[1]+" "+isPlay.ToString());
                    }
                }
                if (num == i - 9 || num == i + 9)
                {
                    var d = new duos(num, i, "r");
                    d.coordinates.Sort();
                    if (!PlayerDuos.Any(t => t.coordinates.SequenceEqual(d.coordinates)))
                    {
                        PlayerDuos.Add(d);
                        tempDuos.Add(d);
                        d.extended.Add(d.coordinates[0] - 9);
                        d.extended.Add(d.coordinates[1] + 9);
                        // Debug.Log(d.coordinates[0]+" "+d.coordinates[1]+" "+isPlay.ToString());
                    }
                }
            }
        }
        else
        {
            foreach (int i in adjacentNum)
            {
                if (num == i - 1 || num == i + 1)
                {
                    var d = new duos(num, i, "row");
                    d.coordinates.Sort();
                    if (!AiDuos.Any(t => t.coordinates.SequenceEqual(d.coordinates)))
                    {
                        AiDuos.Add(d);
                        tempDuos.Add(d);
                        d.extended.Add(d.coordinates[0] - 1);
                        d.extended.Add(d.coordinates[1] + 1);
                        // Debug.Log(d.coordinates[0]+" "+d.coordinates[1]+" "+isPlay.ToString());
                    }
                }
                if (num == i - 10 || num == i + 10)
                {
                    var d = new duos(num, i, "c");
                    d.coordinates.Sort();
                    if (!AiDuos.Any(t => t.coordinates.SequenceEqual(d.coordinates)))
                    {
                        AiDuos.Add(d);
                        tempDuos.Add(d);
                        d.extended.Add(d.coordinates[0] - 10);
                        d.extended.Add(d.coordinates[1] + 10);
                        // Debug.Log(d.coordinates[0]+" "+d.coordinates[1]+" "+isPlay.ToString());
                    }
                }
                if (num == i - 11 || num == i + 11)
                {
                    var d = new duos(num, i, "l");
                    d.coordinates.Sort();
                    if (!AiDuos.Any(t => t.coordinates.SequenceEqual(d.coordinates)))
                    {
                        AiDuos.Add(d);
                        tempDuos.Add(d);
                        d.extended.Add(d.coordinates[0] - 11);
                        d.extended.Add(d.coordinates[1] + 11);
                        // Debug.Log(d.coordinates[0]+" "+d.coordinates[1]+" "+isPlay.ToString());
                    }
                }
                if (num == i - 9 || num == i + 9)
                {
                    var d = new duos(num, i, "r");
                    d.coordinates.Sort();
                    if (!AiDuos.Any(t => t.coordinates.SequenceEqual(d.coordinates)))
                    {
                        AiDuos.Add(d);
                        tempDuos.Add(d);
                        d.extended.Add(d.coordinates[0] - 9);
                        d.extended.Add(d.coordinates[1] + 9);
                        // Debug.Log(d.coordinates[0]+" "+d.coordinates[1]+" "+isPlay.ToString());
                    }
                }
            }
        }
        return checkTrios(isPlayer, tempDuos);
    }
    public int checkTrios(bool isPlayer, List<duos> newDuos)
    {
        List<trios> tempTri = new List<trios>();
        if (isPlayer)
        {
            foreach (duos d in newDuos)
            {
                foreach (duos s in PlayerDuos)
                {
                    List<int> diff = d.coordinates.Union(s.coordinates).ToList();
                    if (diff.Count==3 && d != s && d.type == s.type)
                    {
                        diff.Sort();
                        trios tempTrios = new trios(diff[0], diff[1], diff[2]);
                        if (!PlayerTrios.Any(t => t.coordinates.SequenceEqual(tempTrios.coordinates)))
                        {
                            int t = getTypeValue(s.type);             // zwraca 1,9,10,11 zależnie od ruchu
                            tempTrios.type = d.type;
                            PlayerTrios.Add(tempTrios);
                            tempTri.Add(tempTrios);
                            tempTrios.extended.Add(diff[0] - t);
                            tempTrios.extended.Add(diff[2] + t);
                        }
                    }
                }
            }
        }
        else
        {
            foreach (duos d in newDuos)
            {
                foreach (duos s in AiDuos)
                {
                    List<int> diff = d.coordinates.Union(s.coordinates).ToList();
                    if (diff.Count==3 && d != s && d.type == s.type)
                    {
                        diff.Sort();
                        trios tempTrios = new trios(diff[0], diff[1], diff[2]);
                        if (!AiTrios.Any(t => t.coordinates.SequenceEqual(tempTrios.coordinates)))
                        {
                            int t = getTypeValue(s.type);   // zwraca 1,9,10,11 zależnie od ruchu
                            tempTrios.type = d.type;
                            AiTrios.Add(tempTrios);
                            tempTri.Add(tempTrios);
                            tempTrios.extended.Add(diff[0] - t);
                            tempTrios.extended.Add(diff[2] + t);
                        }
                    }
                }
            }
        }
        return checkFours(isPlayer, tempTri);
    }

    public int checkFours(bool isPlayer, List<trios> newTrios)
    {
        List<Fours> tempFours = new List<Fours>();
        if (isPlayer)
        {
            foreach (trios t in newTrios)
            {
                foreach (duos d in PlayerDuos)
                {
                    List<int> diffFour = t.coordinates.Union(d.coordinates).ToList();
                    if (diffFour.Count == 4 && d.type == t.type)
                    {
                        diffFour.Sort();
                        Fours tempFour = new Fours(diffFour[0], diffFour[1], diffFour[2], diffFour[3]);
                        if (!PlayerFours.Any(t => t.coordinates.SequenceEqual(tempFour.coordinates)))
                        {
                            int v = getTypeValue(t.type);   // zwraca 1,9,10,11 zależnie od ruchu
                            tempFour.type = d.type;
                            PlayerFours.Add(tempFour);
                            tempFours.Add(tempFour);
                            tempFour.extended.Add(diffFour[0] - v);
                            tempFour.extended.Add(diffFour[3] + v);
                        }
                    }
                }
            }
        }
        else
        {
            foreach (trios t in newTrios)
            {
                foreach (duos d in AiDuos)
                {
                    List<int> diffFour = t.coordinates.Union(d.coordinates).ToList();
                    if (diffFour.Count == 4 && d.type == t.type)
                    {
                        diffFour.Sort();
                        Fours tempFour = new Fours(diffFour[0], diffFour[1], diffFour[2], diffFour[3]);
                        if (!AiFours.Any(t => t.coordinates.SequenceEqual(tempFour.coordinates)))
                        {
                            int v = getTypeValue(t.type);   // zwraca 1,9,10,11 zależnie od ruchu
                            tempFour.type = d.type;
                            AiFours.Add(tempFour);
                            tempFours.Add(tempFour);
                            tempFour.extended.Add(diffFour[0] - v);
                            tempFour.extended.Add(diffFour[3] + v);
                        }
                    }
                }
            }
        }
        return checkFives(isPlayer, tempFours);
    }

    public int checkFives(bool isPlayer, List<Fours> newFours)
    {   
        if (isPlayer)
        {
            foreach (Fours t in newFours)
            {
                foreach (duos d in PlayerDuos)
                {
                    List<int> diffFive = t.coordinates.Union(d.coordinates).ToList();
                    if (diffFive.Count == 5 && d.type == t.type)
                    {
                        diffFive.Sort();
                        Fives tempFive = new Fives(diffFive[0], diffFive[1], diffFive[2], diffFive[3],diffFive[4]);
                        if (!PlayerFours.Any(t => t.coordinates.SequenceEqual(tempFive.coordinates)))
                        {
                            tempFive.type = d.type;
                            PlayerFives.Add(tempFive);
                            return 1000;
                        }
                    }
                }
            }
        }
        else
        {
            foreach (Fours t in newFours)
            {
                foreach (duos d in AiDuos)
                {
                    List<int> diffFive = t.coordinates.Union(d.coordinates).ToList();
                    if (diffFive.Count == 5 && d.type == t.type)
                    {
                        diffFive.Sort();
                        Fives tempFive = new Fives(diffFive[0], diffFive[1], diffFive[2], diffFive[3],diffFive[4]);
                        if (!AiFours.Any(t => t.coordinates.SequenceEqual(tempFive.coordinates)))
                        {
                            tempFive.type = d.type;
                            AiFives.Add(tempFive);
                            return -1000;
                        }
                    }
                }
            }
        }
        return 0;
    }   

    public int minimax(int depth, bool isPlayer, List<int> possibleMoves, int alpha, int beta)
    {
        int startEval = evalPosition();  //to jest callowane zarówno jako kopia minimaxa i wywołane po kliknięciu puto czyli trzeba zrobić tak żeby to było callowane albo oba prawda albo oba fałsz bo minimax musi być zcallowany jako fałsz żeby na powrocie na końcowej głębi dać fałsz
        if (depth == 0 || startEval >= 1500 || startEval <= -1500)
        {
            // string s = "";
            // foreach (int i in line)
            // {
            //     s += i + " ";
            // }
            // Debug.Log("linia: " + s + " ocena: " + startEval);
            return startEval;
        }
        List<int> possibleMovesChanged = possibleMoves.ToList();
        int bestMove = 0;
        if (isPlayer)
        {
            int evalHigh = -10000;
            int eval = 0;
            foreach (int move in possibleMovesChanged)
            {
                line.Add(move);
                // Debug.Log("ruch gracza: " + move);
                aiMoveCheck(move, isPlayer);
                List<int> moves = checkAllAdjecent(move);
                filterAllAdjacent(moves, true, true);
                moves.Sort();
                // Debug.Log("pola do okoła: ");
                // foreach (int i in moves)
                // {
                //     Debug.Log(i);
                // }
                List<int> diff = moves.Except(goodMoves).ToList();
                diff.Sort();
                // Debug.Log("roznica od wzorca: ");
                // foreach (int i in diff)
                // {
                //     Debug.Log(i);
                // }
                goodMoves = goodMoves.Union(diff).ToList();
                goodMoves.Sort();
                goodMoves.Remove(move);
                checkDuos(move, true);
                moveBestAtTop(goodMoves,false);
                eval = minimax(depth - 1, false, goodMoves, alpha, beta);
                line.Remove(move);
                foreach (int i in diff)
                {
                    goodMoves.Remove(i);
                }
                goodMoves.Add(move);
                takeBackFives(move, true);
                takeBackFours(move, true);
                takeBackTrios(move, true);
                takeBackDuos(move, true);
                takeBackMove(move, true);
                if (eval > evalHigh)
                {
                    bestMove = move;
                    evalHigh = eval;
                }
                if (eval > alpha)
                {
                    alpha = eval;
                }
                if (beta <= alpha)
                {
                    break;
                }
            }
            return evalHigh;
        }
        else
        {
            int evalHigh = 10000;
            int eval = 0;
            foreach (int move in possibleMovesChanged)
            {
                line.Add(move);
                // Debug.Log("ruch ai: " + move);
                aiMoveCheck(move, false);
                List<int> moves = checkAllAdjecent(move);
                filterAllAdjacent(moves, true, true);
                moves.Sort();
                // Debug.Log("pola do okoła: ");
                // foreach (int i in moves)
                // {
                //     Debug.Log(i);
                // }
                List<int> diff = moves.Except(goodMoves).ToList();
                diff.Sort();
                // Debug.Log("roznica od wzorca: ");
                // foreach (int i in diff)
                // {
                //     Debug.Log(i);
                // }
                goodMoves = goodMoves.Union(diff).ToList();
                goodMoves.Sort();
                goodMoves.Remove(move);
                moveBestAtTop(goodMoves,true);
                checkDuos(move, false);
                eval = minimax(depth - 1, true, goodMoves, alpha, beta);
                line.Remove(move);
                foreach (int i in diff)
                {
                    goodMoves.Remove(i);
                }
                goodMoves.Add(move);
                takeBackFives(move, false);
                takeBackFours(move, false);
                takeBackTrios(move, false);
                takeBackDuos(move, false);
                takeBackMove(move, false);
                if (eval < evalHigh)
                {
                    bestMove = move;
                    evalHigh = eval;
                }
                if (eval < beta)
                {
                    beta = eval;
                }
                if (beta <= alpha)
                {
                    break;
                }
            }
            if (depth == baseDepth)
            {
                // Debug.Log(bestMove);
                desc.text="ai ended thinking - move: "+bestMove;
                aiMove(bestMove);
            }
            return evalHigh;
        }
    }

    public void changeCurrentPosition(int pos, bool isPlayer)
    {
        if (isPlayer)
        {
            currentPosition.Os.Add(pos);
        }
        else
        {
            currentPosition.Xs.Add(pos);
        }
    }
    public void takeBackMove(int move, bool isPlayer)
    {
        if (isPlayer)
        {
            currentPosition.Os.Remove(move);
        }
        else
        {
            currentPosition.Xs.Remove(move);
        }
    }
    public void takeBackDuos(int move, bool isPlayer)
    {
        List<duos> toDel = new List<duos>();
        if (isPlayer)
        {
            foreach (duos d in PlayerDuos)
            {
                if (d.coordinates.Contains(move))
                {
                    toDel.Add(d);
                }
            }
            foreach (duos d in toDel)
            {
                PlayerDuos.Remove(d);
            }
        }
        else
        {
            foreach (duos d in AiDuos)
            {
                if (d.coordinates.Contains(move))
                {
                    toDel.Add(d);
                }
            }
            foreach (duos d in toDel)
            {
                AiDuos.Remove(d);
            }
        }
    }
    public void takeBackTrios(int move, bool isPlayer)
    {
        List<trios> toDel = new List<trios>();
        if (isPlayer)
        {
            foreach (trios t in PlayerTrios)
            {
                if (t.coordinates.Contains(move))
                {
                    toDel.Add(t);
                }
            }
            foreach (trios t in toDel)
            {
                PlayerTrios.Remove(t);
            }
        }
        else
        {
            foreach (trios t in AiTrios)
            {
                if (t.coordinates.Contains(move))
                {
                    toDel.Add(t);
                }
            }
            foreach (trios t in toDel)
            {
                AiTrios.Remove(t);
            }
        }
    }

    public void takeBackFours(int move, bool isPlayer)
    {
        List<Fours> toDel = new List<Fours>();
        if (isPlayer)
        {
            foreach (Fours t in PlayerFours)
            {
                if (t.coordinates.Contains(move))
                {
                    toDel.Add(t);
                }
            }
            foreach (Fours t in toDel)
            {
                PlayerFours.Remove(t);
            }
        }
        else
        {
            foreach (Fours t in AiFours)
            {
                if (t.coordinates.Contains(move))
                {
                    toDel.Add(t);
                }
            }
            foreach (Fours t in toDel)
            {
                AiFours.Remove(t);
            }
        }
    }
    public void takeBackFives(int move, bool isPlayer)
    {
        List<Fives> toDel = new List<Fives>();
        if (isPlayer)
        {
            foreach (Fives t in PlayerFives)
            {
                if (t.coordinates.Contains(move))
                {
                    toDel.Add(t);
                }
            }
            foreach (Fives t in toDel)
            {
                PlayerFives.Remove(t);
            }
        }
        else
        {
            foreach (Fives t in AiFives)
            {
                if (t.coordinates.Contains(move))
                {
                    toDel.Add(t);
                }
            }
            foreach (Fives t in toDel)
            {
                AiFives.Remove(t);
            }
        }
    }
    public int checkUnblockedPairs(bool isPlayer)
    {
        int tempEv = 0;
        if (isPlayer)
        {
            foreach (duos d in PlayerDuos)
            {
                if (!currentPosition.Xs.Contains(d.extended[0]) && FULLTABLE.Contains(d.extended[0]))
                {
                    tempEv++;
                }
                if (!currentPosition.Xs.Contains(d.extended[1]) && FULLTABLE.Contains(d.extended[1]))
                {
                    tempEv++;
                }
            }
        }
        if (!isPlayer)
        {
            foreach (duos d in AiDuos)
            {
                if (!currentPosition.Os.Contains(d.extended[0]) && FULLTABLE.Contains(d.extended[0]))
                {
                    tempEv++;
                }
                if (!currentPosition.Os.Contains(d.extended[1]) && FULLTABLE.Contains(d.extended[1]))
                {
                    tempEv++;
                }
            }
        }
        return tempEv;
    }
    public int checkUnblockedTrios(bool isPlayer)
    {
        int tempEv = 0;
        if (isPlayer)
        {
            foreach (trios d in PlayerTrios)
            {
                if (!currentPosition.Xs.Contains(d.extended[0]) && FULLTABLE.Contains(d.extended[0]))
                {
                    tempEv++;
                }
                if (!currentPosition.Xs.Contains(d.extended[1]) && FULLTABLE.Contains(d.extended[1]))
                {
                    tempEv++;
                }
            }
        }
        if (!isPlayer)
        {
            foreach (trios d in AiTrios)
            {
                if (!currentPosition.Os.Contains(d.extended[0]) && FULLTABLE.Contains(d.extended[0]))
                {
                    tempEv++;
                }
                if (!currentPosition.Os.Contains(d.extended[1]) && FULLTABLE.Contains(d.extended[1]))
                {
                    tempEv++;
                }
            }
        }
        return tempEv;
    }

    public int checkUnblockedFours(bool isPlayer)
    {
        int tempEv = 0;
        if (isPlayer)
        {
            foreach (Fours d in PlayerFours)
            {
                if (!currentPosition.Xs.Contains(d.extended[0]) && FULLTABLE.Contains(d.extended[0]))
                {
                    tempEv++;
                }
                if (!currentPosition.Xs.Contains(d.extended[1]) && FULLTABLE.Contains(d.extended[1]))
                {
                    tempEv++;
                }
            }
        }
        if (!isPlayer)
        {
            foreach (Fours d in AiFours)
            {
                if (!currentPosition.Os.Contains(d.extended[0]) && FULLTABLE.Contains(d.extended[0]))
                {
                    tempEv++;
                }
                if (!currentPosition.Os.Contains(d.extended[1]) && FULLTABLE.Contains(d.extended[1]))
                {
                    tempEv++;
                }
            }
        }
        return tempEv;
    }
    public int getTypeValue(string type)
    {
        if (type == "row")
        {
            return 1;
        }
        if (type == "c")
        {
            return 10;
        }
        if (type == "l")
        {
            return 11;
        }
        if (type == "r")
        {
            return 9;
        }
        return 0;
    }
    public void moveBestAtTop(List<int> list,bool isPlayer)
    {
        if(isPlayer)
        {
        foreach(Fours f in PlayerFours)
        {
            if(!currentPosition.Xs.Contains(f.extended[0]) && FULLTABLE.Contains(f.extended[0]) && !currentPosition.Os.Contains(f.extended[0]))
            {
                list.Remove(f.extended[0]);
                list.Insert(0,f.extended[0]);
            }
            if(!currentPosition.Xs.Contains(f.extended[1]) && FULLTABLE.Contains(f.extended[1]) && !currentPosition.Os.Contains(f.extended[1]))
            {
                list.Remove(f.extended[1]);
                list.Insert(0,f.extended[1]);
            }
        }
        foreach(trios t in PlayerTrios)
        {
            if(!currentPosition.Xs.Contains(t.extended[0]) && FULLTABLE.Contains(t.extended[0]) && !currentPosition.Os.Contains(t.extended[0]))
            {
                list.Remove(t.extended[0]);
                list.Insert(0,t.extended[0]);
            }
            if(!currentPosition.Xs.Contains(t.extended[1]) && FULLTABLE.Contains(t.extended[1]) && !currentPosition.Os.Contains(t.extended[1]))
            {
                list.Remove(t.extended[1]);
                list.Insert(0,t.extended[1]);
            }
        }
        }else
        {
            foreach(Fours f in AiFours)
        {
            if(!currentPosition.Xs.Contains(f.extended[0]) && FULLTABLE.Contains(f.extended[0]) && !currentPosition.Os.Contains(f.extended[0]))
            {
                list.Remove(f.extended[0]);
                list.Insert(0,f.extended[0]);
            }
            if(!currentPosition.Xs.Contains(f.extended[1]) && FULLTABLE.Contains(f.extended[1]) && !currentPosition.Os.Contains(f.extended[1]))
            {
                list.Remove(f.extended[1]);
                list.Insert(0,f.extended[1]);
            }
        }
        foreach(trios t in AiTrios)
        {
            if(!currentPosition.Xs.Contains(t.extended[0]) && FULLTABLE.Contains(t.extended[0]) && !currentPosition.Os.Contains(t.extended[0]))
            {
                list.Remove(t.extended[0]);
                list.Insert(0,t.extended[0]);
            }
            if(!currentPosition.Xs.Contains(t.extended[1]) && FULLTABLE.Contains(t.extended[1]) && !currentPosition.Os.Contains(t.extended[1]))
            {
                list.Remove(t.extended[1]);
                list.Insert(0,t.extended[1]);
            }
        }
        }
    }
}

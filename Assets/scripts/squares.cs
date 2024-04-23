using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class squares : MonoBehaviour
{
    // Start is called before the first frame update
    private int field;
    public bool isUsed;
    public Manager manager;
    [SerializeField] private Sprite squareX;
    [SerializeField] private Sprite squareO; 
    void Start()
    {
        manager=GameObject.Find("GameManager").GetComponent<Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void changeFieldName(int val)
    {
        field=val;
    }
    private void OnMouseDown() {
        putO();
    }
    public void putO()
    {
        if(!isUsed && manager.win==0)
        {
            isUsed=true;
            GetComponent<SpriteRenderer>().sprite=squareO;
            manager.changeCurrentPosition(field,true); 
            List<int> temp = manager.checkAllAdjecent(field);
            manager.filterAllAdjacent(temp,true,true);
            manager.goodMoves=manager.goodMoves.Union(temp).ToList();
            manager.goodMoves.Remove(field);
            manager.goodMoves.Sort();
            if(manager.checkDuos(field,true)==1000)  //to dodaje pary do og tablicy po ruchu a przy tym sprawdza wygraną
            {
                Debug.Log("gracz wygrał");
                manager.desc.text="player won";
                manager.win=1;
            } 
            if(manager.win==0)
            {
                StartCoroutine("coroutine");
            }
            
        }
    }
    public void putX()
    {
        if(!isUsed && manager.win==0)
        {
            isUsed=true;
            GetComponent<SpriteRenderer>().sprite=squareX;
            manager.changeCurrentPosition(field,false);
            List<int> temp = manager.checkAllAdjecent(field);
            manager.filterAllAdjacent(temp,true,true);
            manager.goodMoves=manager.goodMoves.Union(temp).ToList();
            manager.goodMoves.Remove(field);
            manager.goodMoves.Sort();
            if(manager.checkDuos(field,false)==-1000)       //to dodaje pary do og tablicy po ruchu a przy tym sprawdza wygraną
            {
                Debug.Log("ai wygrał");
                manager.desc.text="ai won";
                manager.win=-1;
            }
            if(manager.goodMoves.Count==0)
            {
                Debug.Log("remis");
                manager.desc.text="draw";
            }
        }
    }   
    
    public int getFieldName()
    {
        return field;
    }
    public IEnumerator coroutine()
    {
        manager.desc.text="ai thinks...";
        yield return null;
        manager.evalText.text=manager.minimax(manager.baseDepth,false,manager.goodMoves,-10000,10000).ToString();
    }
}

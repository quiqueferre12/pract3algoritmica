using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];//array de objetos tiles (todas las casillas)
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;
                    
    void Start()
    {        
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }
        
    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;            

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;                
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();                         
            }
        }
                
        cops[0].GetComponent<CopMove>().currentTile=Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile=Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile=Constants.InitialRobber;           
    }

    public void InitAdjacencyLists()
    {
        //TODO: Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes

        for (int i = 0; i < Constants.NumTiles; i = i + 8)//recorremos todas las casillas a cada avance de aqui se le sumará 8 para subir a la siguiente fila
        {
            for (int j = i; j < Constants.TilesPerRow + i; j++)//empezamos por las filas ej: int j=8 j< 8+ 8=i
            {
                if (j != i)//si es 0,8,16,24 (si no esta a la izquierda del todo )
                {

                    tiles[j].adjacency.Add(j - 1);//anyade el adyacente de la izquierda
                }
                if (j != Constants.TilesPerRow + i - 1)//mientras no llegue a la derecha
                {
                    
                    tiles[j].adjacency.Add(j + 1);//anyade la casilla adiacente de la derecha
                }
                if (i == 0)//si esta bajo del todo
                {

                    tiles[j].adjacency.Add(j + 8);// solo se anyade la de arriba
                }
                else if (i == 56)//si se encuentra arriba del todo 
                {
                    tiles[j].adjacency.Add(j - 8);//se anyade solo la adiaciencia de abajo
                }
                else if (i != 0 && i != 56)//si no está ni en la primera ni la ultima fila
                {
                    tiles[j].adjacency.Add(j - 8);//anyadimos una adiaciencia abajo
                    tiles[j].adjacency.Add(j + 8);//anyadimos una adiaciencia arriba
                    
                }
            }
        }

    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {        
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {                     
        clickedTile = t;

        switch (state)
        {            
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {                  
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile=tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;   
                    
                    state = Constants.TileSelected;
                }                
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        /*TODO: Cambia el código de abajo para hacer lo siguiente
        - Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
        - Movemos al caco a esa casilla
        - Actualizamos la variable currentTile del caco a la nueva casilla
        */
        List<Tile> selectT = new List<Tile>();
        foreach(Tile t in tiles)
        {
            if (t.selectable)
            {
                selectT.Add(t);//añado el numero de la casilla seleccionable
                //Debug.Log(t.numTile);//comprovacion de las casillas que aparecen del rober
            }
        }
            int siguienteT = Random.Range(0, selectT.Count); // selecciono una posicion del array
            robber.GetComponent<RobberMove>().MoveToTile(selectT[siguienteT]); // Lo muevo al numero de la casilla selectable[nextTile] = numero de la casilla
            robber.GetComponent<RobberMove>().currentTile = selectT[siguienteT].numTile;
        


        //robber.GetComponent<RobberMove>().MoveToTile(tiles[robber.GetComponent<RobberMove>().currentTile]); esta te mueve al centro no hace falta hacerla
    }

    public void EndGame(bool end)
    {
        if(end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {
                 
        int indexcurrentTile;//la casilla        
        if (cop == true)
        {//comprobamos si es policia
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;//entra en el de policia 
        }
        else
        {
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;//despues de mover el policia se mueve el robber, por eso comparten turno y comparten el metodo indexcurrentfile
        }
        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;
        tiles[indexcurrentTile].parent = null;//ponemos el parent en nulo al cambiar el turno
        tiles[indexcurrentTile].distance = 0;//la distancia la asignamos a 0 para volver a hacer las diferencias de 1 a 2 como maximo
        tiles[indexcurrentTile].visited = true;//la distancia la visualización se pone en visible
        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();
        Tile casilla = null;//creamos un objeto casilla null para bfs

        nodes.Enqueue(tiles[indexcurrentTile]);//de lista de nodes ponemos en la cola la casilla en la que estas
        do
        {
            casilla = nodes.Dequeue();//la quitamos de la cola guarda en una variable auxiliar lo que se va a examinar
            /*Debug.Log(casilla.numTile);*/
            if(casilla.numTile != cops[clickedCop].GetComponent<CopMove>().currentTile)//para que al seleccionarlo no cuente como una ronda mas una vez le hagas click
            {
                casilla.selectable = true;
            }
           
            if ((casilla.distance + 1) <= Constants.Distance)//comprobamos si la distancia de la casilla de adiacientes es 1 o 2 
            {
                
                foreach (int i in casilla.adjacency)//recorremos las adiacentes de la casilla seleccionada
                {
                    
                    if (tiles[i].visited == false && tiles[i].numTile != cops[0].GetComponent<CopMove>().currentTile && tiles[i].numTile != cops[1].GetComponent<CopMove>().currentTile)
                    {
                        tiles[i].parent = casilla;//le asignamos a el padre de la casilla adiacente para que siga el transcurso del movimiento en forma de L y no diagonales
                        tiles[i].distance = casilla.distance + 1;//pillaria la distancia del padre y sumaria la distancia correspondiente para poder moverse
                        tiles[i].visited = true;//ya ha pasado por ahí a cada turno se resetea
                        nodes.Enqueue(tiles[i]);//encola las casillas seleccionables
                    }


                }
            }
        } while (nodes.Count != 0);//mientras haya casillas en la cola

    }
    







}

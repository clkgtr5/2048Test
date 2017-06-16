using UnityEngine;
using System.Collections;
using FairyGUI;
using System.Collections.Generic;
using DG.Tweening;
using System.Threading;


public class gameLogic : MonoBehaviour
{
    
    private GComponent _mainView;
    //Transform _mainPanel;
    GObject[] _blocks;
    GObject _curScore;
    GObject _BestScore;
    GObject _animation;
    GObject[] _aniBlocks;
    Vector3[] _blocksPositions;
       
    private int gameLines; // 默认为4
    private int[][] gameMatrix;
    private int[][] gameMatrixHistory;
    private int keyItemNum;
    private ArrayList calList;
    private int minSwipeDistance;
    private Configuration Config;
    private int scoreHistory;
    private bool canControll = false;
    private bool isGameOver = false;

    void Awake()
    {
        DOTween.useSafeMode = false;
        Application.targetFrameRate = 60;
        //Stage.inst.onKeyDown.Add(OnkeyDown);
        gameLines = 4;
        keyItemNum = -1;
        _blocks = new GObject[gameLines * gameLines];
        calList = new ArrayList();
        _aniBlocks = new GObject[gameLines * gameLines];
        _blocksPositions = new Vector3[gameLines * gameLines];
        minSwipeDistance = 10; //  x pt的防误触距离。
        Config = new Configuration();
        scoreHistory = 0;
        Config.Score = 0;
        
        gameMatrix = new int[gameLines][];
        for (int i = 0; i < gameLines; i++)     // gameMatrix 2维数组定义。
        {
            gameMatrix[i] = new int[gameLines];
        }

        gameMatrixHistory = new int[gameLines][];
        for (int i = 0; i < gameLines; i++)     // gameMatrixhistory 2维数组定义。
        {
            gameMatrixHistory[i] = new int[gameLines];
        }

        reStart();
        UIPackage.AddPackage("UI/Gesture");
    }


    // Use this for initialization
    void Start()
    {

        GRoot.inst.SetContentScaleFactor(410, 700);
        UIPackage.AddPackage("UI/2048_pkg");
        _mainView = UIPackage.CreateObject("2048_pkg", "mainpanel").asCom;
        GRoot.inst.AddChild(_mainView);

        for (int i = 0; i < gameLines * gameLines; i++)
        {
            _blocks[i] = _mainView.GetChild("l" + (i + 1).ToString());  //获取contianers l1 ~ l16
        }

        _curScore =  _mainView.GetChild("CurScore").asCom.GetChild("CS");  

        _BestScore = _mainView.GetChild("BestScore").asCom.GetChild("BS");

       // _animation = _mainView.GetChild("Animation");

        for (int i = 0; i < gameLines * gameLines; i++)
        {
            _aniBlocks[i] = _mainView.GetChild("Animation").asCom.GetChild("a" + (i + 1).ToString());  //获取动画 contianers a1 ~ a16
            _blocksPositions[i] = _aniBlocks[i].position;
        }


        GObject holder = _mainView.GetChild("Control");   //获得触摸区域组件

        _curScore.asTextField.text = Config.Score.ToString(); //分数设置

        loadBest();
     
        _mainView.GetChild("BtnNewGame").onClick.Add(() =>   //新的游戏点击事件
        {
            reStart();
            rearranage();
            loadBest();
        });
        GComponent menuPanel = null;

        _mainView.GetChild("MenuBtn").onClick.Add(() => 
        {
            Debug.LogWarning("MenuBtn");

            menuPanel = UIPackage.CreateObject("2048_pkg", "MenuPanel").asCom;

            Debug.LogWarning( menuPanel.asCom.name);

            GRoot.inst.AddChild(menuPanel);

            menuPanel.asCom.GetChild("ResumeBtn").onClick.Add(() =>
            {
                menuPanel.RemoveFromParent();

            });

            menuPanel.asCom.GetChild("RestartBtn").onClick.Add(() =>
            {
                reStart();
                rearranage();
                loadBest();
                menuPanel.RemoveFromParent();
            });

        });


        

       


        rearranage(); //显示初始化矩阵
      
        SwipeGesture gesture1 = new SwipeGesture(holder);
       
        //gesture1.onMove.Add(OnSwipeMove);
        gesture1.onEnd.Add(OnSwipeEnd);

        canControll = true;
       
    }

    private void Update()
    {        
           if(canControll) 
                KeyBoardUpdate();
    }

    void KeyBoardUpdate()
    {

        //GObjectPool p = new GObjectPool(this.transform);
        //p.GetObject("");
        //p.ReturnObject(_animation);

        saveHistoryMatrix();
        if (Input.GetKey(KeyCode.UpArrow))
        {
            StartCoroutine(swipeUp());
            Debug.LogError("UP KeyboardUpdated");
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            StartCoroutine( swipeLeft());
            Debug.LogError("Left KeyboardUpdated");
            
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {

            StartCoroutine(swipeRight());
            Debug.LogError("Right KeyboardUpdated");
           

        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            StartCoroutine(swipeDown());
            Debug.LogError("Down KeyboardUpdated");
        
        }
        if (isMoved())
        { 
            StartCoroutine( GameUpdate());
        }

        if (gameOver() == 0)
        {
            saveBest();
            Debug.Log("Game Over");
            //Application.Quit();
            canControll = false;
        }

        else if (gameOver() == 2)
            Debug.Log("Wondeful!");

    }

    private IEnumerator GameUpdate()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(Configuration.TimeForAni * 0.6f);
        yield return new WaitForEndOfFrame();
        //Update 游戏数据。

        getScroe();
        occur();
            //rearranage();
    }

    void OnSwipeEnd(EventContext context)
    {
       
        SwipeGesture gesture = (SwipeGesture)context.sender;

        Vector3 v = new Vector3();
        // Debug.Log("Delat X: " + gesture.delta.x + "Delat Y: " + gesture.delta.y);
        //saveHistoryMatrix();

        if (canControll)
        {
                if (Mathf.Abs(gesture.velocity.x) > Mathf.Abs(gesture.velocity.y)) //delta 是滑动的距离 velocity 滑动速度。
                {
                    v.x = Mathf.Round(Mathf.Sign(gesture.velocity.x) * Mathf.Sqrt(Mathf.Abs(gesture.velocity.x)));
            
                    if (Mathf.Abs(v.x) < minSwipeDistance) //消除手抖的影响
                        return;
           
                    if (v.x < 0)        //swipeleft
                    {
                        StartCoroutine(swipeLeft());
                    }
                    else if (v.x > 0)       //swiperight has bug
                    {
                        StartCoroutine(swipeRight()); 
              
                    }
                }
                else
                {
                    v.y = Mathf.Round(Mathf.Sign(gesture.velocity.y) * Mathf.Sqrt(Mathf.Abs(gesture.velocity.y)));
                    if (Mathf.Abs(v.y) < minSwipeDistance)
                        return ;

                    if (v.y < 0)        // swipeup
                    {
                        StartCoroutine(swipeUp());
               
                    }
                    else if (v.y > 0)       // siwpedown
                    {
                        StartCoroutine(swipeDown());

                    }         
                }
        
                if (isMoved())          //Update 游戏数据。
                {
                    StartCoroutine(GameUpdate());
                }
                if (gameOver() == 0)
                {
                    saveBest();
                    Debug.Log("Game Over");
                    //Application.Quit();
                }
                else if (gameOver() == 2)
                {
                    Debug.Log("Wondeful!");
                }
                
         }
    }
    
    void swipeXAni(int row, List<int[]> mergePos)
    {
        
        //Debug.Log("row: " + (row+1) + " col: " + (col+1) + " Endcol: " + (endCol+1));
        for (int i = 0;i < mergePos.Count ;i++)
        {
          
            int col = mergePos[i][1];
            int endCol = mergePos[i][0];
            Vector3 oriPos = _blocksPositions[row * 4 + col];
            
            bool mergeAni = false;
            if (mergePos[i].Length == 3)
                mergeAni = true;
            //Mathf.Abs(((endCol * 100 + 10) - (col * 100 + 10)) / Configuration.Speed)

            if (col != endCol)
            {
                Debug.Log("row: " + (row + 1) + " col: " + (col + 1) + " Endcol: " + (endCol + 1));
                _aniBlocks[row * 4 + col].parent.SetChildIndex(_aniBlocks[row * 4 + col], _aniBlocks[row * 4 + col].parent.numChildren - 1);

                _aniBlocks[row * 4 + col].TweenMoveX(endCol * 100 + 10, Configuration.TimeForAni).OnComplete(() =>
                {
                    _aniBlocks[row * 4 + col].asLoader.visible = false;
                    _aniBlocks[row * 4 + col].asLoader.url = " ";
                    _aniBlocks[row * 4 + col].SetPosition(oriPos.x, oriPos.y, oriPos.z);  //直接设定坐标容易造成错位。
                    //_aniBlocks[row * 4 + col].TweenMoveX(col * 100 + 10, 0.0002f);  //造成闪烁。
                    _aniBlocks[row * 4 + col].asLoader.visible = true;
                    rearranage();
                });
                if (mergeAni == true)
                {
                    _aniBlocks[row * 4 + endCol].TweenScale(new Vector2(1.2f, 1.2f), Configuration.TimeForAni * 0.8f).OnComplete(() =>
                     {
                         _aniBlocks[row * 4 + endCol].TweenScale(new Vector2(1f, 1f), Configuration.TimeForAni * 0.8f);

                     });
                }
            }
            else
            {
                continue;
            }
        }

        
    }

    void swipeYAni(int col, List<int[]> mergePos)
    {
        for (int i = 0; i < mergePos.Count; i++)
        {
            
            int row = mergePos[i][1];
            int endRow = mergePos[i][0];
            Vector3 oriPos = _blocksPositions[row * 4 + col];

            bool mergeAni = false;
            if (mergePos[i].Length == 3)
                mergeAni = true;

            if (row != endRow)
            {
                Debug.Log("col: " + (col + 1) + " row: " + (row + 1) + " EndRow: " + (endRow + 1));


                _aniBlocks[row * 4 + col].parent.SetChildIndex(_aniBlocks[row * 4 + col], _aniBlocks[row * 4 + col].parent.numChildren - 1);
                _aniBlocks[row * 4 + col].TweenMoveY(endRow * 100 + 10, Configuration.TimeForAni).OnComplete(() =>
                {
                    _aniBlocks[row * 4 + col].asLoader.visible = false;
                    _aniBlocks[row * 4 + col].asLoader.url = " ";
                    _aniBlocks[row * 4 + col].SetPosition(oriPos.x, oriPos.y, oriPos.z);  //直接设定坐标容易造成错位。
                    //_aniBlocks[row * 4 + col].TweenMoveY(row * 100 + 10, 0.0002f);
                    _aniBlocks[row * 4 + col].asLoader.visible = true;
                    rearranage();
                });
                if (mergeAni == true)
                {
                    _aniBlocks[endRow * 4 + col].TweenScale(new Vector2(1.2f, 1.2f), Configuration.TimeForAni * 0.8f).OnComplete(() =>
                    {
                        _aniBlocks[endRow * 4 + col].TweenScale(new Vector2(1f, 1f), Configuration.TimeForAni * 0.8f);

                    });
                }
            }
            else
            {
                continue;
            }

        }
    }

    IEnumerator swipeLeft()
    {   
        canControll = false;
       
        for (int i = 0; i < gameLines; i++)
        {
       
            List<int[]> mergePos = new List<int[]>();

            for (int j = 0; j < gameLines; j++)
            {
                int currentNum = gameMatrix[i][j];
                if (currentNum != 0)
                {
                    if (keyItemNum == -1)
                    {
                        int[] temp = new int[2];        //获取移动前后位置。
                        temp[0] = calList.Count;
                        temp[1] = j;
                        mergePos.Add(temp);

                        keyItemNum = currentNum;
                        
                    }
                    else
                    {
                        if (keyItemNum == currentNum)
                        {
                            int[] temp = new int[3];
                            temp[0] = calList.Count;
                            temp[1] = j;
                            temp[2] = -1;
                            mergePos.Add(temp);

                            calList.Add(keyItemNum * 2);
                            Config.Score += keyItemNum * 2;
                            keyItemNum = -1;                         
                        }
                        else
                        { 
                            calList.Add(keyItemNum);
                            keyItemNum = currentNum;

                            int[] temp = new int[2];
                            temp[0] = calList.Count;
                            temp[1] = j;
                            mergePos.Add(temp);
                           
                        }
                    }
                }
                else
                {
                    continue;
                }
            }
            if (keyItemNum != -1)
            {
                calList.Add(keyItemNum);
              
            }
            // 改变Item值  
            for (int j = 0; j < calList.Count; j++)
            {
                gameMatrix[i][j] = (int)calList[j];

                // Debug.Log("SL chItem   i:" + i + " j: " + j);
            }

            for (int m = calList.Count; m < gameLines; m++)
            {
                //print(m);
                gameMatrix[i][m] = 0;
            }

            //执行动画
            swipeXAni(i, mergePos);
            
            // 重置行参数  
            keyItemNum = -1;
            calList = new ArrayList();
            
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(Configuration.TimeForAni*1f);
        yield return new WaitForEndOfFrame();
      
        canControll = true;
    }

    IEnumerator swipeRight()
    {
        canControll = false;

        float time = Configuration.TimeForAni;

        for (int i = 0; i < gameLines; i++)
        {
           
            List<int[]> mergePos = new List<int[]>();

            for (int j = gameLines-1; j >=0; j--)
            {
                int currentNum = gameMatrix[i][j];
                if (currentNum != 0)
                {
                    if (keyItemNum == -1)
                    {
                        int[] temp = new int[2];        //获取移动前后位置。
                        temp[0] = 3 - calList.Count;
                        temp[1] = j;
                        mergePos.Add(temp);

                        keyItemNum = currentNum;
                        
                    }
                    else
                    {
                        if (keyItemNum == currentNum)
                        {
                            int[] temp = new int[3];        
                            temp[0] = 3 - calList.Count;
                            temp[1] = j;
                            temp[2] = -1;
                            mergePos.Add(temp);

                            calList.Add(keyItemNum * 2);
                            Config.Score += keyItemNum * 2;
                            keyItemNum = -1;
                           
                            
                        }
                        else
                        {
                            calList.Add(keyItemNum);
                            keyItemNum = currentNum;

                            int[] temp = new int[2];        
                            temp[0] = 3 - calList.Count;
                            temp[1] = j;
                            mergePos.Add(temp);
                        }
                    }
                }
                else
                {
                    continue;
                }
            }
            if (keyItemNum != -1)
            {
                calList.Add(keyItemNum);              
            }



            // 改变Item值  
            for (int j = gameLines-1,a = 0; j >= gameLines-calList.Count && a < calList.Count; j--,a++)
            {
                gameMatrix[i][j] = (int)calList[a];
            }

            
            for (int m = 0; m <gameLines - calList.Count; m++)
            {
                //print(m);
                gameMatrix[i][m] = 0;
            }
            // 重置行参数  

            //动画实现
            swipeXAni(i, mergePos);

            keyItemNum = -1;
            calList = new ArrayList();
        }

        while (time > 0)
        {
            yield return new WaitForEndOfFrame();
            time -= Time.deltaTime;
            if (time < Time.deltaTime)
                break;
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(Configuration.TimeForAni*0.8f);
        yield return new WaitForEndOfFrame();

        canControll = true;
    }

    IEnumerator swipeUp()
    {
        canControll = false;

        for (int i = 0; i < gameLines; i++)
        {
            List<int[]> mergePos = new List<int[]>();

            for (int j = 0; j < gameLines; j++)
            {
                int currentNum = gameMatrix[j][i];
                if (currentNum != 0)
                {
                    if (keyItemNum == -1)
                    {
                        keyItemNum = currentNum;

                        int[] temp = new int[2];
                        temp[0] = calList.Count;
                        temp[1] = j;
                        mergePos.Add(temp);
                    }
                    else
                    {
                        if (keyItemNum == currentNum)
                        {
                            int[] temp = new int[3];
                            temp[0] = calList.Count;
                            temp[1] = j;
                            temp[2] = -1;
                            mergePos.Add(temp);

                            calList.Add(keyItemNum * 2);
                            Config.Score += keyItemNum * 2;
                            keyItemNum = -1;
                           
                        }
                        else
                        {
                            calList.Add(keyItemNum);
                            keyItemNum = currentNum;

                            int[] temp = new int[2];
                            temp[0] = calList.Count;
                            temp[1] = j;
                            mergePos.Add(temp);
                        }
                    }
                }
                else
                {
                    continue;
                }
            }
            if (keyItemNum != -1)
            {
                calList.Add(keyItemNum);               
            }
            // 改变Item值  
            for (int j = 0; j < calList.Count; j++)
            {
                gameMatrix[j][i] = (int)calList[j];

                
            }

           
            

            for (int m = calList.Count; m < gameLines; m++)
            {
                //print(m);
                gameMatrix[m][i] = 0;
            }

            //动画实现
            swipeYAni(i, mergePos);

            // 重置行参数  

            keyItemNum = -1;
            calList = new ArrayList();
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(Configuration.TimeForAni*0.8f);
        yield return new WaitForEndOfFrame();

        canControll = true;

    }

    IEnumerator swipeDown()
    {
        canControll = false;

        for (int i = 0; i < gameLines; i++)
        {       
            List<int[]> mergePos = new List<int[]>();

            for (int j = gameLines - 1; j >= 0; j--)
            {
                int currentNum = gameMatrix[j][i];
                if (currentNum != 0)
                {
                    if (keyItemNum == -1)
                    {
                        keyItemNum = currentNum;

                        int[] temp = new int[2];
                        temp[0] = 3 - calList.Count;
                        temp[1] = j;
                        mergePos.Add(temp);
                    }
                    else
                    {
                        if (keyItemNum == currentNum)
                        {

                            int[] temp = new int[3];
                            temp[0] = 3 - calList.Count;
                            temp[1] = j;
                            temp[2] = -1;
                            mergePos.Add(temp);

                            calList.Add(keyItemNum * 2);
                            Config.Score += keyItemNum * 2;
                            keyItemNum = -1;
                        }
                        else
                        {
                            calList.Add(keyItemNum);
                            keyItemNum = currentNum;

                            int[] temp = new int[2];
                            temp[0] = 3 - calList.Count;
                            temp[1] = j;
                            mergePos.Add(temp);
                        }
                    }
                }
                else
                {
                    continue;
                }
            }
            if (keyItemNum != -1)
            {
                calList.Add(keyItemNum);
            }
            // 改变Item值  
            for (int j = gameLines - 1, a = 0; j >= gameLines - calList.Count && a < calList.Count; j--, a++)
            {
                gameMatrix[j][i] = (int)calList[a];
                
            }
           
             
            for (int m = 0; m < gameLines - calList.Count; m++)
            {
                //print(m);
                gameMatrix[m][i] = 0;
            }

            //动画实现
            swipeYAni(i, mergePos);

            // 重置行参数  

            keyItemNum = -1;
            calList = new ArrayList();
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(Configuration.TimeForAni*0.8f);
        yield return new WaitForEndOfFrame();

        canControll = true;

    }

    // 保存历史记录 
    private void saveHistoryMatrix()
    {
        scoreHistory = Config.Score;
        for (int i = 0; i < gameLines; i++)
        {
            for (int j = 0; j < gameLines; j++)
            {
                gameMatrixHistory[i][j] = gameMatrix[i][j];
            }
        }
    }

    void rearranage()
    {
        for (int i = 0; i < gameLines; i++)
        {
            for (int j = 0; j < gameLines; j++)
            {


                _blocks[i * 4 + j].asLoader.url = "ui://v8anhwugum7di";

                if (gameMatrix[i][j] == 0)
                    _aniBlocks[i * 4 + j].asLoader.url = " ";
                else if (gameMatrix[i][j] == 2)
                    _aniBlocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchh6";
                else if (gameMatrix[i][j] == 4)
                    _aniBlocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchh7";
                else if (gameMatrix[i][j] == 8)
                    _aniBlocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchh8";
                else if (gameMatrix[i][j] == 16)
                    _aniBlocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchh9";
                else if (gameMatrix[i][j] == 32)
                    _aniBlocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchha";
                else if (gameMatrix[i][j] == 64)
                    _aniBlocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchhb";
                else if (gameMatrix[i][j] == 128)
                    _aniBlocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchhc";
                else if (gameMatrix[i][j] == 256)
                    _aniBlocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchhd";
                else if (gameMatrix[i][j] == 512)
                    _aniBlocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchhe";
                else if (gameMatrix[i][j] == 1024)
                    _aniBlocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchhf";
                else if (gameMatrix[i][j] == 2048)
                    _aniBlocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchhg"; //ui://v8anhwugkchhg 


                //if (gameMatrix[i][j] == 0)
                //    _blocks[i * 4 + j].asLoader.url = "ui://v8anhwugum7di";
                //else if (gameMatrix[i][j] == 2)
                //    _blocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchh6";
                //else if (gameMatrix[i][j] == 4)
                //    _blocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchh7";
                //else if (gameMatrix[i][j] == 8)
                //    _blocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchh8";
                //else if (gameMatrix[i][j] == 16)
                //    _blocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchh9";
                //else if (gameMatrix[i][j] == 32)
                //    _blocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchha";
                //else if (gameMatrix[i][j] == 64)
                //    _blocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchhb";
                //else if (gameMatrix[i][j] == 128)
                //    _blocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchhc";
                //else if (gameMatrix[i][j] == 256)
                //    _blocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchhd";
                //else if (gameMatrix[i][j] == 512)
                //    _blocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchhe";
                //else if (gameMatrix[i][j] == 1024)
                //    _blocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchhf";
                //else if (gameMatrix[i][j] == 2048)
                //    _blocks[i * 4 + j].asLoader.url = "ui://v8anhwugkchhg"; //ui://v8anhwugkchhg 


                //else if (gameMatrix[i][j] == 4096)
                //    _blocks[i * 4 + j].asLoader.url = "ui://v8anhwugqjzaj"; //ui://v8anhwugqjzaj

            }
        }
    }

    void occur() {
        List<int[]> ranList = new List<int[]>();
        
        int ranPos = 0;
        int ranValue = 0;
        for (int i = 0; i < gameLines; i++)
        {
            
            for (int j = 0; j < gameLines; j++)  //遍历储存空白位置，用于随机生成新的方块。
            {    if (gameMatrix[i][j] == 0)
                {
                    int[] temp = { 0, 0 };
                    temp[0] = i;
                    temp[1] = j;
                    ranList.Add(temp);
                       
                }
            }
        }
        ranPos = (int)Random.Range(0, ranList.Count);
     
        ranValue = (int)Random.Range(0, 6);

        int row, col = 0;

        row = ranList[ranPos][0];

        col = ranList[ranPos][1];

        if (ranList.Count != 0)
        {
            if (ranValue < 5)
            {                                       //随机生成方块，4：20% 2： 80%
                gameMatrix[row][col] = 2;           //"ui://v8anhwugkchh6"
                occurAin("ui://v8anhwugkchh6",row,col);

            }
            else
            {
                gameMatrix[row][col] = 4;
                occurAin("ui://v8anhwugkchh7", row, col);
            }
        }
       
    }

    void occurAin(string URL,int row,int col)
    {
        
        _aniBlocks[row * 4 + col].visible = false;
        _aniBlocks[row * 4 + col].asLoader.url = URL;
        _aniBlocks[row * 4 + col].visible = true;
        _aniBlocks[row * 4 + col].TweenScale(new Vector2(0.6f, 0.6f), Configuration.TimeForAni*0.8f).OnComplete(() =>
        {
            _aniBlocks[row * 4 + col].TweenScale(new Vector2(1, 1), Configuration.TimeForAni * 0.8f);
        });
       
        
    }

    void reStart() {


        
        int i = 0;
        int j = 0;
        int count = 0;

        Config.Score = 0;

        for (int k = 0; k < gameLines; k++)
        {
            for (int m = 0; m < gameLines; m++)
            {
                gameMatrix[k][m] = 0;
            }

        }
        for (int k = 0; k < 16; k++)
        {   
            i = (int)Random.Range(0, 4);
            j = (int)Random.Range(0, 4);
            if (gameMatrix[i][j] == 0)
            {
                gameMatrix[i][j] = 2;
                count++;
            }
            if (count == 3)
            {
                int ran = Random.Range(0, 4);
                if(ran > 2)
                    gameMatrix[i][j] = 4;
                else
                    gameMatrix[i][j] = 2;
                break;
            }
            
        }
       
    }

    void showGameMatrix()
    {
        for (int i = 0; i < gameLines; i++)
        {

            for (int j = 0; j < gameLines; j++)
            {

               print("i: " + i + " j: " + j + " " + gameMatrix[i][j]);
            }
          
        }
    }

    //是否移动
    private bool isMoved()
    {
        for (int i = 0; i < gameLines; i++)
        {
            for (int j = 0; j < gameLines; j++)
            {
                if (gameMatrixHistory[i][j] != gameMatrix[i][j])
                {
                    return true;
                }
            }
        }
        return false;
    }

    public int gameOver()
    {
        int Blanks = 0;
        for (int i = 0; i < gameLines; i++)
        {
            for (int j = 0; j < gameLines; j++)  
            {
                if (gameMatrix[i][j] == 0)
                {
                    Blanks++;                   
                }
            }
        }
        if (Blanks == 0)
        {
            for (int i = 0; i < gameLines; i++)
            {
                for (int j = 0; j < gameLines; j++)
                {
                    if (j < gameLines - 1)
                    {
                        if (gameMatrix[i][j] == gameMatrix[i][j + 1])
                        {
                            return 1;
                        }
                    }
                    if (i < gameLines - 1)
                    {
                        if (gameMatrix[i][j] == gameMatrix[i + 1][j])
                        {
                            return 1;
                        }
                    }
                }
            }

            return 0;
        }
        for (int i = 0; i < gameLines; i++)
        {
            for (int j = 0; j < gameLines; j++)
            {
                if (gameMatrix[i][j] == 2048)
                {
                    return 2;
                }
            }
        }
        return 1;
    }

    //显示分数
    void getScroe()
    {
        _curScore.asTextField.text = Config.Score.ToString();
    }

    //保存最高分
    void saveBest()
    {
        int Cs = 0;
        int Bs = 0;

        Cs = int.Parse(_curScore.asTextField.text.ToString());

        Bs = int.Parse(_BestScore.asTextField.text.ToString());

        if (Cs > Bs)
        {
            PlayerPrefs.SetInt("BestScore", Cs);
        }
    }

    //载入最高分
    void loadBest()
    {
        if (PlayerPrefs.HasKey("BestScore"))
        {
            _BestScore.asTextField.text = PlayerPrefs.GetInt("BestScore", 0).ToString();
        }
    }

}


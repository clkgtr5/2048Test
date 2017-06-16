using UnityEngine;
using System.Collections;
using FairyGUI;


public class Configuration
{

    public static float Speed = 50.0f;

    public static float TimeForAni = 0.6f;

    private int _score;

   

    public int Score
    { 
        get
        {
            return _score;
        }

        set
        {
            _score = value;
        }
    }

   

}

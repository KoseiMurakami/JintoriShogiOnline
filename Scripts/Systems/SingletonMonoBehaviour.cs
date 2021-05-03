/*#**************************************************************************#*/
/*#    SingletonMonoBehaviour.cs                                             #*/
/*#                                                                          #*/
/*#    Summary    :    シーンに一つしかないスクリプトには簡単にアクセスでき  #*/
/*#                    るようにする                                          #*/
/*#                                                                          #*/
/*#    How        :    1. シングルトンにしたいスクリプトに                   #*/
/*#                       SingletonMonoBehaviourを継承する                   #*/
/*#                    2. クラス名.Instance.メンバーで呼ぶことができる       #*/
/*#                                                                          #*/
/*#**************************************************************************#*/
using UnityEngine;
using System;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                Type t = typeof(T);

                instance = (T)FindObjectOfType(t);
                if (instance == null)
                {
                    Debug.LogError(t + " をアタッチしているGameObjectはありません。");
                }
            }

            return instance;
        }
    }

    public void Awake()
    {
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TopTable
{
    public int Id;                    /* コマID                   */
    public string Name;               /* コマ名                   */
    public string AssetName;          /* コマアセット名           */
    public MovePattern MovePattern1;  /* コマ移動パターン(進化前) */
    public MovePattern MovePattern2;  /* コマ移動パターン(進化後) */
    public int Page;                  /* コマ定義のページ数       */
    public int Digit;                 /* コマ定義の要素桁数       */
}

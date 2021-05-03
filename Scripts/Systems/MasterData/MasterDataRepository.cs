/*#==========================================================================#*/
/*#    MasterDataRepository                                                  #*/
/*#                                                                          #*/
/*#    Summary    :    マスターデータの配置とロード                          #*/
/*#                                                                          #*/
/*#==========================================================================#*/
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MasterDataRepository : ScriptableObject
{
    [SerializeField]
    private MstData_Table mstData_Table = default;

    public void GetMstDataLoadAll(out List<TopTable> topTableList)
    {
        topTableList = mstData_Table.TopTableList;
    }
}

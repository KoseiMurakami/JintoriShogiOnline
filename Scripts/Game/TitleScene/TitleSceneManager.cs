using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using NCMB;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField]
    private GameObject backPanel = default;

    [SerializeField]
    private GameObject signUpPanel = default;

    [SerializeField]
    private GameObject logInPanel = default;

    [SerializeField]
    private GameObject soundPanel = default;

    [SerializeField]
    private InputField nameField = default;

    private string currentPlayerName;
    private SliderHelper sliderHelper;
    private Slider bgmSlider;
    private Slider seSlider;

    private void Start()
    {
        signUpPanel.SetActive(false);
        logInPanel.SetActive(false);
        soundPanel.SetActive(false);

        sliderHelper = soundPanel.transform.Find("SESlider").GetComponent<SliderHelper>();
        bgmSlider = soundPanel.transform.Find("BGMSlider").GetComponent<Slider>();
        bgmSlider.value = SoundManager.Instance.BgmVolume;
        seSlider = soundPanel.transform.Find("SESlider").GetComponent<Slider>();
        seSlider.value = SoundManager.Instance.SeVolume;

        SoundManager.Instance.PlayBgm(0);

        /* NCMBに登録しておくべきもの                          */
        /* ・オブジェクトID  ->  初回ログイン時                */
        /* ・ユーザー名      ->  初回ログイン時                */
        /* ・パスワード      ->  初回ログイン時                */
        /* ・レート          ->  初回ログイン時 + バトル終了時 */
        /* ・コイン          ->  初回ログイン時 + バトル終了時 */
        /*                                      + ガチャ排出時 */
        /* ・持ち駒          ->  初回ログイン時 + ガチャ排出時 */
        /* ・配置データ      ->  初回ログイン時 + 配置変更時   */

    }

    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit hit_info = new RaycastHit();
        //    float max_distance = 100f;

        //    bool is_hit = Physics.Raycast(ray, out hit_info, max_distance);

        //    if (is_hit)
        //    {
        //        if (hit_info.transform.name == backPanel.name)
        //        {
        //            //TODO: ヒットした時の処理;
        //            signUpPanel.SetActive(false);
        //            logInPanel.SetActive(false);
        //            soundPanel.SetActive(false);
        //        }
        //    }
        //}
    }


    // mobile backendに接続してログイン ------------------------

    public void logIn(string id, string pw)
    {
        NCMBUser.LogInAsync(id, pw, (NCMBException e) => {
            // 接続成功したら
            if (e == null)
            {
                currentPlayerName = id;
                Debug.Log("ログインしました！");

                //currentPlayerNameをキーにデータを引っ張ってくる
                initializePlayerInfo(NCMBUser.CurrentUser.ObjectId);
            }
        });
    }

    // mobile backendに接続して新規会員登録 ------------------------

    public void signUp(string id, string mail, string pw)
    {

        //新規プレイヤー情報を作成する
        string name = id;
        int rate = 0;
        int coins = 0;
        float possessingItemsPage1 = 0x00011002;
        float possessingItemsPage2 = 0x00000100;
        float alignInfo = 0x041815;
        PossessingItems items = new PossessingItems(possessingItemsPage1, possessingItemsPage2);
        GameManager.Instance.SetPlayerInfo(0, name, rate, coins, items, alignInfo);

        //データストアにプレイヤー情報登録
        NCMBUser user = new NCMBUser();
        user.UserName = name;
        //user.Email = mail;
        user.Password = pw;
        //user.Add("PossessingItemsPage1", possessingItemsPage1.ToString());
        //user.Add("PossessingItemsPage2", possessingItemsPage2.ToString());
        //user.Add("AlignInfo", alignInfo.ToString());
        user.SignUpAsync((NCMBException e) => {

            if (e == null)
            {
                currentPlayerName = id;
                Debug.Log("新規会員登録しました");
                //GameManager.Instance.SetUserData(user);
                GameManager.Instance.LoadScene("MenuScene");
            }
        });

    }

    // mobile backendに接続してログアウト ------------------------

    public void logOut()
    {

        NCMBUser.LogOutAsync((NCMBException e) => {
            if (e == null)
            {
                currentPlayerName = null;
                Debug.Log("ログアウトしました");
            }
        });
    }

    // 現在のプレイヤー名を返す --------------------
    public string currentPlayer()
    {
        return currentPlayerName;
    }

    /// <summary>
    /// アカウント作成ボタンを押したときの処理
    /// </summary>
    public void PushSignUpButton()
    {
        signUpPanel.SetActive(true);
        backPanel.SetActive(true);
    }

    /// <summary>
    /// ログインボタンを押したときの処理
    /// </summary>
    public void PushLogInButton()
    {
        logInPanel.SetActive(true);
        backPanel.SetActive(true);
    }

    /// <summary>
    /// サウンドボタンを押したときの処理
    /// </summary>
    public void PushSoundButton()
    {
        soundPanel.SetActive(true);
        backPanel.SetActive(true);
    }

    /// <summary>
    /// backPanelを切り替える
    /// </summary>
    /// <param name="val"></param>
    public void SetActiveBackPanel(bool val)
    {
        backPanel.SetActive(val);
    }

    /// <summary>
    /// BGMスライダーの値を変更したときの処理
    /// </summary>
    public void ChangedBGMSliderValue()
    {
        SoundManager.Instance.BgmVolume = bgmSlider.value;
    }

    /// <summary>
    /// SEスライダーの値を変更したときの処理
    /// </summary>
    public void ChangedSESliderValue()
    {
        sliderHelper.OnChangeValue.Subscribe(value => {
            SoundManager.Instance.SeVolume = seSlider.value;
            SoundManager.Instance.PlaySe(0);
        });
    }

    /// <summary>
    /// ゲストスタートボタンを押したときの処理
    /// </summary>
    public void PushGuestLogInButton()
    {
        //ゲスト用のプレイヤー情報を作成する
        string name;
        if (nameField.text == "")
        {
            name = "ゲスト";
        }
        else
        {
            name = nameField.text;
        }
        int rate = 0;
        int coins = 1000;
        float possessingItemsPage1 = 0x00011002;
        float possessingItemsPage2 = 0x00000100;
        float alignInfo = 0x041815;
        PossessingItems items = new PossessingItems(possessingItemsPage1, possessingItemsPage2);
        GameManager.Instance.SetPlayerInfo(0, name, rate, coins, items, alignInfo);
        GameManager.Instance.LoadScene("MenuScene");
    }

    /// <summary>
    /// アカウント作成決定時の処理
    /// </summary>
    public void PushSignUpDecideButton()
    {
        string id = signUpPanel.transform.Find("UserNameInputField").Find("Text").GetComponent<Text>().text;
        string pw = signUpPanel.transform.Find("PasswordInputField").Find("Text").GetComponent<Text>().text;
        signUp(id, "", pw);
    }

    /// <summary>
    /// ログイン決定時の処理
    /// </summary>
    public void PushLogInDecideButton()
    {
        string id = logInPanel.transform.Find("UserNameInputField").Find("Text").GetComponent<Text>().text;
        string pw = logInPanel.transform.Find("PasswordInputField").Find("Text").GetComponent<Text>().text;
        logIn(id, pw);
    }

    /// <summary>
    /// オブジェクトIDをキーにしてプレイヤー情報をデータベースから作成する
    /// </summary>
    /// <param name="objectId"></param>
    private void initializePlayerInfo(string objectId)
    {
        NCMBUser user = new NCMBUser();
        user.ObjectId = objectId;
        user.FetchAsync((NCMBException e) => {
            if (e != null)
            {
                Debug.Log("データベース登録失敗");
                //エラー処理
            }
            else
            {
                //成功時の処理
                string name = user.UserName;
                int rate = 0;
                int coins = 0;
                var tmpVal1 = (string)user["PossessingItemsPage1"];
                var tmpVal2 = (string)user["PossessingItemsPage2"];
                float possessingItemsPage1 = float.Parse(tmpVal1);
                float possessingItemsPage2 = float.Parse(tmpVal2);
                float alignInfo = float.Parse((string)user["AlignInfo"]);
                PossessingItems items = new PossessingItems(possessingItemsPage1, possessingItemsPage2);
                GameManager.Instance.SetPlayerInfo(0, name, rate, coins, items, alignInfo);

                Debug.Log("データベース登録完了");
                //GameManager.Instance.SetUserData(user);
                //メニューシーンへ
                GameManager.Instance.LoadScene("MenuScene");
            }
        });
    }
}

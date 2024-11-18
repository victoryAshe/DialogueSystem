using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text.RegularExpressions;

public class DialogueManager : MonoBehaviour
{

    //================================================
    //Public Variable
    //================================================
    [Header("Game Objects")]
    public GameObject Printer;
    public GameObject Characters;
    public GameObject FadeObj;


    [Header("UI Objects")]
    public Sprite[] printerSprites;
    public TextMeshProUGUI Printer_Text;
    public TextMeshProUGUI Name_Text;
    public GameObject skipBtnObj;
    public GameObject arrowObject;
    

    [Header("Rename")]
    public GameObject rename_Panel;
    public TMP_InputField inputField;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI countText;
    public Button nameBtn;
    [HideInInspector] public string name_got = "";

    [Header("Audio Objects")]
    public AudioSource SEAudio;
    public AudioSource CallAudio;

    [Header("Preference")]
    public float Delay = 0.1f;

    //[HideInInspector]
    public State state;

    [HideInInspector]
    public string Result;

    [Header("Story")]
    public GameObject[] Stories;
    public Sprite[] StoryBGs;
    public Image backgroundImage;
    public int tempStroyIndex;
    int currStory;

    public List<DialogueData> Story;
    int currIdx = 0;

    public static DialogueManager instance;

    //================================================
    //Private Method
    //================================================
    public Fade fade;
    private DCharacter _current_Character;
    private DialogueData _current_Data;

    private float _currentDelay;
    private float _lastDelay;
    private Coroutine _textingRoutine;
    private Coroutine _printingRoutine;

    private void Awake()
    {
        TryGetComponent<Fade>(out fade);
        fade.imgObject.SetActive(true);

        if (!instance) instance = this;
        else return;

        if (ConfigDataManager.Instance!=null && BackendGameData.Instance!=null)
        {
            currStory = ConfigDataManager.Instance.currStory;
            Stories[currStory].SetActive(true);
            if (BackendGameData.userData.storyCheck[currStory] == true)
            { 
                skipBtnObj.SetActive(true); 
            }

            backgroundImage.sprite = StoryBGs[currStory];
            StoryObject currStoryObj;
            Stories[currStory].TryGetComponent<StoryObject>(out currStoryObj);
            if (currStoryObj.themeIdx != 0)
            {
                Printer.GetComponent<Image>().sprite = printerSprites[currStoryObj.themeIdx - 1];
            }
        }
        else
        {
            Stories[tempStroyIndex].SetActive(true);
        }

        StartCoroutine(fade.fade(1, 0));
    }

    //================================================
    //Public Method
    //================================================
    #region Show & Hide
    public void Show(DialogueData Data)
    {
        _current_Data = Data;
        _find_character(Data.Character);

        if (_current_Character != null)
        {
            _emote("Normal");
            Name_Text.text = _current_Character.cName;
        }

        _textingRoutine = StartCoroutine(Activate());
    }

    public void Show(List<DialogueData> Data)
    {
        if(Story==null)Story = Data;
        StartCoroutine(Activate_List());
    }

    public void Click_Window()
    {
        SoundManager.instance.ButtonTochPlay();
        arrowObject.SetActive(false);
        switch (state)
        {
            case State.Active:
                StartCoroutine(_skip()); break;

            case State.Wait:
                Hide(); break;
        }
    }

    public void Hide()
    {
        if (_textingRoutine != null)
            StopCoroutine(_textingRoutine);

        if (_printingRoutine != null)
            StopCoroutine(_printingRoutine);

        state = State.Deactivate;

        if (_current_Data.Callback != null)
        {
            _current_Data.Callback.Invoke();
            _current_Data.Callback = null;
        }
    }

    IEnumerator FadeAndLoad()
    {
        fade.imgObject.SetActive(true);
        yield return fade.fade(0, 1);
        if (currStory == 0)
        {
            SceneManager.LoadScene("Tutorial");
        }
        else
        {
            SceneManager.LoadScene("StageSelect");
        }
    }

    public void SkipStory()
    {
        StartCoroutine(FadeAndUnLoad());
    }

    IEnumerator FadeAndUnLoad()
    {
        fade.imgObject.SetActive(true);
        yield return fade.fade(0, 1);
        SceneOrganizer.instance.UnloadSceneAdditive("Dialogue");
    }

    public string GetCompleteWord(string name, string firstVal, string secondVal)
    {
        char last = name[name.Length - 1];
        if (last < 0xAC00 || last > 0xD7A3) { return name; }

        string selectVal = (last - 0xAC00) % 28 > 0 ? firstVal : secondVal;

        return name + selectVal;
    }
    #endregion

    #region Sound

    public void Play_ChatSE()
    {
        if (_current_Character != null)
        {
            SEAudio.clip = _current_Character.ChatSE[UnityEngine.Random.Range(0, _current_Character.ChatSE.Length)];
            SEAudio.Play();
        }
    }

    public void Play_CallSE(string SEname)
    {
        if (_current_Character != null)
        {
            var FindSE
                = Array.Find(_current_Character.CallSE, (SE) => SE.name == SEname);

            SoundManager.instance.SFXPlay("se", FindSE);
        }
    }

    #endregion

    #region Speed

    public void Set_Speed(string speed)
    {
        switch (speed)
        {
            case "up":
                _currentDelay -= 0.25f;
                if (_currentDelay <= 0) _currentDelay = 0.001f;
                break;

            case "down":
                _currentDelay += 0.25f;
                break;

            case "init":
                _currentDelay = Delay;
                break;

            default:
                _currentDelay = float.Parse(speed);
                break;
        }

        _lastDelay = _currentDelay;
    }

    #endregion



    //================================================
    //Private Method
    //================================================

    private void _find_character(string name)
    {
        if (name != string.Empty)
        {
            Transform Child = Characters.transform.Find(name);
            if (Child != null)
            {
                Child.TryGetComponent<DCharacter>(out _current_Character);
                if (name == "Dog"){
                    _current_Character.cName = BackendGameData.Instance.Characters[0].name;
                }
                else if (name == "Hamster"){
                    _current_Character.cName = BackendGameData.Instance.Characters[1].name;
                }
                else if (name == "Cat"){
                    _current_Character.cName = BackendGameData.Instance.Characters[2].name;
                }
            }
        }
    }

    private void _initialize()
    {
        _currentDelay = Delay;
        _lastDelay = 0.1f;
        Printer_Text.text = string.Empty;

        Printer.SetActive(true);

        Characters.SetActive(_current_Character != null);
        foreach (Transform item in Characters.transform) item.gameObject.SetActive(false);
        if (_current_Character != null) _current_Character.gameObject.SetActive(true);
    }



    #region Show Text

    private IEnumerator Activate_List()
    {
        state = State.Active;

        for (; currIdx < Story.Count; currIdx++)
        {
            var Data = Story[currIdx];
            Show(Data);

            while (state != State.Deactivate) { yield return null; }
        }

        if (BackendGameData.userData.storyCheck[currStory] == false)
        {
            BackendGameData.userData.storyCheck[currStory] = true;
            StartCoroutine(FadeAndLoad());
        }
        else
        {
            StartCoroutine(FadeAndUnLoad());
        }
    }

    private IEnumerator Activate()
    {
        _initialize();

        state = State.Active;

        foreach (var item in _current_Data.Commands)
        {
            switch (item.Command)
            {
                case Command.print:
                    yield return _printingRoutine = StartCoroutine(_print(item.Context));
                    break;

                case Command.color:
                    _current_Data.Format.Color = item.Context;
                    break;

                case Command.emote:
                    _emote(item.Context);
                    break;

                case Command.size:
                    _current_Data.Format.Resize(item.Context);
                    break;

                case Command.sound:
                    Play_CallSE(item.Context);
                    break;

                case Command.speed:
                    Set_Speed(item.Context);
                    break;

                case Command.click:
                    yield return _waitInput();
                    break;

                case Command.close:
                    Hide();
                    yield break;

                case Command.wait:
                    yield return new WaitForSeconds(float.Parse(item.Context));
                    break;

                case Command.rename:
                    if (BackendGameData.userData.storyCheck[currStory] == true)
                    {
                        Hide();
                        yield break; 
                    }
                    else
                    {
                        string kind = _current_Character.name;
                        inputField.onValueChanged.AddListener(OnNameChanged);
                        if (kind == "Dog")
                        {
                            questionText.text = "강아지의 새로운 이름은?";
                        }
                        else if (kind == "Hamster")
                        {
                            questionText.text = "햄스터의 이름은?";
                        }
                        else
                        {
                            questionText.text = "고양이가 기억하는 이름은?";
                        }
                        rename_Panel.SetActive(true);
                        yield return _waitInput();
                        break;
                    }

                case Command.getname_ah:
                    GetName(item.Context);
                    name_got = GetCompleteWord(name_got, "아", "야");
                    ChangeScript(currIdx+1);
                    break;

                case Command.getname_yee:
                    GetName(item.Context);
                    name_got = GetCompleteWord(name_got, "이", "");
                    ChangeScript(currIdx + 1);
                    break;

                case Command.printer_off:
                    Printer.SetActive(false);
                    Printer_Text.text = "";
                    break;

                case Command.fade:
                    arrowObject.SetActive(true);
                    yield return _waitInput();
                    FadeObj.SetActive(true);
                    break;
            }
        }

        arrowObject.SetActive(true);
        if (!FadeObj.activeSelf) state = State.Wait;
        else ++currIdx;
    }

    private IEnumerator _waitInput()
    {
        while (!Input.GetMouseButtonDown(0)) yield return null;
        _currentDelay = _lastDelay;
    }

    private IEnumerator _print(string Text)
    {
        _current_Data.PrintText += _current_Data.Format.OpenTagger;

        for (int i = 0; i < Text.Length; i++)
        {
            _current_Data.PrintText += Text[i];
            Printer_Text.text = _current_Data.PrintText + _current_Data.Format.CloseTagger;

            if (Text[i] != ' ') Play_ChatSE();
            if (_currentDelay != 0) yield return new WaitForSeconds(_currentDelay);
        }

        _current_Data.PrintText += _current_Data.Format.CloseTagger;
    }

    public void _emote(string Text)
    {
        _current_Character.GetComponent<Image>().sprite = _current_Character.Emotion.Data[Text];
    }

    private IEnumerator _skip()
    {
        if (_current_Data.isSkippable)
        {
            _currentDelay = 0;
            while (state != State.Wait) yield return null;
            _currentDelay = Delay;
        }
    }


    #endregion

    #region Rename

    public void OnClickName()
    {
        string name_data = inputField.text;

        if (name_data.Length > 6)
        {
            Alarm.instance.CallAlarm("이름이 너무 길어요.\n6자 이내로 입력해주세요.");
            inputField.text = "";
        }
        else if (!Checkname(name_data))
        {
            Alarm.instance.CallAlarm("특수문자를 제외하고 입력해주세요!");
            inputField.text = "";
        }
        else
        {
            string kind = _current_Character.name;
            int index = 0;
            if (kind == "Hamster"){
                index = 1;
            }
            else if (kind == "Cat"){
                index = 2;
            }
            BackendGameData.Instance.Characters[index].name = name_data;

            _current_Character.cName = name_data;
            Name_Text.text = name_data;
            rename_Panel.SetActive(false);
            
            inputField.text = "";
            Click_Window();
            //Hide();
        }
    }

    void OnNameChanged(string s)
    {
        if (s.Length > 6)
        {
            countText.text = $"<color=#FF4A3E>{s.Length}</color>/6";
        }
        else
        {
            countText.text = $"<color=#CAB2A1>{s.Length}/6</color>";
            if (s.Length == 0)
            {
                nameBtn.interactable = false;
            }
            else
            {
                nameBtn.interactable = true;
            }

        }

    }

    bool Checkname(string value)
    {
        return Regex.IsMatch(value, @"^[0-9a-zA-Z가-힣]{1,6}$");
    }

    #endregion

    #region GetName
    void GetName(string command)
    {
        switch (command)
        {
            case "Dog":
                name_got = BackendGameData.Instance.Characters[0].name;
                break;

            case "Hamster":
                name_got = BackendGameData.Instance.Characters[1].name;
                break;

            case "Cat":
                name_got = BackendGameData.Instance.Characters[2].name;
                break;
        }
    }

    void ChangeScript(int idx)
    {
        string newText = Story[idx].getString;
        
        int i = newText.IndexOf("namegot");
        newText = newText.Replace("namegot", name_got);
        name_got = "";

        if (Story[idx].Callback != null)
        {
            UnityAction callback = Story[idx].Callback;
            Story[idx] = new DialogueData(newText, Story[idx].Character, callback);
        }
        else
        {
            Story[idx] = new DialogueData(newText, Story[idx].Character);
        }
            
    }
    #endregion
}


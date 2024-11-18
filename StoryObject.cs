using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StoryObject : MonoBehaviour
{
    public DialogueManager DialogManager;

    //public Story story;
    public int themeIdx = 0;
    public GameObject[] Example;
    readonly int notUsed = -1;
    
    private void Start()
    {
        List<DialogueData> dataList = ResourceManager.Instance.dialogueDatas[int.Parse(gameObject.name)];
        Debug.Log($"Story {gameObject.name} : {dataList.Count}"); //temporary

        for (int i = 0; i < dataList.Count; i++)
        {
            // i는 for문이 끝나면 사라지는 값이기 때문에 Action이 올바르게 동작하려면 캐싱을 해줘야 함
            int showIndex = dataList[i].showIndex;
            int hideIndex = dataList[i].hideIndex;
            if (showIndex != notUsed || hideIndex != notUsed)
            {
                dataList[i].Callback = new UnityAction(() =>{
                    if (showIndex != notUsed) Example[showIndex].SetActive(true);
                    if (hideIndex != notUsed) Example[hideIndex].SetActive(false);
                });
            }
        }
                
        DialogManager.Show(dataList);
    }
}

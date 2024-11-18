using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(Story))]
public class StoryDrawer : PropertyDrawer
{
    #region variables
    //================================================
    //Private Variable
    //================================================
    private int ArraySize = 0;
    private string DialogueString = "Input Dialogue";
    string defaultDialogue = "Plaese fill this string";

    private SerializedProperty _dialogue = null;
    private SerializedProperty _charType = null;
    #endregion

    #region override
    //================================================
    //Public Method
    //================================================
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        _initialize(position, property);
        _display_DialogueList(position);
        _display_AddArea(position);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 18 * (ArraySize + 2);
    }
    #endregion

    #region init
    //================================================
    //Private Method : init
    //================================================
    private void _initialize(Rect pos, SerializedProperty property)
    {
        _dialogue = property.FindPropertyRelative("_dialogue");
        _charType = property.FindPropertyRelative("_charType");

        ArraySize = _dialogue.arraySize;
    }
    #endregion

    #region display
    //================================================
    //Private Method : display
    //================================================
    private void _display_Header(Rect startPos)
    {
        EditorGUI.LabelField(startPos, "Story");
        
        EditorGUI.indentLevel++;
    }

    private void _display_Array(Rect startPos, SerializedProperty array)
    {
        for (int i = 0; i < array.arraySize; i++)
        {
            startPos = new Rect(startPos.position + new Vector2(0, 18), startPos.size);
            EditorGUI.PropertyField(startPos, array.GetArrayElementAtIndex(i), GUIContent.none);
        }
    }

    private void _display_DeleteButton(Rect startPos)
    {
        for (int i = 0; i < _charType.arraySize; i++)
        {
            startPos = new Rect(startPos.position + new Vector2(0, 18), startPos.size);
            if (_dialogue.GetArrayElementAtIndex(i).stringValue != "Input Dialogue" && GUI.Button(startPos, "-"))
            {
                int j = i;
                _delete_Raw(j);
            }
        }
    }

    private void _display_DialogueList(Rect startPos)
    {
        Rect NewRect = new Rect(startPos.position, new Vector2(startPos.width / 3, 16));

        _display_Header(NewRect);
        _display_Array(NewRect, _dialogue);
        _display_Array(_get_Rect(NewRect, NewRect.width, NewRect.width), _charType);
        _display_DeleteButton(_get_Rect(NewRect, NewRect.width * 2 + 10, 30));
    }

    private void _display_AddButton(Rect rect)
    {
        if (GUI.Button(rect, "create"))
        {
            _add_Raw();
            DialogueString = defaultDialogue;
        }
    }

    private void _display_TextArea(Rect rect)
    {
        DialogueString = EditorGUI.TextField(rect, DialogueString);
    }

    private void _display_AddArea(Rect startPos)
    {
        Rect InputRect = _get_Rect(startPos, 0, startPos.width / 3 * 2, (_dialogue.arraySize + 1) * 18);

        _display_TextArea(InputRect);
        _display_AddButton(_get_Rect(InputRect, InputRect.width + 20, 70));
    }
    #endregion

    #region methods
    //================================================
    //Private Method : methods
    //================================================
    private void _delete_ArrayElement(SerializedProperty array, int index, bool isObject = false)
    {
        if (isObject && array.GetArrayElementAtIndex(index) != null) array.DeleteArrayElementAtIndex(index);
        array.DeleteArrayElementAtIndex(index);
    }

    private void _delete_Raw(int index)
    {
        _delete_ArrayElement(_dialogue, index);
        _delete_ArrayElement(_charType, index, true);
    }

    private void _add_Raw()
    {
        if (!_is_duplicated_emotion_name(DialogueString))
        {
            _dialogue.InsertArrayElementAtIndex(_dialogue.arraySize);

            if (string.IsNullOrEmpty(DialogueString))
            {
                DialogueString = defaultDialogue;
            }

            _dialogue.GetArrayElementAtIndex(_dialogue.arraySize - 1).stringValue = DialogueString;

            _charType.InsertArrayElementAtIndex(_charType.arraySize);
            
        }
    }

    private bool _is_duplicated_emotion_name(string name)
    {
        for (int i = 0; i < _dialogue.arraySize; i++)
        {
            if (_dialogue.GetArrayElementAtIndex(i).stringValue == name) return true;
        }

        return false;
    }

    private Rect _get_Rect(Rect From, float x, float width)
    {
        return new Rect(From.position + new Vector2(x, 0), new Vector2(width, 16));
    }

    private Rect _get_Rect(Rect From, float x, float width, float y)
    {
        return new Rect(From.position + new Vector2(x, y), new Vector2(width, 16));
    }
    #endregion
}
#endif
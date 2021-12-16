using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DIALOG", menuName = "DIALOG/DIALOG", order = 0)]
public class DialogLine : ScriptableObject {
    public DialogSystem.Dialog.Content[] content;
}

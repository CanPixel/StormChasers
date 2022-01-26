using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Top3Billboards : MonoBehaviour {
    public WorldTexturer board1, board2, board3;
    public Text totalScore;

    public GameObject focalPoint;

    public void SubmitPlayerResults(CameraControl cm, CameraControl.Screenshot[] s, int ts) {
        if(s.Length <= 0) return;

        totalScore.text = ts.ToString();

        if(s[0] != null) {
            board1.SetSprite(Sprite.Create(s[0].image, new Rect(0, 0, cm.resWidth, cm.resHeight), new Vector2(0.5f, 0.5f)));
            board1.SetLabel(s[0].score.ToString());
        }
        if(s.Length > 1 && s[1] != null) {
            board2.SetSprite(Sprite.Create(s[1].image, new Rect(0, 0, cm.resWidth, cm.resHeight), new Vector2(0.5f, 0.5f)));
            board2.SetLabel(s[1].score.ToString());
        }
        if(s.Length > 2 && s[2] != null) {
            board3.SetSprite(Sprite.Create(s[2].image, new Rect(0, 0, cm.resWidth, cm.resHeight), new Vector2(0.5f, 0.5f)));
            board3.SetLabel(s[2].score.ToString());
        }
    }
}

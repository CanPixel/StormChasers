using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShaderReel : MonoBehaviour {
    public CameraControl camControl;
    public GameObject reelobject, photoReelPrefab;

    public int offset = 550;
    public int padding = 20;
    public int current = 0;

    private List<ShaderFilter> filters = new List<ShaderFilter>();
    
    void Start() {
        foreach(Transform i in reelobject.transform) Destroy(i.gameObject);

        for(int i = 0; i < camControl.camSystem.shaderFilters.Length; i++) {
            var fil = camControl.camSystem.shaderFilters[i];
            var obj = Instantiate(photoReelPrefab);
            obj.transform.SetParent(reelobject.transform);
            obj.transform.localScale = Vector3.one;
            var sf = obj.GetComponent<ShaderFilter>();
            filters.Add(sf);
            sf.icon.sprite = fil.icon;
            sf.text.text = fil.name;
        }
    }

    void Update() {
        reelobject.transform.localPosition = Vector3.Lerp(reelobject.transform.localPosition, new Vector3((current) * -200 + offset + (padding * current), reelobject.transform.localPosition.y, reelobject.transform.localPosition.z), Time.unscaledDeltaTime * 18f);        
    
        for(int i = 0; i < filters.Count; i++) {
            var cur = i == current;
            filters[i].transform.localScale = Vector3.one * (cur ? 1.1f : 1);
        }
    }
}

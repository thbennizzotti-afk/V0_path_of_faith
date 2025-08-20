using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIRaycastPeek : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && EventSystem.current)
        {
            var data = new PointerEventData(EventSystem.current){ position = Input.mousePosition };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, hits);
            Debug.Log(hits.Count == 0 ? "[UIRaycast] rien" : "[UIRaycast] top → " + hits[0].gameObject.name);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CutsceneCanvas : MonoBehaviour
{
    [System.Serializable]
    private struct Info {
        public TMP_Text name;
        public TMP_Text description;
        public GameObject obj;

        public void SetActive(bool active)
        {
            obj.SetActive(active);
        }
    }

    public enum CutsceneTextEvent
    {
        Start,
        End,
        TurnOff,
    }

    [SerializeField] private Info start;
    [SerializeField] private Info end;

    public void SetText(LevelScriptableObject obj) {
        start.name.text        = obj.DisplayName;
        start.description.text = obj.Description;

        end.name.text        = obj.CompletedMessage;
        end.description.text = "Press space to continue";
    }

    public void CutsceneText(CutsceneTextEvent e) {
        switch (e) { 
            case CutsceneTextEvent.Start:
                start.SetActive(true);
                end.SetActive(false);
                break;

            case CutsceneTextEvent.End:
                start.SetActive(false);
                end.SetActive(true);
                break;

            case CutsceneTextEvent.TurnOff:
                start.SetActive(false);
                end.SetActive(false);
                break;
        }
    }
}

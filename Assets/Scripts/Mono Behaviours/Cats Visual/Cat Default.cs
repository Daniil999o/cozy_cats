using UnityEngine;
using CozyPolyomino;

[RequireComponent(typeof(DraggableShape))]
public class CatDefault : MonoBehaviour
{
    private void OnEnable() {
        DraggableShape draggable = GetComponent<DraggableShape>();

        CatColors catColors = GetComponent<CatColors>();
        CatAudioPlayer catAudio = GetComponentInChildren<CatAudioPlayer>();
        SausageRotation catSausage = GetComponent<SausageRotation>();
        CatBehaviour catBehaviour = GetComponent<CatBehaviour>();

        draggable.OnEnterHover += _ => catColors.Hover = true;
        draggable.OnEnterHover += catColors.UpdateState;
        draggable.OnEnterHover += _ => catAudio.PlayEnter();
        
        draggable.OnExitHover += _ => catColors.Hover = false;
        draggable.OnExitHover += catColors.UpdateState;
        draggable.OnExitHover += _ => catAudio.PlayExit();
        
        draggable.OnStartDrag += _ => catBehaviour.WakeUp();
        draggable.OnStartDrag += catColors.UpdateState;
        draggable.OnStartDrag += _ => catSausage.SetDragging(true);
        draggable.OnStartDrag += _ => catAudio.PlayPickup();
        draggable.OnStartDrag += _ => catAudio.PlayMew();
        
        draggable.OnDragging += catColors.UpdateState;
        
        draggable.OnEndsDrag += catColors.UpdateState;
        draggable.OnEndsDrag += _ => catSausage.SetDragging(false);
        draggable.OnEndsDrag += _ => catAudio.PlayDrop();
        draggable.OnEndsDrag += _ => catBehaviour.GoToSleep();
    }
    
    private void OnDisable() {
        DraggableShape draggable = GetComponent<DraggableShape>();

        CatColors catColors = GetComponent<CatColors>();
        CatAudioPlayer catAudio = GetComponentInChildren<CatAudioPlayer>();
        SausageRotation sausage = GetComponent<SausageRotation>();

        draggable.OnEnterHover -= _ => catColors.Hover = true;
        draggable.OnEnterHover -= catColors.UpdateState;
        draggable.OnEnterHover -= _ => catAudio.PlayEnter();
        
        draggable.OnExitHover -= _ => catColors.Hover = false;
        draggable.OnExitHover -= catColors.UpdateState;
        draggable.OnExitHover -= _ => catAudio.PlayExit();
        
        draggable.OnStartDrag -= catColors.UpdateState;
        draggable.OnStartDrag -= _ => sausage.SetDragging(true);
        draggable.OnStartDrag -= _ => catAudio.PlayPickup();
        
        draggable.OnDragging -= catColors.UpdateState;
        
        draggable.OnEndsDrag -= catColors.UpdateState;
        draggable.OnEndsDrag -= _ => sausage.SetDragging(false);
        draggable.OnEndsDrag -= _ => catAudio.PlayDrop();
    }
}
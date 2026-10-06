using UnityEngine;
using UnityEngine.EventSystems;
namespace HuntingBoat
{
    public sealed class HeldControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public System.Action<bool> Changed;
        public void OnPointerDown(PointerEventData data) { Changed?.Invoke(true); }
        public void OnPointerUp(PointerEventData data) { Changed?.Invoke(false); }
        private void OnDisable() { Changed?.Invoke(false); }
    }

}

using UnityEngine;
using UnityEngine.EventSystems;

namespace AL.UI
{
    public class SearchbarInteraction : InputFieldInteraction
    {
        [SerializeField]
        private Vector2 onSelectSize;
        private Vector2 normalSizeDelta;

        public override void Init()
        {
            base.Init();
            normalSizeDelta = ((RectTransform)transform).sizeDelta;
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            ((RectTransform)transform).sizeDelta = onSelectSize;
        }

        //public override void OnEndEdit(string val)
        //{
        //    base.OnEndEdit(val);
        //    ((RectTransform)transform).sizeDelta = normalSizeDelta;
        //}

        public override void OnDeselect(BaseEventData baseEventData)
        {
            base.OnDeselect(baseEventData);
            ((RectTransform)transform).sizeDelta = normalSizeDelta;
        }

    }
}

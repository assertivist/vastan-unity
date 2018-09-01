using UnityEngine;
using UnityEngine.UI;
using Vastan.Util;

namespace Vastan.GUI
{
    class ScrollyText : MonoBehaviour
    {
        private Text MyText;
        private RectTransform ParentRect;

        public void Start()
        {
            MyText = GetComponent<Text>();
            ParentRect = GetComponent<RectTransform>();
            if (!MyText)
            {
                Log.Error("ScrollyText added to non-text element");
            }
        }

        private bool TextIsAtEndOfBox()
        {
            var textWidth = LayoutUtility.GetPreferredWidth(MyText.rectTransform);
            var parentWidth = ParentRect.rect.width;
            if (textWidth >= parentWidth)
            {
                return true;
            }
            else return false;
        }

        public void AddText(string theText)
        {
            MyText.text += theText;
            while(TextIsAtEndOfBox())
            {
                MyText.text = MyText.text.Remove(0, 1);
            }
        }

        public void ClearText()
        {
            MyText.text = "";
        }

    }
}
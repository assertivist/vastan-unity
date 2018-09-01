using UnityEngine;
using Vastan.GUI;
using Vastan.InputManagement;

namespace Vastan.Test
{
    class VastanChatTest : MonoBehaviour
    {
        public GameObject TestText;
        private ScrollyText TestScrolly;

        private ChatManager TheChatManager;
        public void Start()
        {
            TestScrolly = TestText.GetComponent<ScrollyText>();
            TheChatManager = new ChatManager();
            TheChatManager.ChatEnabled = true;
        }

        public void Update()
        {
            TheChatManager.Update();
            TestScrolly.AddText(TheChatManager.Flush());
        }
    }

    
}
using UnityEngine;

namespace Vastan.InputManagement
{
    class ChatManager
    {
        public bool ChatEnabled = false;
        
        private string Buffa = "";

        public void Update()
        {
            if (ChatEnabled)
            {
                Buffa += Input.inputString;
            }
        }

        public string Flush()
        {
            if (Buffa.Length < 1)
            {
                return "";
            }
            string theBuffa = Buffa;
            Buffa = "";
            return theBuffa;
        }
    }
}
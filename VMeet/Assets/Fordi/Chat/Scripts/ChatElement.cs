using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fordi.ChatEngine
{
    public class ChatElement : MonoBehaviour, IResettable
    {
        [SerializeField]
        private TextMeshProUGUI m_message = null;
        public string Message { get { return m_message.text; } private set { m_message.text = value; } }

        private ChatInfo m_chatInfo;

        public void Init(ChatInfo chatInfo)
        {
            m_chatInfo = chatInfo;
            m_message.text = chatInfo.Sender + ": " + chatInfo.Message;
        }

        public bool IsRelavant(string searchValue)
        {
            var lowerItemSender = m_chatInfo.Sender.ToLower();
            var lowerMessage = m_chatInfo.Message.ToLower();
            var lowerSearchValue = searchValue.ToLower();
            return lowerItemSender.Contains(lowerSearchValue) || lowerMessage.Contains(lowerSearchValue);
        }

        public void OnReset()
        {
            Message = "";
        }
    }
}
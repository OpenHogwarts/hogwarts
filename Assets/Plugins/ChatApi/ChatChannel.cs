// ----------------------------------------------------------------------------------------------------------------------
// <summary>The Photon Chat Api enables clients to connect to a chat server and communicate with other clients.</summary>
// <remarks>ChatClient is the main class of this api.</remarks>
// <copyright company="Exit Games GmbH">Photon Chat Api - Copyright (C) 2014 Exit Games GmbH</copyright>
// ----------------------------------------------------------------------------------------------------------------------

namespace ExitGames.Client.Photon.Chat
{
    using System.Collections.Generic;

    /// <summary>
    /// A channel of communication in Photon Chat, updated by ChatClient and provided as READ ONLY.
    /// </summary>
    /// <remarks>
    /// Contains messages and senders to use (read!) and display by your GUI.

    /// Access these by:
    ///     ChatClient.PublicChannels
    ///     ChatClient.PrivateChannels
    /// </remarks>
    public class ChatChannel
    {
        /// <summary>Name of the channel (used to subscribe and unsubscribe).</summary>
        public readonly string Name;

        /// <summary>Senders of messages in chronoligical order. Senders and Messages refer to each other by index. Senders[x] is the sender of Messages[x].</summary>
        public readonly List<string> Senders = new List<string>();

        /// <summary>Messages in chronoligical order. Senders and Messages refer to each other by index. Senders[x] is the sender of Messages[x].</summary>
        public readonly List<object> Messages = new List<object>();

        /// <summary>Is this a private 1:1 channel?</summary>
        public bool IsPrivate { get; internal protected set; }

        /// <summary>Count of messages this client still buffers/knows for this channel.</summary>
        public int MessageCount { get { return this.Messages.Count; } }


        /// <summary>Used internally to create new channels. This does NOT create a channel on the server! Use ChatClient.Subscribe.</summary>
        public ChatChannel(string name)
        {
            this.Name = name;
        }

        /// <summary>Used internally to add messages to this channel.</summary>
        public void Add(string sender, object message)
        {
            this.Senders.Add(sender);
            this.Messages.Add(message);
        }

        /// <summary>Used internally to add messages to this channel.</summary>
        public void Add(string[] senders, object[] messages)
        {
            this.Senders.AddRange(senders);
            this.Messages.AddRange(messages);
        }

        /// <summary>Clear the local cache of messages currently stored. This frees memory but doesn't affect the server.</summary>
        public void ClearMessages()
        {
            this.Senders.Clear();
            this.Messages.Clear();
        }
    }
}
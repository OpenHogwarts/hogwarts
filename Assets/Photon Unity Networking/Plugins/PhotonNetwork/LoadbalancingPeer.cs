// ----------------------------------------------------------------------------
// <copyright file="LoadbalancingPeer.cs" company="Exit Games GmbH">
//   Loadbalancing Framework for Photon - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   Provides the operations needed to use the loadbalancing server app(s).
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;


namespace ExitGames.Client.Photon
{
    /// <summary>
    /// Internally used by PUN, a LoadbalancingPeer provides the operations and enum
    /// definitions needed to use the Photon Loadbalancing server (or the Photon Cloud).
    /// </summary>
    /// <remarks>
    /// The LoadBalancingPeer does not keep a state, instead this is done by a LoadBalancingClient.
    /// </remarks>
    internal class LoadbalancingPeer : PhotonPeer
    {

        virtual internal bool IsProtocolSecure { get { return this.UsedProtocol == ConnectionProtocol.WebSocketSecure; } }

        private readonly Dictionary<byte, object> opParameters = new Dictionary<byte, object>();    // used in OpRaiseEvent() (avoids lots of new Dictionary() calls)

        public LoadbalancingPeer(IPhotonPeerListener listener, ConnectionProtocol protocolType) : base(listener, protocolType)
        {
        }

        public virtual bool OpGetRegions(string appId)
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>();
            parameters[(byte)ParameterCode.ApplicationId] = appId;

            return this.OpCustom(OperationCode.GetRegions, parameters, true, 0, true);
        }

        /// <summary>
        /// Joins the lobby on the Master Server, where you get a list of RoomInfos of currently open rooms.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// </summary>
        /// <returns>If the operation could be sent (has to be connected).</returns>
        public virtual bool OpJoinLobby(TypedLobby lobby)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpJoinLobby()");
            }

            Dictionary<byte, object> parameters = null;
            if (lobby != null && !lobby.IsDefault)
            {
                parameters = new Dictionary<byte, object>();
                parameters[(byte)ParameterCode.LobbyName] = lobby.Name;
                parameters[(byte)ParameterCode.LobbyType] = (byte)lobby.Type;
            }

            return this.OpCustom(OperationCode.JoinLobby, parameters, true);
        }

        /// <summary>
        /// Leaves the lobby on the Master Server.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// </summary>
        /// <returns>If the operation could be sent (has to be connected).</returns>
        public virtual bool OpLeaveLobby()
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpLeaveLobby()");
            }

            return this.OpCustom(OperationCode.LeaveLobby, null, true);
        }


        /// <summary>
        /// Don't use this method directly, unless you know how to cache and apply customActorProperties.
        /// The PhotonNetwork methods will handle player and room properties for you and call this method.
        /// </summary>
        public virtual bool OpCreateRoom(string roomName, RoomOptions roomOptions, TypedLobby lobby, Hashtable playerProperties, bool onGameServer)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpCreateRoom()");
            }

            Dictionary<byte, object> op = new Dictionary<byte, object>();

            if (!string.IsNullOrEmpty(roomName))
            {
                op[ParameterCode.RoomName] = roomName;
            }
            if (lobby != null)
            {
                op[ParameterCode.LobbyName] = lobby.Name;
                op[ParameterCode.LobbyType] = (byte)lobby.Type;
            }

            if (onGameServer)
            {
                if (playerProperties != null && playerProperties.Count > 0)
                {
                    op[ParameterCode.PlayerProperties] = playerProperties;
                    op[ParameterCode.Broadcast] = true; // TODO: check if this also makes sense when creating a room?! // broadcast actor properties
                }


                if (roomOptions == null)
                {
                    roomOptions = new RoomOptions();
                }

                Hashtable gameProperties = new Hashtable();
                op[ParameterCode.GameProperties] = gameProperties;
                gameProperties.MergeStringKeys(roomOptions.customRoomProperties);

                gameProperties[GameProperties.IsOpen] = roomOptions.isOpen; // TODO: check default value. dont send this then
                gameProperties[GameProperties.IsVisible] = roomOptions.isVisible; // TODO: check default value. dont send this then
                gameProperties[GameProperties.PropsListedInLobby] = roomOptions.customRoomPropertiesForLobby;
                if (roomOptions.maxPlayers > 0)
                {
                    gameProperties[GameProperties.MaxPlayers] = roomOptions.maxPlayers;
                }
                if (roomOptions.cleanupCacheOnLeave)
                {
                    op[ParameterCode.CleanupCacheOnLeave] = true;               // this is actually setting the room's config
                    gameProperties[GameProperties.CleanupCacheOnLeave] = true;  // this is only informational for the clients which join
                }
                if (roomOptions.suppressRoomEvents)
                {
                    op[ParameterCode.SuppressRoomEvents] = true;
                }
            }

            // UnityEngine.Debug.Log("CreateGame: " + SupportClass.DictionaryToString(op));
            return this.OpCustom(OperationCode.CreateGame, op, true);
        }


        /// <summary>LoadBalancingPeer.OpJoinRoom</summary>
        public virtual bool OpJoinRoom(string roomName, RoomOptions roomOptions, TypedLobby lobby, bool createIfNotExists, Hashtable playerProperties, bool onGameServer)
        {
            Dictionary<byte, object> op = new Dictionary<byte, object>();

            if (!string.IsNullOrEmpty(roomName))
            {
                op[ParameterCode.RoomName] = roomName;
            }
            if (createIfNotExists)
            {
                op[ParameterCode.CreateIfNotExists] = true;
                if (lobby != null)
                {
                    op[ParameterCode.LobbyName] = lobby.Name;
                    op[ParameterCode.LobbyType] = (byte)lobby.Type;
                }
            }

            if (onGameServer)
            {
                if (playerProperties != null && playerProperties.Count > 0)
                {
                    op[ParameterCode.PlayerProperties] = playerProperties;
                    op[ParameterCode.Broadcast] = true; // broadcast actor properties
                }


                if (createIfNotExists)
                {
                    if (roomOptions == null)
                    {
                        roomOptions = new RoomOptions();
                    }

                    Hashtable gameProperties = new Hashtable();
                    op[ParameterCode.GameProperties] = gameProperties;
                    gameProperties.MergeStringKeys(roomOptions.customRoomProperties);

                    gameProperties[GameProperties.IsOpen] = roomOptions.isOpen;
                    gameProperties[GameProperties.IsVisible] = roomOptions.isVisible;
                    gameProperties[GameProperties.PropsListedInLobby] = roomOptions.customRoomPropertiesForLobby;
                    if (roomOptions.maxPlayers > 0)
                    {
                        gameProperties[GameProperties.MaxPlayers] = roomOptions.maxPlayers;
                    }
                    if (roomOptions.cleanupCacheOnLeave)
                    {
                        op[ParameterCode.CleanupCacheOnLeave] = true;               // this is actually setting the room's config
                        gameProperties[GameProperties.CleanupCacheOnLeave] = true;  // this is only informational for the clients which join
                    }
                    if (roomOptions.suppressRoomEvents)
                    {
                        op[ParameterCode.SuppressRoomEvents] = true;
                    }
                }
            }

            // UnityEngine.Debug.Log("JoinGame: " + SupportClass.DictionaryToString(op));
            return this.OpCustom(OperationCode.JoinGame, op, true);
        }


        /// <summary>
        /// Operation to join a random, available room. Overloads take additional player properties.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// If all rooms are closed or full, the OperationResponse will have a returnCode of ErrorCode.NoRandomMatchFound.
        /// If successful, the OperationResponse contains a gameserver address and the name of some room.
        /// </summary>
        /// <param name="expectedCustomRoomProperties">Optional. A room will only be joined, if it matches these custom properties (with string keys).</param>
        /// <param name="expectedMaxPlayers">Filters for a particular maxplayer setting. Use 0 to accept any maxPlayer value.</param>
        /// <param name="playerProperties">This player's properties (custom and well known).</param>
        /// <param name="matchingType">Selects one of the available matchmaking algorithms. See MatchmakingMode enum for options.</param>
        /// <returns>If the operation could be sent currently (requires connection).</returns>
        public virtual bool OpJoinRandomRoom(Hashtable expectedCustomRoomProperties, byte expectedMaxPlayers, Hashtable playerProperties, MatchmakingMode matchingType, TypedLobby typedLobby, string sqlLobbyFilter)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpJoinRandomRoom()");
            }

            Hashtable expectedRoomProperties = new Hashtable();
            expectedRoomProperties.MergeStringKeys(expectedCustomRoomProperties);
            if (expectedMaxPlayers > 0)
            {
                expectedRoomProperties[GameProperties.MaxPlayers] = expectedMaxPlayers;
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            if (expectedRoomProperties.Count > 0)
            {
                opParameters[ParameterCode.GameProperties] = expectedRoomProperties;
            }

            if (playerProperties != null && playerProperties.Count > 0)
            {
                opParameters[ParameterCode.PlayerProperties] = playerProperties;
            }

            if (matchingType != MatchmakingMode.FillRoom)
            {
                opParameters[ParameterCode.MatchMakingType] = (byte)matchingType;
            }

            if (typedLobby != null)
            {
                opParameters[ParameterCode.LobbyName] = typedLobby.Name;
                opParameters[ParameterCode.LobbyType] = (byte)typedLobby.Type;
            }

            if (!string.IsNullOrEmpty(sqlLobbyFilter))
            {
                opParameters[ParameterCode.Data] = sqlLobbyFilter;
            }

            // UnityEngine.Debug.LogWarning("OpJoinRandom: " + opParameters.ToStringFull());
            return this.OpCustom(OperationCode.JoinRandomGame, opParameters, true);
        }

        /// <summary>
        /// Request the rooms and online status for a list of friends (each client must set a unique username via OpAuthenticate).
        /// </summary>
        /// <remarks>
        /// Used on Master Server to find the rooms played by a selected list of users.
        /// Users identify themselves by using OpAuthenticate with a unique username.
        /// The list of usernames must be fetched from some other source (not provided by Photon).
        ///
        /// The server response includes 2 arrays of info (each index matching a friend from the request):
        /// ParameterCode.FindFriendsResponseOnlineList = bool[] of online states
        /// ParameterCode.FindFriendsResponseRoomIdList = string[] of room names (empty string if not in a room)
        /// </remarks>
        /// <param name="friendsToFind">Array of friend's names (make sure they are unique).</param>
        /// <returns>If the operation could be sent (requires connection).</returns>
        public virtual bool OpFindFriends(string[] friendsToFind)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            if (friendsToFind != null && friendsToFind.Length > 0)
            {
                opParameters[ParameterCode.FindFriendsRequestList] = friendsToFind;
            }

            return this.OpCustom(OperationCode.FindFriends, opParameters, true);
        }

        public bool OpSetCustomPropertiesOfActor(int actorNr, Hashtable actorProperties, bool broadcast, byte channelId)
        {
            return this.OpSetPropertiesOfActor(actorNr, actorProperties.StripToStringKeys(), broadcast, channelId, null);
        }

        protected internal bool OpSetPropertiesOfActor(int actorNr, Hashtable actorProperties, bool broadcast, byte channelId, Hashtable expectedValues)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfActor()");
            }

            if (actorNr <= 0 || actorProperties == null)
            {
                if (this.DebugOut >= DebugLevel.INFO)
                {
                    this.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfActor not sent. ActorNr must be > 0 and actorProperties != null.");
                }
                return false;
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters.Add(ParameterCode.Properties, actorProperties);
            opParameters.Add(ParameterCode.ActorNr, actorNr);
            if (broadcast)
            {
                opParameters.Add(ParameterCode.Broadcast, broadcast);
            }

            if (expectedValues != null && expectedValues.Count > 0)
            {
                // UnityEngine.Debug.Log("Expected values: " + expectedValues.ToStringFull());
                opParameters.Add(ParameterCode.ExpectedValues, expectedValues);
            }

            return this.OpCustom((byte)OperationCode.SetProperties, opParameters, broadcast, channelId);
        }

        protected void OpSetPropertyOfRoom(byte propCode, object value)
        {
            Hashtable properties = new Hashtable();
            properties[propCode] = value;
            this.OpSetPropertiesOfRoom(properties, true, (byte)0, null);
        }

        public bool OpSetCustomPropertiesOfRoom(Hashtable gameProperties, bool broadcast, byte channelId)
        {
            return this.OpSetPropertiesOfRoom(gameProperties.StripToStringKeys(), broadcast, channelId, null);
        }

        protected internal bool OpSetPropertiesOfRoom(Hashtable gameProperties, bool broadcast, byte channelId, Hashtable expectedValues)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfRoom()");
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters.Add(ParameterCode.Properties, gameProperties);
            if (broadcast)
            {
                opParameters.Add(ParameterCode.Broadcast, true);
            }

            if (expectedValues != null && expectedValues.Count > 0)
            {
                // UnityEngine.Debug.Log("Expected values: " + expectedValues.ToStringFull());
                opParameters.Add(ParameterCode.ExpectedValues, expectedValues);
            }

            return this.OpCustom((byte)OperationCode.SetProperties, opParameters, true, channelId);
        }

        /// <summary>
        /// Sends this app's appId and appVersion to identify this application server side.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// </summary>
        /// <remarks>
        /// This operation makes use of encryption, if that is established before.
        /// See: EstablishEncryption(). Check encryption with IsEncryptionAvailable.
        /// This operation is allowed only once per connection (multiple calls will have ErrorCode != Ok).
        /// </remarks>
        /// <param name="appId">Your application's name or ID to authenticate. This is assigned by Photon Cloud (webpage).</param>
        /// <param name="appVersion">The client's version (clients with differing client appVersions are separated and players don't meet).</param>
        /// <param name="authValues"></param>
        /// <param name="regionCode">When authenticating for a specific region, a NameServer will forward you to that region's MasterServer.</param>
        /// <returns>If the operation could be sent (has to be connected).</returns>
        public virtual bool OpAuthenticate(string appId, string appVersion, AuthenticationValues authValues, string regionCode)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpAuthenticate()");
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            if (authValues != null && authValues.Token != null)
            {
                opParameters[ParameterCode.Secret] = authValues.Token;
                return this.OpCustom(OperationCode.Authenticate, opParameters, true, (byte)0, false);
            }

            opParameters[ParameterCode.AppVersion] = appVersion;
            opParameters[ParameterCode.ApplicationId] = appId;

            if (!string.IsNullOrEmpty(regionCode))
            {
                opParameters[ParameterCode.Region] = regionCode;
            }

            if (authValues != null)
            {
                if (!string.IsNullOrEmpty(authValues.UserId))
                {
                    UnityEngine.Debug.LogWarning("UserId sent: " + authValues.UserId);
                    opParameters[ParameterCode.UserId] = authValues.UserId;
                }

                if (authValues.AuthType != CustomAuthenticationType.None)
                {
                    if (!this.IsProtocolSecure && !this.IsEncryptionAvailable)
                    {
                        this.Listener.DebugReturn(DebugLevel.ERROR, "OpAuthenticate() failed. When you want Custom Authentication encryption is mandatory.");
                        return false;
                    }

                    opParameters[ParameterCode.ClientAuthenticationType] = (byte)authValues.AuthType;
                    if (!string.IsNullOrEmpty(authValues.Token))
                    {
                        opParameters[ParameterCode.Secret] = authValues.Token;
                    }
                    //else
                    //{
                    if (!string.IsNullOrEmpty(authValues.AuthGetParameters))
                    {
                        opParameters[ParameterCode.ClientAuthenticationParams] = authValues.AuthGetParameters;
                    }
                    if (authValues.AuthPostData != null)
                    {
                        opParameters[ParameterCode.ClientAuthenticationData] = authValues.AuthPostData;
                    }
                    //}
                }
            }

            bool sent = this.OpCustom(OperationCode.Authenticate, opParameters, true, (byte)0, this.IsEncryptionAvailable);
            if (!sent)
            {
                this.Listener.DebugReturn(DebugLevel.ERROR, "Error calling OpAuthenticate! Did not work. Check log output, CustomAuthenticationValues and if you're connected.");
            }
            return sent;
        }


        /// <summary>
        /// Operation to handle this client's interest groups (for events in room).
        /// </summary>
        /// <remarks>
        /// Note the difference between passing null and byte[0]:
        ///   null won't add/remove any groups.
        ///   byte[0] will add/remove all (existing) groups.
        /// First, removing groups is executed. This way, you could leave all groups and join only the ones provided.
        /// </remarks>
        /// <param name="groupsToRemove">Groups to remove from interest. Null will not remove any. A byte[0] will remove all.</param>
        /// <param name="groupsToAdd">Groups to add to interest. Null will not add any. A byte[0] will add all current.</param>
        /// <returns></returns>
        public virtual bool OpChangeGroups(byte[] groupsToRemove, byte[] groupsToAdd)
        {
            if (this.DebugOut >= DebugLevel.ALL)
            {
                this.Listener.DebugReturn(DebugLevel.ALL, "OpChangeGroups()");
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            if (groupsToRemove != null)
            {
                opParameters[(byte)ParameterCode.Remove] = groupsToRemove;
            }
            if (groupsToAdd != null)
            {
                opParameters[(byte)ParameterCode.Add] = groupsToAdd;
            }

            return this.OpCustom((byte)OperationCode.ChangeGroups, opParameters, true, 0);
        }


        /// <summary>
        /// Send an event with custom code/type and any content to the other players in the same room.
        /// </summary>
        /// <remarks>This override explicitly uses another parameter order to not mix it up with the implementation for Hashtable only.</remarks>
        /// <param name="eventCode">Identifies this type of event (and the content). Your game's event codes can start with 0.</param>
        /// <param name="customEventContent">Any serializable datatype (including Hashtable like the other OpRaiseEvent overloads).</param>
        /// <param name="sendReliable">If this event has to arrive reliably (potentially repeated if it's lost).</param>
        /// <param name="raiseEventOptions">Contains (slightly) less often used options. If you pass null, the default options will be used.</param>
        /// <returns>If operation could be enqueued for sending. Sent when calling: Service or SendOutgoingCommands.</returns>
        public virtual bool OpRaiseEvent(byte eventCode, object customEventContent, bool sendReliable, RaiseEventOptions raiseEventOptions)
        {
            opParameters.Clear();   // re-used private variable to avoid many new Dictionary() calls (garbage collection)
            opParameters[(byte)ParameterCode.Code] = (byte)eventCode;
            if (customEventContent != null)
            {
                opParameters[(byte) ParameterCode.Data] = customEventContent;
            }

            if (raiseEventOptions == null)
            {
                raiseEventOptions = RaiseEventOptions.Default;
            }
            else
            {
                if (raiseEventOptions.CachingOption != EventCaching.DoNotCache)
                {
                    opParameters[(byte) ParameterCode.Cache] = (byte) raiseEventOptions.CachingOption;
                }
                if (raiseEventOptions.Receivers != ReceiverGroup.Others)
                {
                    opParameters[(byte) ParameterCode.ReceiverGroup] = (byte) raiseEventOptions.Receivers;
                }
                if (raiseEventOptions.InterestGroup != 0)
                {
                    opParameters[(byte) ParameterCode.Group] = (byte) raiseEventOptions.InterestGroup;
                }
                if (raiseEventOptions.TargetActors != null)
                {
                    opParameters[(byte) ParameterCode.ActorList] = raiseEventOptions.TargetActors;
                }
                if (raiseEventOptions.ForwardToWebhook)
                {
                    opParameters[(byte) ParameterCode.EventForward] = true; //TURNBASED
                }
            }

            return this.OpCustom((byte)OperationCode.RaiseEvent, opParameters, sendReliable, raiseEventOptions.SequenceChannel, raiseEventOptions.Encrypt);
        }
    }



    /// <summary>
    /// ErrorCode defines the default codes associated with Photon client/server communication.
    /// </summary>
    public class ErrorCode
    {
        /// <summary>(0) is always "OK", anything else an error or specific situation.</summary>
        public const int Ok = 0;

        // server - Photon low(er) level: <= 0

        /// <summary>
        /// (-3) Operation can't be executed yet (e.g. OpJoin can't be called before being authenticated, RaiseEvent cant be used before getting into a room).
        /// </summary>
        /// <remarks>
        /// Before you call any operations on the Cloud servers, the automated client workflow must complete its authorization.
        /// In PUN, wait until State is: JoinedLobby (with AutoJoinLobby = true) or ConnectedToMaster (AutoJoinLobby = false)
        /// </remarks>
        public const int OperationNotAllowedInCurrentState = -3;

        /// <summary>(-2) The operation you called is not implemented on the server (application) you connect to. Make sure you run the fitting applications.</summary>
        [Obsolete("Use InvalidOperation.")]
        public const int InvalidOperationCode = -2;

        /// <summary>(-2) The operation you called could not be executed on the server.</summary>
        /// <remarks>
        /// Make sure you are connected to the server you expect.
        ///
        /// This code is used in several cases:
        /// The arguments/parameters of the operation might be out of range, missing entirely or conflicting.
        /// The operation you called is not implemented on the server (application). Server-side plugins affect the available operations.
        /// </remarks>
        public const int InvalidOperation = -2;

        /// <summary>(-1) Something went wrong in the server. Try to reproduce and contact Exit Games.</summary>
        public const int InternalServerError = -1;

        // server - PhotonNetwork: 0x7FFF and down
        // logic-level error codes start with short.max

        /// <summary>(32767) Authentication failed. Possible cause: AppId is unknown to Photon (in cloud service).</summary>
        public const int InvalidAuthentication = 0x7FFF;

        /// <summary>(32766) GameId (name) already in use (can't create another). Change name.</summary>
        public const int GameIdAlreadyExists = 0x7FFF - 1;

        /// <summary>(32765) Game is full. This rarely happens when some player joined the room before your join completed.</summary>
        public const int GameFull = 0x7FFF - 2;

        /// <summary>(32764) Game is closed and can't be joined. Join another game.</summary>
        public const int GameClosed = 0x7FFF - 3;

        [Obsolete("No longer used, cause random matchmaking is no longer a process.")]
        public const int AlreadyMatched = 0x7FFF - 4;

        /// <summary>(32762) Not in use currently.</summary>
        public const int ServerFull = 0x7FFF - 5;

        /// <summary>(32761) Not in use currently.</summary>
        public const int UserBlocked = 0x7FFF - 6;

        /// <summary>(32760) Random matchmaking only succeeds if a room exists thats neither closed nor full. Repeat in a few seconds or create a new room.</summary>
        public const int NoRandomMatchFound = 0x7FFF - 7;

        /// <summary>(32758) Join can fail if the room (name) is not existing (anymore). This can happen when players leave while you join.</summary>
        public const int GameDoesNotExist = 0x7FFF - 9;

        /// <summary>(32757) Authorization on the Photon Cloud failed becaus the concurrent users (CCU) limit of the app's subscription is reached.</summary>
        /// <remarks>
        /// Unless you have a plan with "CCU Burst", clients might fail the authentication step during connect.
        /// Affected client are unable to call operations. Please note that players who end a game and return
        /// to the master server will disconnect and re-connect, which means that they just played and are rejected
        /// in the next minute / re-connect.
        /// This is a temporary measure. Once the CCU is below the limit, players will be able to connect an play again.
        ///
        /// OpAuthorize is part of connection workflow but only on the Photon Cloud, this error can happen.
        /// Self-hosted Photon servers with a CCU limited license won't let a client connect at all.
        /// </remarks>
        public const int MaxCcuReached = 0x7FFF - 10;

        /// <summary>(32756) Authorization on the Photon Cloud failed because the app's subscription does not allow to use a particular region's server.</summary>
        /// <remarks>
        /// Some subscription plans for the Photon Cloud are region-bound. Servers of other regions can't be used then.
        /// Check your master server address and compare it with your Photon Cloud Dashboard's info.
        /// https://cloud.exitgames.com/dashboard
        ///
        /// OpAuthorize is part of connection workflow but only on the Photon Cloud, this error can happen.
        /// Self-hosted Photon servers with a CCU limited license won't let a client connect at all.
        /// </remarks>
        public const int InvalidRegion = 0x7FFF - 11;

        /// <summary>
        /// (32755) Custom Authentication of the user failed due to setup reasons (see Cloud Dashboard) or the provided user data (like username or token). Check error message for details.
        /// </summary>
        public const int CustomAuthenticationFailed = 0x7FFF - 12;

        /// <summary>(32753) The Authentication ticket expired. Usually, this is refreshed behind the scenes. Connect (and authorize) again.</summary>
        public const int AuthenticationTicketExpired = 0x7FF1;

        /// <summary>
        /// (32752) A server-side plugin (or webhook) failed to execute and reported an error. Check the OperationResponse.DebugMessage.
        /// </summary>
        public const int PluginReportedError = 0x7FFF - 15;

        /// <summary>
        /// (32751) CreateGame/JoinGame/Join operation fails if expected plugin does not correspond to loaded one.
        /// </summary>
        public const int PluginMismatch = 0x7FFF - 16;


    }


    /// <summary>
    /// Class for constants. These (byte) values define "well known" properties for an Actor / Player.
    /// Pun uses these constants internally.
    /// </summary>
    /// <remarks>
    /// "Custom properties" have to use a string-type as key. They can be assigned at will.
    /// </remarks>
    public class ActorProperties
    {
        /// <summary>(255) Name of a player/actor.</summary>
        public const byte PlayerName = 255; // was: 1

        /// <summary>(254) Tells you if the player is currently in this game (getting events live).</summary>
        /// <remarks>A server-set value for async games, where players can leave the game and return later.</remarks>
        public const byte IsInactive = 254;
    }


    /// <summary>
    /// Class for constants. These (byte) values are for "well known" room/game properties used in Photon Loadbalancing.
    /// Pun uses these constants internally.
    /// </summary>
    /// <remarks>
    /// "Custom properties" have to use a string-type as key. They can be assigned at will.
    /// </remarks>
    public class GameProperties
    {
        /// <summary>(255) Max number of players that "fit" into this room. 0 is for "unlimited".</summary>
        public const byte MaxPlayers = 255;
        /// <summary>(254) Makes this room listed or not in the lobby on master.</summary>
        public const byte IsVisible = 254;
        /// <summary>(253) Allows more players to join a room (or not).</summary>
        public const byte IsOpen = 253;
        /// <summary>(252) Current count of players in the room. Used only in the lobby on master.</summary>
        public const byte PlayerCount = 252;
        /// <summary>(251) True if the room is to be removed from room listing (used in update to room list in lobby on master)</summary>
        public const byte Removed = 251;
        /// <summary>(250) A list of the room properties to pass to the RoomInfo list in a lobby. This is used in CreateRoom, which defines this list once per room.</summary>
        public const byte PropsListedInLobby = 250;
        /// <summary>(249) Equivalent of Operation Join parameter CleanupCacheOnLeave.</summary>
        public const byte CleanupCacheOnLeave = 249;

        /// <summary>(248) Code for MasterClientId, which is synced by server. When sent as op-parameter this is (byte)203. As room property this is (byte)248.</summary>
        /// <remarks>Tightly related to ParameterCode.MasterClientId.</remarks>
        public const byte MasterClientId = (byte)248;
    }


    /// <summary>
    /// Class for constants. These values are for events defined by Photon Loadbalancing.
    /// Pun uses these constants internally.
    /// </summary>
    /// <remarks>They start at 255 and go DOWN. Your own in-game events can start at 0.</remarks>
    public class EventCode
    {
        /// <summary>(230) Initial list of RoomInfos (in lobby on Master)</summary>
        public const byte GameList = 230;

        /// <summary>(229) Update of RoomInfos to be merged into "initial" list (in lobby on Master)</summary>
        public const byte GameListUpdate = 229;

        /// <summary>(228) Currently not used. State of queueing in case of server-full</summary>
        public const byte QueueState = 228;

        /// <summary>(227) Currently not used. Event for matchmaking</summary>
        public const byte Match = 227;

        /// <summary>(226) Event with stats about this application (players, rooms, etc)</summary>
        public const byte AppStats = 226;
        /// <summary>(224) This event provides a list of lobbies with their player and game counts.</summary>
        public const byte TypedLobbyStats = 224;
        /// <summary>(210) Internally used in case of hosting by Azure</summary>
        [Obsolete("TCP routing was removed after becoming obsolete.")]
        public const byte AzureNodeInfo = 210;

        /// <summary>(255) Event Join: someone joined the game. The new actorNumber is provided as well as the properties of that actor (if set in OpJoin).</summary>
        public const byte Join = (byte)255;

        /// <summary>(254) Event Leave: The player who left the game can be identified by the actorNumber.</summary>
        public const byte Leave = (byte)254;

        /// <summary>(253) When you call OpSetProperties with the broadcast option "on", this event is fired. It contains the properties being set.</summary>
        public const byte PropertiesChanged = (byte)253;

        /// <summary>(253) When you call OpSetProperties with the broadcast option "on", this event is fired. It contains the properties being set.</summary>
        [Obsolete("Use PropertiesChanged now.")]
        public const byte SetProperties = (byte)253;

        /// <summary>(252) When player left game unexpected and the room has a playerTtl > 0, this event is fired to let everyone know about the timeout.</summary>
        /// Obsolete. Replaced by Leave. public const byte Disconnect = LiteEventCode.Disconnect;

        /// <summary>(251) Sent by Photon Cloud when a plugin-call failed. Usually, the execution on the server continues, despite the issue. Contains: ParameterCode.Info.</summary>
        public const byte ErrorInfo = 251;

        /// <summary>(250) Sent by Photon whent he event cache slice was changed. Done by OpRaiseEvent.</summary>
        public const byte CacheSliceChanged = 250;
    }


    /// <summary>
    /// Class for constants. Codes for parameters of Operations and Events.
    /// Pun uses these constants internally.
    /// </summary>
    public class ParameterCode
    {
        /// <summary>(237) A bool parameter for creating games. If set to true, no room events are sent to the clients on join and leave. Default: false (and not sent).</summary>
        public const byte SuppressRoomEvents = 237;

        /// <summary>(236) Time To Live (TTL) for a room when the last player leaves. Keeps room in memory for case a player re-joins soon. In milliseconds.</summary>
        public const byte EmptyRoomTTL = 236;

        /// <summary>(235) Time To Live (TTL) for an 'actor' in a room. If a client disconnects, this actor is inactive first and removed after this timeout. In milliseconds.</summary>
        public const byte PlayerTTL = 235;

        /// <summary>(234) Optional parameter of OpRaiseEvent to forward the event to some web-service.</summary>
        public const byte EventForward = 234;

        /// <summary>(233) Optional parameter of OpLeave in async games. If false, the player does abandons the game (forever). By default players become inactive and can re-join.</summary>
        public const byte IsComingBack = (byte)233;

        /// <summary>(233) Used in EvLeave to describe if a user is inactive (and might come back) or not. In async / Turnbased games, inactive is default.</summary>
        public const byte IsInactive = (byte)233;

        /// <summary>(232) Used when creating rooms to define if any userid can join the room only once.</summary>
        public const byte CheckUserOnJoin = (byte)232;

        /// <summary>(231) Code for "Check And Swap" (CAS) when changing properties.</summary>
        public const byte ExpectedValues = (byte)231;

        /// <summary>(230) Address of a (game) server to use.</summary>
        public const byte Address = 230;

        /// <summary>(229) Count of players in this application in a rooms (used in stats event)</summary>
        public const byte PeerCount = 229;

        /// <summary>(228) Count of games in this application (used in stats event)</summary>
        public const byte GameCount = 228;

        /// <summary>(227) Count of players on the master server (in this app, looking for rooms)</summary>
        public const byte MasterPeerCount = 227;

        /// <summary>(225) User's ID</summary>
        public const byte UserId = 225;

        /// <summary>(224) Your application's ID: a name on your own Photon or a GUID on the Photon Cloud</summary>
        public const byte ApplicationId = 224;

        /// <summary>(223) Not used currently (as "Position"). If you get queued before connect, this is your position</summary>
        public const byte Position = 223;

        /// <summary>(223) Modifies the matchmaking algorithm used for OpJoinRandom. Allowed parameter values are defined in enum MatchmakingMode.</summary>
        public const byte MatchMakingType = 223;

        /// <summary>(222) List of RoomInfos about open / listed rooms</summary>
        public const byte GameList = 222;

        /// <summary>(221) Internally used to establish encryption</summary>
        public const byte Secret = 221;

        /// <summary>(220) Version of your application</summary>
        public const byte AppVersion = 220;

        /// <summary>(210) Internally used in case of hosting by Azure</summary>
        [Obsolete("TCP routing was removed after becoming obsolete.")]
        public const byte AzureNodeInfo = 210;	// only used within events, so use: EventCode.AzureNodeInfo

        /// <summary>(209) Internally used in case of hosting by Azure</summary>
        [Obsolete("TCP routing was removed after becoming obsolete.")]
        public const byte AzureLocalNodeId = 209;

        /// <summary>(208) Internally used in case of hosting by Azure</summary>
        [Obsolete("TCP routing was removed after becoming obsolete.")]
        public const byte AzureMasterNodeId = 208;

        /// <summary>(255) Code for the gameId/roomName (a unique name per room). Used in OpJoin and similar.</summary>
        public const byte RoomName = (byte)255;

        /// <summary>(250) Code for broadcast parameter of OpSetProperties method.</summary>
        public const byte Broadcast = (byte)250;

        /// <summary>(252) Code for list of players in a room. Currently not used.</summary>
        public const byte ActorList = (byte)252;

        /// <summary>(254) Code of the Actor of an operation. Used for property get and set.</summary>
        public const byte ActorNr = (byte)254;

        /// <summary>(249) Code for property set (Hashtable).</summary>
        public const byte PlayerProperties = (byte)249;

        /// <summary>(245) Code of data/custom content of an event. Used in OpRaiseEvent.</summary>
        public const byte CustomEventContent = (byte)245;

        /// <summary>(245) Code of data of an event. Used in OpRaiseEvent.</summary>
        public const byte Data = (byte)245;

        /// <summary>(244) Code used when sending some code-related parameter, like OpRaiseEvent's event-code.</summary>
        /// <remarks>This is not the same as the Operation's code, which is no longer sent as part of the parameter Dictionary in Photon 3.</remarks>
        public const byte Code = (byte)244;

        /// <summary>(248) Code for property set (Hashtable).</summary>
        public const byte GameProperties = (byte)248;

        /// <summary>
        /// (251) Code for property-set (Hashtable). This key is used when sending only one set of properties.
        /// If either ActorProperties or GameProperties are used (or both), check those keys.
        /// </summary>
        public const byte Properties = (byte)251;

        /// <summary>(253) Code of the target Actor of an operation. Used for property set. Is 0 for game</summary>
        public const byte TargetActorNr = (byte)253;

        /// <summary>(246) Code to select the receivers of events (used in Lite, Operation RaiseEvent).</summary>
        public const byte ReceiverGroup = (byte)246;

        /// <summary>(247) Code for caching events while raising them.</summary>
        public const byte Cache = (byte)247;

        /// <summary>(241) Bool parameter of CreateGame Operation. If true, server cleans up roomcache of leaving players (their cached events get removed).</summary>
        public const byte CleanupCacheOnLeave = (byte)241;

        /// <summary>(240) Code for "group" operation-parameter (as used in Op RaiseEvent).</summary>
        public const byte Group = 240;

        /// <summary>(239) The "Remove" operation-parameter can be used to remove something from a list. E.g. remove groups from player's interest groups.</summary>
        public const byte Remove = 239;

        /// <summary>(238) The "Add" operation-parameter can be used to add something to some list or set. E.g. add groups to player's interest groups.</summary>
        public const byte Add = 238;

        /// <summary>(218) Content for EventCode.ErrorInfo and internal debug operations.</summary>
        public const byte Info = 218;

        /// <summary>(217) This key's (byte) value defines the target custom authentication type/service the client connects with. Used in OpAuthenticate</summary>
        public const byte ClientAuthenticationType = 217;

        /// <summary>(216) This key's (string) value provides parameters sent to the custom authentication type/service the client connects with. Used in OpAuthenticate</summary>
        public const byte ClientAuthenticationParams = 216;

        /// <summary>(215) Makes the server create a room if it doesn't exist. OpJoin uses this to always enter a room, unless it exists and is full/closed.</summary>
        public const byte CreateIfNotExists = 215;

        /// <summary>(215) The JoinMode enum defines which variant of joining a room will be executed: Join only if available, create if not exists or re-join.</summary>
        /// <remarks>Replaces CreateIfNotExists which was only a bool-value.</remarks>
        public const byte JoinMode = 215;

        /// <summary>(214) This key's (string or byte[]) value provides parameters sent to the custom authentication service setup in Photon Dashboard. Used in OpAuthenticate</summary>
        public const byte ClientAuthenticationData = 214;

        /// <summary>(203) Code for MasterClientId, which is synced by server. When sent as op-parameter this is code 203.</summary>
        /// <remarks>Tightly related to GameProperties.MasterClientId.</remarks>
        public const byte MasterClientId = (byte)203;

        /// <summary>(1) Used in Op FindFriends request. Value must be string[] of friends to look up.</summary>
        public const byte FindFriendsRequestList = (byte)1;

        /// <summary>(1) Used in Op FindFriends response. Contains bool[] list of online states (false if not online).</summary>
        public const byte FindFriendsResponseOnlineList = (byte)1;

        /// <summary>(2) Used in Op FindFriends response. Contains string[] of room names ("" where not known or no room joined).</summary>
        public const byte FindFriendsResponseRoomIdList = (byte)2;

        /// <summary>(213) Used in matchmaking-related methods and when creating a room to name a lobby (to join or to attach a room to).</summary>
        public const byte LobbyName = (byte)213;

        /// <summary>(212) Used in matchmaking-related methods and when creating a room to define the type of a lobby. Combined with the lobby name this identifies the lobby.</summary>
        public const byte LobbyType = (byte)212;

        /// <summary>(211) This (optional) parameter can be sent in Op Authenticate to turn on Lobby Stats (info about lobby names and their user- and game-counts). See: PhotonNetwork.Lobbies</summary>
        public const byte LobbyStats = (byte)211;

        /// <summary>(210) Used for region values in OpAuth and OpGetRegions.</summary>
        public const byte Region = (byte)210;

        /// <summary>(209) Path of the WebRPC that got called. Also known as "WebRpc Name". Type: string.</summary>
        public const byte UriPath = 209;

        /// <summary>(208) Parameters for a WebRPC as: Dictionary<string, object>. This will get serialized to JSon.</summary>
        public const byte WebRpcParameters = 208;

        /// <summary>(207) ReturnCode for the WebRPC, as sent by the web service (not by Photon, which uses ErrorCode). Type: byte.</summary>
        public const byte WebRpcReturnCode = 207;

        /// <summary>(206) Message returned by WebRPC server. Analog to Photon's debug message. Type: string.</summary>
        public const byte WebRpcReturnMessage = 206;

        /// <summary>(205) Used to define a "slice" for cached events. Slices can easily be removed from cache. Type: int.</summary>
        public const byte CacheSliceIndex = 205;

        /// <summary>
        /// Informs the server of the expected plugin setup.
        /// The operation will fail in case of a plugin mismatch returning error code PluginMismatch 32751(0x7FFF - 16).
        /// Setting string[]{} means the client expects no plugin to be setup.
        /// Note: for backwards compatibility null omits any check.
        /// </summary>
        public const byte Plugins = 204;

        /// <summary>(201) Informs user about name of plugin load to game</summary>
        public const byte PluginName = 201;

        /// <summary>(200) Informs user about version of plugin load to game</summary>
        public const byte PluginVersion = 200;
    }


    /// <summary>
    /// Class for constants. Contains operation codes.
    /// Pun uses these constants internally.
    /// </summary>
    public class OperationCode
    {

        [Obsolete("Exchanging encrpytion keys is done internally in the lib now. Don't expect this operation-result.")]
        public const byte ExchangeKeysForEncryption = 250;

        /// <summary>(255) Code for OpJoin, to get into a room.</summary>
        public const byte Join = 255;

        /// <summary>(230) Authenticates this peer and connects to a virtual application</summary>
        public const byte Authenticate = 230;

        /// <summary>(229) Joins lobby (on master)</summary>
        public const byte JoinLobby = 229;

        /// <summary>(228) Leaves lobby (on master)</summary>
        public const byte LeaveLobby = 228;

        /// <summary>(227) Creates a game (or fails if name exists)</summary>
        public const byte CreateGame = 227;

        /// <summary>(226) Join game (by name)</summary>
        public const byte JoinGame = 226;

        /// <summary>(225) Joins random game (on master)</summary>
        public const byte JoinRandomGame = 225;

        // public const byte CancelJoinRandom = 224; // obsolete, cause JoinRandom no longer is a "process". now provides result immediately

        /// <summary>(254) Code for OpLeave, to get out of a room.</summary>
        public const byte Leave = (byte)254;

        /// <summary>(253) Raise event (in a room, for other actors/players)</summary>
        public const byte RaiseEvent = (byte)253;

        /// <summary>(252) Set Properties (of room or actor/player)</summary>
        public const byte SetProperties = (byte)252;

        /// <summary>(251) Get Properties</summary>
        public const byte GetProperties = (byte)251;

        /// <summary>(248) Operation code to change interest groups in Rooms (Lite application and extending ones).</summary>
        public const byte ChangeGroups = (byte)248;

        /// <summary>(222) Request the rooms and online status for a list of friends (by name, which should be unique).</summary>
        public const byte FindFriends = 222;

        /// <summary>(221) Request statistics about a specific list of lobbies (their user and game count).</summary>
        public const byte GetLobbyStats = 221;

        /// <summary>(220) Get list of regional servers from a NameServer.</summary>
        public const byte GetRegions = 220;

        /// <summary>(219) WebRpc Operation.</summary>
        public const byte WebRpc = 219;
    }


    /// <summary>
    /// Lite - OpRaiseEvent lets you chose which actors in the room should receive events.
    /// By default, events are sent to "Others" but you can overrule this.
    /// </summary>
    public enum ReceiverGroup : byte
    {
        /// <summary>Default value (not sent). Anyone else gets my event.</summary>
        Others = 0,

        /// <summary>Everyone in the current room (including this peer) will get this event.</summary>
        All = 1,

        /// <summary>The server sends this event only to the actor with the lowest actorNumber.</summary>
        /// <remarks>The "master client" does not have special rights but is the one who is in this room the longest time.</remarks>
        MasterClient = 2,
    }

    /// <summary>
    /// Lite - OpRaiseEvent allows you to cache events and automatically send them to joining players in a room.
    /// Events are cached per event code and player: Event 100 (example!) can be stored once per player.
    /// Cached events can be modified, replaced and removed.
    /// </summary>
    /// <remarks>
    /// Caching works only combination with ReceiverGroup options Others and All.
    /// </remarks>
    public enum EventCaching : byte
    {
        /// <summary>Default value (not sent).</summary>
        DoNotCache = 0,

        /// <summary>Will merge this event's keys with those already cached.</summary>
        [Obsolete]
        MergeCache = 1,

        /// <summary>Replaces the event cache for this eventCode with this event's content.</summary>
        [Obsolete]
        ReplaceCache = 2,

        /// <summary>Removes this event (by eventCode) from the cache.</summary>
        [Obsolete]
        RemoveCache = 3,

        /// <summary>Adds an event to the room's cache</summary>
        AddToRoomCache = 4,

        /// <summary>Adds this event to the cache for actor 0 (becoming a "globally owned" event in the cache).</summary>
        AddToRoomCacheGlobal = 5,

        /// <summary>Remove fitting event from the room's cache.</summary>
        RemoveFromRoomCache = 6,

        /// <summary>Removes events of players who already left the room (cleaning up).</summary>
        RemoveFromRoomCacheForActorsLeft = 7,

        /// <summary>Increase the index of the sliced cache.</summary>
        SliceIncreaseIndex = 10,

        /// <summary>Set the index of the sliced cache. You must set RaiseEventOptions.CacheSliceIndex for this.</summary>
        SliceSetIndex = 11,

        /// <summary>Purge cache slice with index. Exactly one slice is removed from cache. You must set RaiseEventOptions.CacheSliceIndex for this.</summary>
        SlicePurgeIndex = 12,

        /// <summary>Purge cache slices with specified index and anything lower than that. You must set RaiseEventOptions.CacheSliceIndex for this.</summary>
        SlicePurgeUpToIndex = 13,
    }

    /// <summary>
    /// Flags for "types of properties", being used as filter in OpGetProperties.
    /// </summary>
    [Flags]
    public enum PropertyTypeFlag : byte
    {
        /// <summary>(0x00) Flag type for no property type.</summary>
        None = 0x00,

        /// <summary>(0x01) Flag type for game-attached properties.</summary>
        Game = 0x01,

        /// <summary>(0x02) Flag type for actor related propeties.</summary>
        Actor = 0x02,

        /// <summary>(0x01) Flag type for game AND actor properties. Equal to 'Game'</summary>
        GameAndActor = Game | Actor
    }
}


/// <summary>
/// Options for matchmaking rules for OpJoinRandom.
/// </summary>
public enum MatchmakingMode : byte
{
    /// <summary>Fills up rooms (oldest first) to get players together as fast as possible. Default.</summary>
    /// <remarks>Makes most sense with MaxPlayers > 0 and games that can only start with more players.</remarks>
    FillRoom = 0,
    /// <summary>Distributes players across available rooms sequentially but takes filter into account. Without filter, rooms get players evenly distributed.</summary>
    SerialMatching = 1,
    /// <summary>Joins a (fully) random room. Expected properties must match but aside from this, any available room might be selected.</summary>
    RandomMatching = 2
}

/// <summary>
/// Options for optional "Custom Authentication" services used with Photon. Used by OpAuthenticate after connecting to Photon.
/// </summary>
public enum CustomAuthenticationType : byte
{
    /// <summary>Use a custom authentification service. Currently the only implemented option.</summary>
    Custom = 0,

    /// <summary>Authenticates users by their Steam Account. Set auth values accordingly!</summary>
    Steam = 1,

    /// <summary>Authenticates users by their Facebook Account. Set auth values accordingly!</summary>
    Facebook = 2,

    /// <summary>Disables custom authentification. Same as not providing any AuthenticationValues for connect (more precisely for: OpAuthenticate).</summary>
    None = byte.MaxValue
}


/// <summary>
/// Container for user authentication in Photon. Set AuthValues before you connect - all else is handled.
/// </summary>
/// <remarks>
/// On Photon, user authentication is optional but can be useful in many cases. 
/// If you want to FindFriends, a unique ID per user is very practical.
/// 
/// There are basically three options for user authentification: None at all, the client sets some UserId
/// or you can use some account web-service to authenticate a user (and set the UserId server-side).
/// 
/// Custom Authentication lets you verify end-users by some kind of login or token. It sends those
/// values to Photon which will verify them before granting access or disconnecting the client.
///
/// The Photon Cloud Dashboard will let you enable this feature and set important server values for it.
/// https://www.exitgames.com/dashboard
/// </remarks>
public class AuthenticationValues
{
    /// <summary>The type of custom authentication provider that should be used. Currently only "Custom" or "None" (turns this off).</summary>
    public CustomAuthenticationType AuthType = CustomAuthenticationType.None;

    /// <summary>This string must contain any (http get) parameters expected by the used authentication service. By default, username and token.</summary>
    /// <remarks>Standard http get parameters are used here and passed on to the service that's defined in the server (Photon Cloud Dashboard).</remarks>
    public string AuthGetParameters;

    /// <summary>Data to be passed-on to the auth service via POST. Default: null (not sent). Either string or byte[] (see setters).</summary>
    public object AuthPostData { get; private set; }

    /// <summary>After initial authentication, Photon provides a token for this client / user, which is subsequently used as (cached) validation.</summary>
    public string Token;


    /// <summary>The UserId should be a unique identifier per user. This is for finding friends, etc..</summary>
    public string UserId { get; set; }


    /// <summary>Creates empty auth values without any info.</summary>
    public AuthenticationValues()
    {
    }

    /// <summary>Creates minimal info about the user. If this is authenticated or not, depends on the set AuthType.</summary>
    /// <param name="userId">Some UserId to set in Photon.</param>
    public AuthenticationValues(string userId)
    {
        this.UserId = userId;
    }

    /// <summary>Sets the data to be passed-on to the auth service via POST.</summary>
    /// <param name="byteData">Binary token / auth-data to pass on. Empty string will set AuthPostData to null.</param>
    public virtual void SetAuthPostData(string stringData)
    {
        this.AuthPostData = (string.IsNullOrEmpty(stringData)) ? null : stringData;
    }

    /// <summary>Sets the data to be passed-on to the auth service via POST.</summary>
    /// <param name="byteData">Binary token / auth-data to pass on.</param>
    public virtual void SetAuthPostData(byte[] byteData)
    {
        this.AuthPostData = byteData;
    }

    /// <summary>Adds a key-value pair to the get-parameters used for Custom Auth.</summary>
    /// <remarks>This method does uri-encoding for you.</remarks>
    /// <param name="key">Key for the value to set.</param>
    /// <param name="value">Some value relevant for Custom Authentication.</param>
    public virtual void AddAuthParameter(string key, string value)
    {
        string ampersand = string.IsNullOrEmpty(this.AuthGetParameters) ? "" : "&";
        this.AuthGetParameters = string.Format("{0}{1}{2}={3}", this.AuthGetParameters, ampersand, System.Uri.EscapeDataString(key), System.Uri.EscapeDataString(value));
    }

    public override string ToString()
    {
        return string.Format("AuthenticationValues UserId: {0}, GetParameters: {1} Token available: {2}", UserId, this.AuthGetParameters, Token != null);
    }
}

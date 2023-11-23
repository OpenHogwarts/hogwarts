// ----------------------------------------------------------------------------
// <copyright file="AccountService.cs" company="Exit Games GmbH">
//   Photon Cloud Account Service - Copyright (C) 2012 Exit Games GmbH
// </copyright>
// <summary>
//   Provides methods to register a new user-account for the Photon Cloud and
//   get the resulting appId.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

#if UNITY_EDITOR

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Net;

namespace ExitGames.Client.Photon
{
    public class AccountService
    {
        private const string ServiceUrl = "https://service.exitgames.com/AccountExt/AccountServiceExt.aspx";

        private Action<AccountService> registrationCallback; // optional (when using async reg)

        public string Message { get; private set; } // msg from server (in case of success, this is the appid)

        protected internal Exception Exception { get; set; } // exceptions in account-server communication

        public string AppId { get; private set; }

        public string AppId2 { get; private set; }

        public int ReturnCode { get; private set; } // 0 = OK. anything else is a error with Message

        public enum Origin : byte
        {
            ServerWeb = 1,
            CloudWeb = 2,
            Pun = 3,
            Playmaker = 4
        };

        /// <summary>
        /// Creates a instance of the Account Service to register Photon Cloud accounts.
        /// </summary>
        public AccountService()
        {
            WebRequest.DefaultWebProxy = null;
            ServicePointManager.ServerCertificateValidationCallback = Validator;
        }

        public static bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true; // any certificate is ok in this case
        }

        /// <summary>
        /// Attempts to create a Photon Cloud Account asynchronously.
        /// Once your callback is called, check ReturnCode, Message and AppId to get the result of this attempt.
        /// </summary>
        /// <param name="email">Email of the account.</param>
        /// <param name="origin">Marks which channel created the new account (if it's new).</param>
        /// <param name="serviceType">Defines which type of Photon-service is being requested.</param>
        /// <param name="callback">Called when the result is available.</param>
        public void RegisterByEmail(string email, Origin origin, string serviceType, Action<AccountService> callback = null)
        {
            this.registrationCallback = callback;
            this.AppId = string.Empty;
            this.AppId2 = string.Empty;
            this.Message = string.Empty;
            this.ReturnCode = -1;

            string url = this.RegistrationUri(email, (byte) origin, serviceType);
            PhotonEditorUtils.StartCoroutine(
                PhotonEditorUtils.HttpGet(url,
                    (s) =>
                    {
                        this.ParseResult(s);
                        if (this.registrationCallback != null)
                        {
                            this.registrationCallback(this);
                        }
                    },
                    (e) =>
                    {
                        this.Message = e;
                        if (this.registrationCallback != null)
                        {
                            this.registrationCallback(this);
                        }
                    })
                );
        }

        /// <summary>
        /// Creates the service-call Uri, escaping the email for security reasons.
        /// </summary>
        /// <param name="email">Email of the account.</param>
        /// <param name="origin">1 = server-web, 2 = cloud-web, 3 = PUN, 4 = playmaker</param>
        /// <param name="serviceType">Defines which type of Photon-service is being requested. Options: "", "voice", "chat"</param>
        /// <returns>Uri to call.</returns>
        private string RegistrationUri(string email, byte origin, string serviceType)
        {
            if (serviceType == null)
            {
                serviceType = string.Empty;
            }

            string emailEncoded = Uri.EscapeDataString(email);
            return string.Format("{0}?email={1}&origin={2}&serviceType={3}", ServiceUrl, emailEncoded, origin, serviceType);
        }

        /// <summary>
        /// Reads the Json response and applies it to local properties.
        /// </summary>
        /// <param name="result"></param>
        private void ParseResult(string result)
        {
            if (string.IsNullOrEmpty(result))
            {
                this.Message = "Server's response was empty. Please register through account website during this service interruption.";
                return;
            }

            try
            {
                AccountServiceResponse res = UnityEngine.JsonUtility.FromJson<AccountServiceResponse>(result);
                this.ReturnCode = res.ReturnCode;
                this.Message = res.Message;
                if (this.ReturnCode == 0)
                {
                    // returnCode == 0 means: all ok. message is new AppId
                    this.AppId = this.Message;
                    if (PhotonEditorUtils.HasVoice)
                    {
                        this.AppId2 = res.MessageDetailed;
                    }
                }
                else
                {
                    // any error gives returnCode != 0
                    this.AppId = string.Empty;
                    if (PhotonEditorUtils.HasVoice)
                    {
                        this.AppId2 = string.Empty;
                    }
                }
            }
            catch (Exception ex) // probably JSON parsing exception, check if returned string is valid JSON
            {
                this.ReturnCode = -1;
                this.Message = ex.Message;
            }
        }

    }

    [Serializable]
    public class AccountServiceResponse
    {
        public int ReturnCode;
        public string Message;
        public string MessageDetailed;
    }
}

#endif
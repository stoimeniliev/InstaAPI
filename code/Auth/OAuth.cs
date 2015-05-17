﻿using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.IO;

using InstaAPI.Entities;
using Newtonsoft.Json;

namespace InstaAPI.Auth
{
    [Serializable]
    public class OAuth
    {
        /// <summary>
        ///     <para>set up fields</para>
        /// </summary>
        private InstaConfig Config = null;
        private String GrantType = "authorization_code";
        private String Code;
        private String AccessTokenUri = "api.instagram.com/oauth/authorize";
        private AuthUser AuthorisedUser = null;
        private MetaData Meta = null;

        /*************************************************************** CONSTRUCTORS ****************************************************************/

        /// <summary>
        ///     <para>initialise with the required parameters</para>
        /// </summary>
        /// <param name="Config"></param>
        /// <param name="Code"></param>
        public OAuth(InstaConfig Config, String Code)
        {
            this.Config = Config;
            this.Code = Code;
            
            FetchAccessToken();
        }

        /************************************************************** OTHER METHODS ****************************************************************/

        /// <summary>
        ///     <para>verifies and sets the authorised user</para>
        /// </summary>
        private void FetchAccessToken()
        {
            try
            {
                WebClient Client = new WebClient();
                UriBuilder AuthenticationTokenRequestUri = new UriBuilder();
                NameValueCollection PostStrings = System.Web.HttpUtility.ParseQueryString(String.Empty);

                // SET THE POST VALUES
                PostStrings.Add("client_id", this.Config.GetClientId());
                PostStrings.Add("client_secret", this.Config.GetClientSecret());
                PostStrings.Add("grant_type", this.GrantType);
                PostStrings.Add("redirect_uri", this.Config.GetRedirectUriString());
                PostStrings.Add("code", this.Code);

                // SET UP REQUEST URI
                AuthenticationTokenRequestUri.Scheme = this.Config.GetUriScheme();
                AuthenticationTokenRequestUri.Host = this.AccessTokenUri;

                // STORE VALUES IN AUTHUSER
                AuthorisedUser = new AuthUser();

                // CREATE NEW META OBJECT AND FILL IN DATA
                Meta = new MetaData();

                // SEND POST REQUEST
                byte[] ResponseBytes = Client.UploadValues(AuthenticationTokenRequestUri.Uri, PostStrings);
                String Response = Encoding.UTF8.GetString(ResponseBytes);

                // PARSE JSON
                dynamic ParsedJson = JsonConvert.DeserializeObject(Response);

                Meta.Code = 200;
                AuthorisedUser.Meta = Meta;
                AuthorisedUser.AccessToken = ParsedJson.access_token;
                AuthorisedUser.UserId = ParsedJson.user.id;
                AuthorisedUser.UserName = ParsedJson.user.username;
                AuthorisedUser.FullName = ParsedJson.user.full_name;
                AuthorisedUser.Bio = ParsedJson.user.bio;
                AuthorisedUser.Website = ParsedJson.user.website;
                AuthorisedUser.ProfilePicture = ParsedJson.user.profile_picture;
            }
            catch (WebException WEx)
            {
                // FETCHES ANY ERROR THROWN BY INSTAGRAM API
                Stream ResponseStream = WEx.Response.GetResponseStream();
                if (ResponseStream != null)
                {
                    StreamReader ResponseReader = new StreamReader(ResponseStream);
                    if (ResponseReader != null)
                    {
                        // PARSE JSON
                        dynamic ParsedJson = JsonConvert.DeserializeObject(ResponseReader.ReadToEnd());

                        // CREATE NEW META OBJECT AND FILL IN DATA
                        Meta.Code = ParsedJson.code;
                        Meta.ErrorType = ParsedJson.error_type;
                        Meta.ErrorMessage = ParsedJson.error_message;
                        AuthorisedUser.Meta = Meta;
                    }
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.StackTrace);
            }
        }

        /************************************************************************************************************/

        /// <summary>
        ///     <para>gets the authorised user object</para>
        /// </summary>
        /// <returns></returns>
        public AuthUser GetAuhtorisedUser()
        {
            return this.AuthorisedUser;
        }      

    }
}

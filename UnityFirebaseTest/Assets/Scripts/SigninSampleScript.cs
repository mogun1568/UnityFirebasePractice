// <copyright file="SigninSampleScript.cs" company="Google Inc.">
// Copyright (C) 2017 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations

namespace SignInSample
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Firebase;
    using Firebase.Auth;
    using Google;
    using UnityEngine;
    using UnityEngine.UI;

    public class SigninSampleScript : MonoBehaviour
    {
        public Text infoText;

        public string webClientId = "291566476876-gsn5ofpikir2s34t120a24dpr9qub0vg.apps.googleusercontent.com";

        private FirebaseAuth auth;
        private GoogleSignInConfiguration configuration;

        // Defer the configuration creation until Awake so the web Client ID
        // Can be set via the property inspector in the Editor.
        void Awake()
        {
            configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
            CheckFirebaseDependencies();
        }

        private void CheckFirebaseDependencies()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    if (task.Result == DependencyStatus.Available)
                        auth = FirebaseAuth.DefaultInstance;
                    else
                        AddToInformation("Could not resolve all Firebase dependencies: " + task.Result.ToString());
                }
                else
                {
                    AddToInformation("Dependency check was not completed. Error : " + task.Exception.Message);
                }
            });
        }

        public void OnSignIn()
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
            GoogleSignIn.Configuration.RequestEmail = true;
            AddToInformation("Calling SignIn");

            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void OnSignOut()
        {
            AddToInformation("Calling SignOut");
            GoogleSignIn.DefaultInstance.SignOut();
        }

        public void OnDisconnect()
        {
            AddToInformation("Calling Disconnect");
            GoogleSignIn.DefaultInstance.Disconnect();
        }

        internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
        {
            if (task.IsFaulted)
            {
                using (IEnumerator<System.Exception> enumerator =
                        task.Exception.InnerExceptions.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        GoogleSignIn.SignInException error =
                                (GoogleSignIn.SignInException)enumerator.Current;
                        AddToInformation("Got Error: " + error.Status + " " + error.Message);
                    }
                    else
                    {
                        AddToInformation("Got Unexpected Exception?!?" + task.Exception);
                    }
                }
            }
            else if (task.IsCanceled)
            {
                AddToInformation("Canceled");
            }
            else
            {
                AddToInformation("Welcome: " + task.Result.DisplayName + "!");
                AddToInformation("Email = " + task.Result.Email);
                AddToInformation("Google ID Token = " + task.Result.IdToken);
                SignInWithGoogleOnFirebase(task.Result.IdToken);
            }
        }

        private void SignInWithGoogleOnFirebase(string idToken)
        {
            Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

            auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
            {
                AggregateException ex = task.Exception;
                if (ex != null)
                {
                    if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0))
                        AddToInformation("\nError code = " + inner.ErrorCode + " Message = " + inner.Message);
                }
                else
                {
                    AddToInformation("Sign In Successful.");
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void OnSignInSilently()
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
            AddToInformation("Calling SignIn Silently");

            GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished, TaskScheduler.FromCurrentSynchronizationContext());
        }


        public void OnGamesSignIn()
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = true;
            GoogleSignIn.Configuration.RequestIdToken = false;

            AddToInformation("Calling Games SignIn");

            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void AddToInformation(string str) { infoText.text += "\n" + str; }
    }
}
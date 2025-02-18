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
    using Firebase.Database;
    using Google;
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    public class SigninSampleScript : MonoBehaviour
    {
        public Text infoText;

        public string webClientId = "291566476876-gsn5ofpikir2s34t120a24dpr9qub0vg.apps.googleusercontent.com";

        private FirebaseAuth auth;
        private DatabaseReference dbReference;
        private GoogleSignInConfiguration configuration;

        // Defer the configuration creation until Awake so the web Client ID
        // Can be set via the property inspector in the Editor.
        void Awake()
        {
            configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestIdToken = true };
            CheckFirebaseDependencies();
        }

        private void CheckFirebaseDependencies()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    AddToInformation("Firebase Initialize Failed: " + task.Exception?.ToString());
                    return;
                }

                if (task.Result == DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;
                    dbReference = FirebaseDatabase.DefaultInstance.RootReference;

                    AddToInformation("Firebase is initialized successfully.");
                }
                else
                {
                    AddToInformation("Could not resolve all Firebase dependencies: " + task.Result.ToString());
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        public void OnSignIn()
        {
            AddToInformation("Calling SignIn");

            if (auth.CurrentUser != null)
            {
                AddToInformation("Already signed in as: " + auth.CurrentUser.DisplayName);
                return;  // 이미 로그인된 경우 함수 종료
            }

            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;

            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void OnSignOut()
        {
            AddToInformation("Calling SignOut");

            if (auth.CurrentUser == null)
            {
                AddToInformation("No user is signed in.");
                return; // 로그인된 사용자가 없으면 종료
            }

            auth.SignOut();
            GoogleSignIn.DefaultInstance.SignOut();
        }

        // 안됨
        public void OnDisconnect()
        {
            AddToInformation("Calling Disconnect");

            if (auth.CurrentUser == null)
            {
                AddToInformation("No user is signed in.");
                return; // 로그인된 사용자가 없으면 종료
            }

            GoogleSignIn.DefaultInstance.Disconnect();
            auth.SignOut();
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
                SignInWithGoogleOnFirebase(task.Result.IdToken);
            }
        }

        private void SignInWithGoogleOnFirebase(string idToken)
        {
            Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

            auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    AddToInformation("Sign-in Failed: " + task.Exception?.ToString());
                    return;
                }

                AddToInformation("Sign In Successful.");

                // 인증 완료 후 CurrentUser 확인
                if (auth.CurrentUser != null)
                {
                    AddToInformation("User is signed in: " + auth.CurrentUser.DisplayName);
                    LoadUserData();  // 사용자 데이터 로드
                }
                else
                {
                    AddToInformation("CurrentUser is null. Something went wrong.");
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void OnSignInSilently()
        {
            AddToInformation("Calling SignIn Silently");

            if (auth.CurrentUser != null)
            {
                AddToInformation("Already signed in as: " + auth.CurrentUser.DisplayName);
                return;  // 이미 로그인된 경우 함수 종료
            }

            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;

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

        private List<string> messages = new List<string>();
        private void AddToInformation(string text)
        {
            if (messages.Count == 5)
            {
                messages.RemoveAt(0);
            }
            messages.Add(text);
            string txt = "";
            foreach (string s in messages)
            {
                txt += "\n" + s;
            }
            infoText.text = txt;
        }


        private bool CheckFirebase()
        {
            if (auth == null)
            {
                AddToInformation("Firebase authentication is not initialized properly.");
                return false;
            }
            if (dbReference == null)
            {
                AddToInformation("Firebase Database is not initialized properly.");
                return false;
            }

            return true;
        }

        // 유저 데이터 저장 (전체 덮어쓰기)
        public void SaveUserData(UserData data)
        {
            if (!CheckFirebase()) return;

            FirebaseUser user = auth.CurrentUser;
            if (user == null)
            {
                AddToInformation("Login required.");
                return;
            }

            string userId = user.UserId;
            string jsonData = JsonUtility.ToJson(data);

            dbReference.Child("users").Child(userId).SetRawJsonValueAsync(jsonData).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    AddToInformation("User data saved successfully.");
                    LoadUserData();
                }
                else
                    AddToInformation("Failed to save user data: " + task.Exception);
            });
        }

        // 특정 데이터 저장 (개별 값 업데이트)
        public void SaveSingleData(string key, object value)
        {
            if (!CheckFirebase()) return;

            FirebaseUser user = auth.CurrentUser;
            if (user == null)
            {
                AddToInformation("Login required.");
                return;
            }

            string userId = user.UserId;
            dbReference.Child("users").Child(userId).Child(key).SetValueAsync(value).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    AddToInformation($"{key} saved successfully: {value}");
                    LoadUserData();
                }
                else
                    AddToInformation($"{key} save failed: " + task.Exception);
            });
        }

        // 유저 데이터 불러오기
        public void LoadUserData()
        {
            if (!CheckFirebase()) return;

            FirebaseUser user = auth.CurrentUser;
            if (user == null)
            {
                AddToInformation("Login required.");
                return;
            }

            string userId = user.UserId;
            dbReference.Child("users").Child(userId).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    AddToInformation("Failed to load data: " + task.Exception?.ToString());
                    return;
                }

                if (task.Result.Exists)
                {
                    string json = task.Result.GetRawJsonValue();
                    UserData userData = JsonUtility.FromJson<UserData>(json);
                    AddToInformation($"Loaded data: Gold {userData.gold}, Level {userData.level}, Exp {userData.exp}");
                }
                else
                {
                    AddToInformation("User data not found. Creating new data.");
                    UserData newUser = new UserData(100, 1, 0); // 기본값
                    SaveUserData(newUser);
                }
            });
        }

        // 특정 데이터 불러오기
        public void LoadSingleData(string key)
        {
            if (!CheckFirebase()) return;

            FirebaseUser user = auth.CurrentUser;
            if (user == null)
            {
                AddToInformation("Login required.");
                return;
            }

            string userId = user.UserId;
            dbReference.Child("users").Child(userId).Child(key).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    AddToInformation($"{key} loaded successfully: {task.Result.Value}");
                }
                else
                {
                    AddToInformation($"{key} data not found.");
                }
            });
        }

        public void ExampleUpdateUserData()
        {
            if (!CheckFirebase()) return;

            UpdateUserData("gold", 100);
            UpdateUserData("level", 1);
            UpdateUserData("exp", 10);
        }

        // 특정 데이터 업데이트 (+- 연산 가능)
        public void UpdateUserData(string key, int amount)
        {
            //if (!CheckFirebase()) return;

            FirebaseUser user = auth.CurrentUser;
            if (user == null)
            {
                AddToInformation("Login required.");
                return;
            }

            string userId = user.UserId;
            DatabaseReference dataRef = dbReference.Child("users").Child(userId).Child(key);

            dataRef.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    int currentValue = int.Parse(task.Result.Value.ToString());
                    int newValue = currentValue + amount;
                    dataRef.SetValueAsync(newValue);
                    AddToInformation($"{key} updated successfully: {newValue}"); 
                }
                else
                {
                    AddToInformation($"{key} data not found.");
                }
            });
        }

        // 특정 데이터 삭제
        public void DeleteSingleData(string key)
        {
            if (!CheckFirebase()) return;

            FirebaseUser user = auth.CurrentUser;
            if (user == null)
            {
                AddToInformation("Login required.");
                return;
            }

            string userId = user.UserId;
            dbReference.Child("users").Child(userId).Child(key).RemoveValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                    AddToInformation($"{key} deleted successfully");
                else
                    AddToInformation($"{key} deletion failed: " + task.Exception);
            });
        }

        // 유저 전체 데이터 삭제
        public void DeleteUserData()
        {
            if (!CheckFirebase()) return;

            FirebaseUser user = auth.CurrentUser;
            if (user == null)
            {
                AddToInformation("Login required.");
                return;
            }

            string userId = user.UserId;
            dbReference.Child("users").Child(userId).RemoveValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                    AddToInformation("User data deleted successfully.");
                else
                    AddToInformation("Failed to delete user data: " + task.Exception);
            });
        }
    }

    // 유저 데이터 모델 (JSON 변환용)
    [System.Serializable]
    public class UserData
    {
        public int gold;
        public int level;
        public int exp;

        public UserData(int gold, int level, int exp)
        {
            this.gold = gold;
            this.level = level;
            this.exp = exp;
        }
    }
}
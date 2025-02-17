//using UnityEngine;
//using Firebase;
//using Firebase.Auth;
//using Firebase.Database;
//using SignInSample;

//public class DataManager : SigninSampleScript
//{
//    private bool CheckFirebase()
//    {
//        if (auth == null)
//        {
//            AddToInformation("Firebase authentication is not initialized properly.");
//            return false;
//        }
//        if (dbReference == null)
//        {
//            AddToInformation("Firebase Database is not initialized properly.");
//            return false;
//        }

//        return true;
//    }

//    // ���� ������ ���� (��ü �����)
//    public void SaveUserData(UserData data)
//    {
//        if (!CheckFirebase()) return;

//        FirebaseUser user = auth.CurrentUser;
//        if (user == null)
//        {
//            AddToInformation("Login required.");
//            return;
//        }

//        string userId = user.UserId;
//        string jsonData = JsonUtility.ToJson(data);

//        dbReference.Child("users").Child(userId).SetRawJsonValueAsync(jsonData).ContinueWith(task =>
//        {
//            if (task.IsCompleted)
//                AddToInformation("User data saved successfully.");
//            else
//                AddToInformation("Failed to save user data: " + task.Exception);
//        });
//    }

//    // Ư�� ������ ���� (���� �� ������Ʈ)
//    public void SaveSingleData(string key, object value)
//    {
//        if (!CheckFirebase()) return;

//        FirebaseUser user = auth.CurrentUser;
//        if (user == null)
//        {
//            AddToInformation("Login required.");
//            return;
//        }

//        string userId = user.UserId;
//        dbReference.Child("users").Child(userId).Child(key).SetValueAsync(value).ContinueWith(task =>
//        {
//            if (task.IsCompleted)
//            {
//                AddToInformation($"{key} saved successfully: {value}");
//                LoadUserData();
//            }
//            else
//                AddToInformation($"{key} save failed: " + task.Exception);
//        });
//    }

//    // ���� ������ �ҷ�����
//    public void LoadUserData()
//    {
//        if (!CheckFirebase()) return;

//        FirebaseUser user = auth.CurrentUser;
//        if (user == null)
//        {
//            AddToInformation("Login required.");
//            return;
//        }

//        string userId = user.UserId;
//        dbReference.Child("users").Child(userId).GetValueAsync().ContinueWith(task =>
//        {
//            if (task.IsFaulted)
//            {
//                AddToInformation("Failed to load data: " + task.Exception);
//                return;
//            }

//            if (task.IsCanceled)
//            {
//                AddToInformation("Data request was canceled.");
//                return;
//            }

//            if (task.Result.Exists)
//            {
//                string json = task.Result.GetRawJsonValue();
//                UserData userData = JsonUtility.FromJson<UserData>(json);
//                AddToInformation($"Loaded data: Gold {userData.gold}, Level {userData.level}, Exp {userData.exp}");
//            }
//            else
//            {
//                AddToInformation("User data not found. Creating new data.");
//                UserData newUser = new UserData(100, 1, 0); // �⺻��
//                SaveUserData(newUser);
//            }
//        });
//    }

//    // Ư�� ������ �ҷ�����
//    public void LoadSingleData(string key)
//    {
//        if (!CheckFirebase()) return;

//        FirebaseUser user = auth.CurrentUser;
//        if (user == null)
//        {
//            AddToInformation("Login required.");
//            return;
//        }

//        string userId = user.UserId;
//        dbReference.Child("users").Child(userId).Child(key).GetValueAsync().ContinueWith(task =>
//        {
//            if (task.IsCompleted && task.Result.Exists)
//            {
//                AddToInformation($"{key} loaded successfully: {task.Result.Value}");
//            }
//            else
//            {
//                AddToInformation($"{key} data not found.");
//            }
//        });
//    }

//    public void ExampleUpdateUserData()
//    {
//        UpdateUserData("gold", 100);
//        UpdateUserData("level", 1);
//        UpdateUserData("exp", 10);
//    }

//    // Ư�� ������ ������Ʈ (+- ���� ����)
//    public void UpdateUserData(string key, int amount)
//    {
//        if (!CheckFirebase()) return;

//        FirebaseUser user = auth.CurrentUser;
//        if (user == null)
//        {
//            AddToInformation("Login required.");
//            return;
//        }

//        string userId = user.UserId;
//        DatabaseReference dataRef = dbReference.Child("users").Child(userId).Child(key);

//        dataRef.GetValueAsync().ContinueWith(task =>
//        {
//            if (task.IsCompleted && task.Result.Exists)
//            {
//                int currentValue = int.Parse(task.Result.Value.ToString());
//                int newValue = currentValue + amount;
//                dataRef.SetValueAsync(newValue);
//                AddToInformation($"{key} updated successfully: {newValue}");
//                LoadUserData();
//            }
//            else
//            {
//                AddToInformation($"{key} data not found.");
//            }
//        });
//    }

//    // Ư�� ������ ����
//    public void DeleteSingleData(string key)
//    {
//        if (!CheckFirebase()) return;

//        FirebaseUser user = auth.CurrentUser;
//        if (user == null)
//        {
//            AddToInformation("Login required.");
//            return;
//        }

//        string userId = user.UserId;
//        dbReference.Child("users").Child(userId).Child(key).RemoveValueAsync().ContinueWith(task =>
//        {
//            if (task.IsCompleted)
//                AddToInformation($"{key} deleted successfully");
//            else
//                AddToInformation($"{key} deletion failed: " + task.Exception);
//        });
//    }

//    // ���� ��ü ������ ����
//    public void DeleteUserData()
//    {
//        if (!CheckFirebase()) return;

//        FirebaseUser user = auth.CurrentUser;
//        if (user == null)
//        {
//            AddToInformation("Login required.");
//            return;
//        }

//        string userId = user.UserId;
//        dbReference.Child("users").Child(userId).RemoveValueAsync().ContinueWith(task =>
//        {
//            if (task.IsCompleted)
//                AddToInformation("User data deleted successfully.");
//            else
//                AddToInformation("Failed to delete user data: " + task.Exception);
//        });
//    }
//}

//// ���� ������ �� (JSON ��ȯ��)
//[System.Serializable]
//public class UserData
//{
//    public int gold;
//    public int level;
//    public int exp;

//    public UserData(int gold, int level, int exp)
//    {
//        this.gold = gold;
//        this.level = level;
//        this.exp = exp;
//    }
//}

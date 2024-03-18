using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminManager : MonoBehaviour
{
    public static AdminManager instance;

    private User user;
    
    private List<User> userList;
    
    private void Awake()
    {
        instance = this;
    }

    public void GenerateUser(int viewId, string nickname)
    {
        user = new User(viewId, nickname);
    }

    public void SetUser(int viewId, string nickname)
    {
        userList.Add(new User(viewId, nickname));
    }
    
    public int GetViewId(string nickname)
    {
        return userList.Find(x => x.nickname == nickname).viewId;
    }
    
    public string GetNickname(int viewId)
    {
        return userList.Find(x => x.viewId == viewId).nickname;
    }
    
    public List<User> GetUserList()
    {
        return userList;
    }
}

public class User
{
    public int viewId;
    public string nickname;
    
    public User(int viewId, string nickname)
    {
        this.viewId = viewId;
        this.nickname = nickname;
    }
}
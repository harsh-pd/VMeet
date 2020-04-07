using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserInfo{
    public int id = 0;
    public string name = "User Name";
    public string userName = "userName";
    public string emailAddress = "user@email.com";
    public string password = "password";
    public string phoneNumber = "0000000000";
    public int organizationId = 0;
    public bool isAccountLocked = false;
    public int loginAttempt = 0;
    public int createdUserId = 0;
    public int modifiedUserId = 0;
    public string modifiedDateTime  = "modifiedDateTime";
    public bool isActive = false;
    public int[] roldIds;
    public string[] roleNames;
    public int[] organizationUnitIds;
    public int UserRoletype = 0;
    public bool hasValidLicense = false;
}
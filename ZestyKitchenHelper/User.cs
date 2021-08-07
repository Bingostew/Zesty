using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace ZestyKitchenHelper
{
    [Table("UserProfile")]
    public class UserProfile
    {
        List<Action<UserProfile>> onProfileChangeEvent = new List<Action<UserProfile>>();

        [PrimaryKey, Column("Name")]
        public string Name { get; set; }
        public bool IsLocal { get; set; }
        public string Email { get; set; }
        public string IconImage { get; set; }
        public void AddOnProfileChangedListener(Action<UserProfile> action)
        {
            onProfileChangeEvent.Add(action);   
        }
        public void ChangeProfileWithListener(string name, string email, string icon)
        {
            Name = name; Email = email; IconImage = icon;
            foreach(var changeEvent in onProfileChangeEvent)
            {
                changeEvent?.Invoke(this);
            }
        }
    }
}

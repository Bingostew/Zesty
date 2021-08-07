using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace ZestyKitchenHelper
{
    [Table("MetaUserInfo")]
    public class MetaUserInfo
    {
        public bool IsLocal { get; set; }
        public MetaUserInfo() { }
        public MetaUserInfo(bool isLocal)
        {
            IsLocal = isLocal;
        }
    }
}

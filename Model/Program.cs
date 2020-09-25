using Base;
using System;
using System.Collections.Generic;

namespace Model
{
    public class Mail
    {
        public UInt64 uid = 1;
    }
    public class MailDb
    {
        private Dictionary<string, string> dic = new Dictionary<string, string>();
    }
}

using System;

namespace gaga_bot.Attributes
{
    public class User
    {
        public string userId { get; set; }
        public string money { get; set; }
        public string rep_rank { get; set; }
        public string lvl { get; set; }
        public string xp { get; set; }
    }

    public class UserBan
    {
        public string userID { get; set; }
        public string reason { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public string banID { get; set; }
        public string issued { get; set; }
    }

    public class UserWarn
    {
        public string userID { get; set; }
        public string reason { get; set; }
        public string date { get; set; }
        public string warnID { get; set; }
        public string issued { get; set; }
        public bool valid { get; set; }
    }

    public class UserMute
    {
        public string userID { get; set; }    
        public string reason { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public string muteID { get; set; }
        public string issued { get; set; }
    }
}

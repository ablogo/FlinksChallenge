using System.Collections.Generic;

namespace Core.Dtos
{
    public class LoginResponse
    {
        public bool Result { get; set; } = false;

        public string Token { get; set; }

        public Dictionary<string, string> Cookies { get; set; }

        public string ChallengeId { get; set; }

        public string UserAgent { get; set; }
    }
}

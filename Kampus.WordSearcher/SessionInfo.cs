using System;

namespace Kampus.WordSearcher
{
    public class SessionInfo
    {
        public TimeSpan Expires { get; set; }
        public DateTime Created { get; set; }
    }
}
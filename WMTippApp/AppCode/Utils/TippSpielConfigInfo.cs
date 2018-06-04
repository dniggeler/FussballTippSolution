using Tippspiel.Base;

namespace BhFS.Tippspiel.Utils
{
    public class TippspielConfigInfo : AppInfo<TippspielConfigInfo>
    {
        public string EmailFrom { get; set; }
        public string EmailProviderUser { get; set; }
        public string EmailProviderPwd { get; set; }
        public string EmailHost { get; set; }

        public int MaxJokerTips { get; set; }
        public double JokerMultiplicator { get; set; }

        public TippspielConfigInfo()
        {
            EmailFrom = @"dniggeler@bhfs.ch";
            EmailProviderUser = @"buli@bhfs.ch";
            EmailProviderPwd = @"hilbert1";
            EmailHost = @"mail.netzone.ch";
            MaxJokerTips = 0;
            JokerMultiplicator = 1.0;
        }
    }
}
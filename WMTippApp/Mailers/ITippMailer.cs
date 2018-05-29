using Mvc.Mailer;
using WMTippApp.Models;

namespace WMTippApp.Mailers
{ 
    public interface ITippMailer
    {
        MvcMailMessage EmailDailyWinner(string email, RankingInfoModel model, int spieltag);
	}
}
using Mvc.Mailer;
using FussballTippApp.Models;

namespace FussballTippApp.Mailers
{ 
    public interface ITippMailer
    {
        MvcMailMessage EmailDailyWinner(string email, DailyWinnerInfoModel model, int spieltag);
	}
}
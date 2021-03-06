using Mvc.Mailer;
using WMTippApp.Models;

namespace WMTippApp.Mailers
{ 
    public class TippMailer : MailerBase, ITippMailer 	
	{
		public TippMailer()
		{
			MasterName="_Layout";
		}

        public virtual MvcMailMessage EmailDailyWinner(string email, RankingInfoModel model, int spieltag)
		{
            ViewBag.Spieltag = spieltag;
			ViewData.Model = model;

			return Populate(x =>
			{
				x.Subject = "BuLi-Tagessieger Spieltag "+spieltag.ToString();
				x.ViewName = "EmailDailyWinner";
                x.To.Add(email);
			});
		}
 	}
}
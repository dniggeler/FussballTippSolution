using Mvc.Mailer;
using FussballTippApp.Models;

namespace FussballTippApp.Mailers
{ 
	public class TippMailer : MailerBase, ITippMailer 	
	{
		public TippMailer()
		{
			MasterName="_Layout";
		}

		public virtual MvcMailMessage EmailDailyWinner(string email, DailyWinnerInfoModel model, int spieltag)
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
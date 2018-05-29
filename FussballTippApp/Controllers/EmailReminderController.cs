using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FussballTippApp.Models;
using BhFS.Tippspiel.Utils;
using System.Net.Mail;
using System.Net;
using FussballTipp.Repository;

namespace FussballTippApp.Controllers
{
    public class EmailReminderController : Controller
    {
        readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IFussballDataRepository _matchDataRepository = new BuLiDataRepository(SportsdataConfigInfo.Current);

        public ActionResult Index()
        {
            var model = GetEmailReminders();

            return View(model);

        }

        private EmailReminderInfoModel GetEmailReminders()
        {
            using (var ctxt = new UsersContext())
            {
                var reminderModel = new EmailReminderInfoModel();

                var nextMatch = _matchDataRepository.GetNextMatch();

                reminderModel.KickoffTime = nextMatch.KickoffTime;
                reminderModel.HomeTeam = nextMatch.HomeTeam;
                reminderModel.AwayTeam = nextMatch.AwayTeam;

                using (var tippContext = new TippSpielContext())
                {
                    var tippList = (from t in tippContext.TippMatchList
                                    where t.MatchId == nextMatch.MatchId
                                    select t);

                    foreach (var user in ctxt.UserProfiles)
                    {
                        var tippObj = (from userTipp in tippList
                                       where userTipp.User == user.UserName
                                       select userTipp)
                                      .FirstOrDefault();

                        reminderModel.EmailReminderDict.Add(user.UserName, IsSendEmailReminder(user, nextMatch, tippObj));
                    }
                }

                return reminderModel;

            }
        }

        private static bool IsSendEmailReminder(UserProfile user, MatchDataModel matchObj, TippMatchModel tippObj)
        {
            if (user.HasEmailReminder == false)
            {
                return false;
            }

            if ((tippObj == null || tippObj.MyTip.HasValue == false) &&
                (matchObj.KickoffTime > DateTime.Now) &&
                (DateTime.Now > matchObj.KickoffTime.AddDays(-1)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public ActionResult Send(string user)
        {
            using (var userContext = new UsersContext())
            {
                var userObj = userContext.UserProfiles.Where(u => u.UserName == user).FirstOrDefault();

                if (userObj != null && userObj.HasEmailReminder == true)
                {
                    try
                    {
                        SendEmail(userObj.UserName, userObj.Email);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Exception while email sent: " + ex.Message);
                    }
                }
            }

            return RedirectToAction("Index");
        }

        public JsonResult CheckAndSend()
        {
            log.Debug("Begin CheckAndSend()");

            var model = GetEmailReminders();

            using (var ctxt = new UsersContext())
            {
                foreach (var kp in model.EmailReminderDict.Where(p => (p.Value == true)))
                {
                    var userObj = ctxt.UserProfiles.Where(u => u.UserName == kp.Key).FirstOrDefault();
                    if (userObj != null)
                    {
                        SendEmail(kp.Key, userObj.Email);
                    }
                }

                var jsonResponse = new
                {
                    Success = true,
                    Receivers = model.EmailReminderDict.Where(p => (p.Value == true)).Select(p => p.Key).ToArray()
                };

                log.Debug("End CheckAndSend()");

                return Json(jsonResponse, JsonRequestBehavior.AllowGet);
            }
        }

        private static bool IsSendEmailReminder(UserProfile user, MatchInfoModel matchObj, TippMatchModel tippObj)
        {
            if (user.HasEmailReminder == false)
            {
                return false;
            }

            if ((tippObj == null || tippObj.MyTip.HasValue == false) &&
                (matchObj.KickoffTime > DateTime.Now) &&
                (DateTime.Now > matchObj.KickoffTime.AddDays(-1)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SendEmail(string username, string email)
        {
            MailMessage msg = new MailMessage();

            msg.From = new MailAddress(TippspielConfigInfo.Current.EmailFrom);
            msg.To.Add(email);
            msg.Subject = "Buli-Tippspiel: Reminder";
            msg.Body = "Hallo " + username + "," + Environment.NewLine + Environment.NewLine +
                        @"Du hast für ein oder mehrere Spiele der aktuellen Buli-Runde, die in Kürze beginnen, noch Tipps offen." +
                        Environment.NewLine + Environment.NewLine +
                        "Gruss, Dieter";
            msg.Priority = MailPriority.Normal;

            SmtpClient client = new SmtpClient();

            client.Credentials = new NetworkCredential(TippspielConfigInfo.Current.EmailProviderUser, TippspielConfigInfo.Current.EmailProviderPwd);
            client.Host = TippspielConfigInfo.Current.EmailHost;
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = false;
            client.Send(msg);

            log.DebugFormat("Reminder email sent to {0}", email);

        }
    }
}

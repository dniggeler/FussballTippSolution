namespace OddsScraper.Contract.Model
{
    public class OddsInfoModel
    {
        public int Id { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public string HomeTeamSearch { get; set; }
        public string AwayTeamSearch { get; set; }
        public string HomeTeamSearch2 { get; set; }
        public string AwayTeamSearch2 { get; set; }
        public double? WinOdds { get; set; }
        public double? DrawOdds { get; set; }
        public double? LossOdds { get; set; }
    }
}

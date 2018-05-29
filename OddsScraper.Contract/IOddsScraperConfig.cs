namespace OddsScraper.Contract
{
    public interface IOddsScraperConfig
    {
        string BaseUrl { get; set; }
        string OddsLink { get; set; }
    }
}
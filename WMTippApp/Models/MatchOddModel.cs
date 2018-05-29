using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace WMTippApp.Models
{
    public class TippSpielContext : DbContext
    {
        public TippSpielContext()
            : base("DataConnection")
        {
        }

        public DbSet<MatchOddsModel> MatchOddsList { get; set; }
        public DbSet<TippMatchModel> TippMatchList { get; set; }
    }

    [Table("MatchOdds")]
    public class MatchOddsModel
    {
        [Key]
        public int MatchId { get; set; }
        public string User { get; set; }
        public double HomeOdds { get; set; }
        public double DrawOdds { get; set; }
        public double AwayOdds { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    [Table("TippMatch")]
    public class TippMatchModel
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int MatchId { get; set; }
        public int GroupId { get; set; }
        public string User { get; set; }
        public double? MyOdds { get; set; }
        public int? MyTip { get; set; }
        public bool IsJoker { get; set; }
        public double? MyAmount { get; set; }
        public DateTime LastUpdated { get; set; }
    }

}
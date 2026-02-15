using System;

namespace NogometTerminApp.Models
{
    public class TermStatisticsViewModel
    {
        public int TermId { get; set; }
        public DateTime TermDateTime { get; set; }
        public string Location { get; set; }
        public int MaxPlayers { get; set; }
        public int RegisteredCount { get; set; }
        public bool IsPast { get; set; }
        public string Result { get; set; }
        public bool IsInEditMode { get; set; }
        public bool IsPostponed { get; set; }
    }
}

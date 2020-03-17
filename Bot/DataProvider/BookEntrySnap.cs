using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.DataProvider
{
    public class BookEntrySnap
    {
        public DateTime StartedTime { get; set; } = DateTime.Now;
        public DateTime LastRefresh { get; set; }

        public Dictionary<DateTime, BookEntry> BookData { get; set; } = new Dictionary<DateTime, BookEntry>();
        public BookEntrySnap()
        {

        }
        public void AddBookCapture(BookEntry DataIn)
        {
            BookData.Add(DateTime.Now, DataIn);
            LastRefresh = DateTime.Now;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab3_WebApp.Models
{
    public class Review
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
        public string DateTime { get; set; }

        public Review(int rating, string comment, string datetime)
        {
            Rating = rating;
            Comment = comment;
            DateTime = datetime;
        }
        public Review()
        { 
        }
    }
}

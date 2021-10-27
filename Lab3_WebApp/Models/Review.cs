using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;

namespace Lab3_WebApp.Models
{
    [DynamoDBTable("Review")]
    public class Review
    {
        [DynamoDBHashKey]
        public string Username { get; set; }

        [DynamoDBRangeKey]
        public int Rating { get; set; }

        [DynamoDBProperty]
        public string DateTime { get; set; }

        [DynamoDBProperty]
        public string Title { get; set; }

        [DynamoDBProperty]
        public string Comment { get; set; }

        public Review() { }

        public Review(string username, int rating, string dateTime, string title, string comment)
        {
            Username = username;
            Rating = rating;
            DateTime = dateTime;
            Title = title;
            Comment = comment;
        }
    }
}

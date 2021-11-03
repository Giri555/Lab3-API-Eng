using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;

namespace Lab3_WebApp.Models
{
    [DynamoDBTable("Movie")]        // initial model -- ** needs redesign **
    public class Movie
    {
        [DynamoDBHashKey]
        public string Id { get; set; }

        [DynamoDBProperty]
        public string Title { get; set; }

        [DynamoDBRangeKey]
        public string Username { get; set; }

        [DynamoDBProperty]
        public string Cast { get; set; }

        [DynamoDBProperty]
        public string ReleaseDate { get; set; }

        [DynamoDBProperty]
        public int Budget { get; set; }

        public Movie() { }

        public Movie(string id, string title, string username, string cast, string releaseDate, int budget)
        {
            Id = id;
            Title = title;
            Username = username;
            Cast = cast;
            ReleaseDate = releaseDate;
            Budget = budget;
        }
    }
}


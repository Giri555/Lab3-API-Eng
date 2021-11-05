using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;

namespace Lab3_WebApp.Models
{
    [DynamoDBTable("Movie")]
    public class Movie
    {
        [DynamoDBHashKey]
        public string Id { get; set; }

        [DynamoDBRangeKey]
        public string Username { get; set; } // owner

        [DynamoDBProperty]
        public int Rating { get; set; } // average rating

        [DynamoDBProperty]
        public Dictionary<string, Review> Review { get; set; } // key = username(author of review), value = Review(rating, comment, datetime)

        [DynamoDBProperty]
        public string Title { get; set; }

        [DynamoDBProperty]
        public string Cast { get; set; }

        [DynamoDBProperty]
        public string ReleaseDate { get; set; }

        [DynamoDBProperty]
        public int Budget { get; set; }

        [DynamoDBProperty]
        public string Video { get; set; }

        public Movie() { }

        public Movie(string id, string title, string username, string cast, string releaseDate, int budget, int rating)
        {
            Id = id;
            Title = title;
            Username = username;
            Cast = cast;
            ReleaseDate = releaseDate;
            Budget = budget;
            Rating = rating;
            Review = new Dictionary<string, Review>() { };
        }

        public Movie(string id, string title, string username, string cast, string releaseDate, int budget, int rating, Dictionary<string, Review> review) // movie + review
        {
            Id = id;
            Title = title;
            Username = username;
            Cast = cast;
            ReleaseDate = releaseDate;
            Budget = budget;
            Rating = rating;
            Review = review;
        }
    }
}


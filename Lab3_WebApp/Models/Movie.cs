﻿using System;
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
        public string Title { get; set; }

        [DynamoDBProperty]
        public string Username { get; set; }

        [DynamoDBProperty]
        public string Cast { get; set; }

        [DynamoDBProperty]
        public string ReleaseDate { get; set; }

        [DynamoDBProperty]
        public int Budget { get; set; }

        public Movie() { }

        public Movie(string title, string username, string cast, string releaseDate, int budget)
        {
            Title = title;
            Username = username;
            Cast = cast;
            ReleaseDate = releaseDate;
            Budget = budget;
        }
    }
}

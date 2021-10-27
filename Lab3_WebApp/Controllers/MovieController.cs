using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lab3_WebApp.Models;

namespace Lab3_WebApp.Controllers
{
    public class MovieController : Controller
    {
        IAmazonS3 S3Client { get; set; }
        IAmazonDynamoDB DynamoDBClient { get; set; }
        IDynamoDBContext DynamoDBContext { get; set; }

        public MovieController(IAmazonS3 s3Client, IAmazonDynamoDB dynamoDB, IDynamoDBContext dynamoDBContext)
        {
            S3Client = s3Client;
            DynamoDBClient = dynamoDB;
            DynamoDBContext = dynamoDBContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var scanConditions = new List<ScanCondition>() { new ScanCondition("Title", ScanOperator.IsNotNull) };
            var searchResults = DynamoDBContext.ScanAsync<Movie>(scanConditions, null);
            List<Movie> movies = await searchResults.GetNextSetAsync();
            return View(movies);
        }

        public async Task<IActionResult> Delete(string title)
        {
            await DynamoDBContext.DeleteAsync<Movie>(title);
            //return View("Index");
            return RedirectToAction("Index");
        }
    }
}



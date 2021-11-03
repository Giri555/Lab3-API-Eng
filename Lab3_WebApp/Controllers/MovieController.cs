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

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(Movie newMovie)
        {
            await DynamoDBContext.SaveAsync<Movie>(newMovie);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Update(Movie movie)
        {
            Movie updatingMovie = await DynamoDBContext.LoadAsync<Movie>(movie.Title);
            updatingMovie.Title = movie.Title;
            await DynamoDBContext.SaveAsync(updatingMovie);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("Edit/{movieTitle}")]
        public async Task<ActionResult> Edit(String title)
        {
            var movie = await DynamoDBContext.LoadAsync<Movie>(title);

            ViewBag.Title = title;

            return View();
        }

      


        [HttpDelete]
        public async Task<IActionResult> Delete(string title)
        {
            await DynamoDBContext.DeleteAsync<Movie>(title);
            //return View("Index");
            return RedirectToAction("Index");
        }
    }
}



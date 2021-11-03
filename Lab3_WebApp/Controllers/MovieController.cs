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
using Microsoft.AspNetCore.Identity;

namespace Lab3_WebApp.Controllers
{
    public class MovieController : Controller
    {
        IAmazonS3 S3Client { get; set; }
        IAmazonDynamoDB DynamoDBClient { get; set; }
        IDynamoDBContext DynamoDBContext { get; set; }

        private UserManager<AppUser> userManager;
        public MovieController(IAmazonS3 s3Client, IAmazonDynamoDB dynamoDB, IDynamoDBContext dynamoDBContext, UserManager<AppUser> userManager)
        {
            S3Client = s3Client;
            DynamoDBClient = dynamoDB;
            DynamoDBContext = dynamoDBContext;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userName = userManager.GetUserId(HttpContext.User);
            AppUser user = userManager.FindByIdAsync(userName).Result;

            var scanConditions = new List<ScanCondition>() { new ScanCondition("Id", ScanOperator.IsNotNull), new ScanCondition("Username", ScanOperator.Equal, user.Email) };
            var searchResults = DynamoDBContext.ScanAsync<Movie>(scanConditions, null);
            List<Movie> movies = await searchResults.GetNextSetAsync();
           
            return View(movies);
        }

        [HttpGet]
        public async Task<IActionResult> Other()
        {
            var userName = userManager.GetUserId(HttpContext.User);
            AppUser user = userManager.FindByIdAsync(userName).Result;

            var scanConditions = new List<ScanCondition>() { new ScanCondition("Id", ScanOperator.IsNotNull), new ScanCondition("Username", ScanOperator.NotEqual, user.Email) };
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
            string userName = userManager.GetUserId(HttpContext.User);
            System.Diagnostics.Debug.WriteLine("User log:" + userName);
            Movie updatingMovie = await DynamoDBContext.LoadAsync<Movie>(movie.Title);
            updatingMovie.Title = movie.Title;
            await DynamoDBContext.SaveAsync(updatingMovie);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("Edit/{id}")]
        public async Task<ActionResult> Edit(String id)
        {
            var userName = userManager.GetUserId(HttpContext.User);
            AppUser user = userManager.FindByIdAsync(userName).Result;
            var movie = await DynamoDBContext.LoadAsync<Movie>(id, user.Email);
            ViewBag.Id = id;

            return View("Edit", movie);
        }

        [HttpPost]
        [Route("Edit/{id}")]
        public async Task<ActionResult> Edit()
        {
          
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



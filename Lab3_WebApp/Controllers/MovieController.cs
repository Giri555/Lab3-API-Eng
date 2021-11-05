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
using Microsoft.Extensions.Configuration;
using Amazon;
using System.IO;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Lab3_WebApp.Models.ViewModels;
using System.Net.Http.Headers;
using System.Diagnostics;

namespace Lab3_WebApp.Controllers
{
    public class MovieController : Controller
    {
        IAmazonS3 S3Client { get; set; }
        IAmazonDynamoDB DynamoDBClient { get; set; }
        IDynamoDBContext DynamoDBContext { get; set; }
        
        private static readonly string bucketname = "lab03movies";

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
            ViewBag.CurrentUser = user.Email;
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
            var userName = userManager.GetUserId(HttpContext.User);
            AppUser user = userManager.FindByIdAsync(userName).Result;
            newMovie.Id = Guid.NewGuid().ToString();
            newMovie.Username = user.Email;
            await DynamoDBContext.SaveAsync<Movie>(newMovie);
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
        public async Task<ActionResult> Edit(Movie movie)
        {
            var userName = userManager.GetUserId(HttpContext.User);
            AppUser user = userManager.FindByIdAsync(userName).Result;
            var updatingMovie = await DynamoDBContext.LoadAsync<Movie>(movie.Id, user.Email);
            updatingMovie.Title = movie.Title;
            updatingMovie.Cast = movie.Cast;
            updatingMovie.Budget = movie.Budget;
            updatingMovie.ReleaseDate = movie.ReleaseDate;
            await DynamoDBContext.SaveAsync(updatingMovie);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("Delete/{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var userName = userManager.GetUserId(HttpContext.User);
            AppUser user = userManager.FindByIdAsync(userName).Result;
            await DynamoDBContext.DeleteAsync<Movie>(id, user.Email);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id, string username)
        {
            var userName = userManager.GetUserId(HttpContext.User);
            AppUser user = userManager.FindByIdAsync(userName).Result;
            ViewBag.CurrentUser = user.Email;
            
            var movie = await DynamoDBContext.LoadAsync<Movie>(id, username);
            return View(movie);
        }

        [HttpPost]
        public async Task<ActionResult> FilterByRating(int rating)
        {
            var userName = userManager.GetUserId(HttpContext.User);
            AppUser user = userManager.FindByIdAsync(userName).Result;

            var scanConditions = new List<ScanCondition>() { new ScanCondition("Id", ScanOperator.IsNotNull), new ScanCondition("Username", ScanOperator.NotEqual, user.Email), new ScanCondition("Rating", ScanOperator.GreaterThan, rating) };
            var searchResults = DynamoDBContext.ScanAsync<Movie>(scanConditions, null);
            List<Movie> movies = await searchResults.GetNextSetAsync();

            return View("Other", movies);
        }


        [HttpGet]
        [Route("Download/{id}/{username}")]
        public async Task<ActionResult> Download(string id, string username)
        {
            Movie movie = await DynamoDBContext.LoadAsync<Movie>(id, username);
            AWSHelper.DownloadFile(S3Client, bucketname, movie.Video);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("Upload/{id}")]
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [Route("Upload/{id}")]
        public async Task<IActionResult> Upload(IFormFile videoUploaded, string id)
        {
            string key = id +"_" + videoUploaded.FileName ;
            try
            {
                await AWSHelper.UploadFileAsync(videoUploaded, key, bucketname, S3Client);
                
                var userName = userManager.GetUserId(HttpContext.User);
                AppUser user = userManager.FindByIdAsync(userName).Result;
                var movie = await DynamoDBContext.LoadAsync<Movie>(id, user.Email);
                movie.Video = key;
                await DynamoDBContext.SaveAsync(movie);
            }

            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    ViewBag.Message = "Check the provided AWS Credentials.";
                }
                else
                {
                    ViewBag.Message = "Error occurred: " + amazonS3Exception.Message;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("CreateReview/{id}/{username}")]
        public async Task<IActionResult> CreateReview()
        {
           return View("Comment");
        }

        [HttpPost]
        [Route("CreateReview/{id}/{username}")]
        public async Task<IActionResult> CreateReview(Review review, string id, string username)
        { 
            Movie editingMovie = await DynamoDBContext.LoadAsync<Movie>(id, username);
            var userName = userManager.GetUserId(HttpContext.User);
            AppUser user = userManager.FindByIdAsync(userName).Result;
            
            review.DateTime = DateTime.Now.ToString();
            if(editingMovie.Review == null)
            {
                editingMovie.Review = new Dictionary<string, Review> { };
            }
            editingMovie.Review.Add(user.Email, review);
            await DynamoDBContext.SaveAsync(editingMovie);
            return RedirectToAction("Other");
        }

        [HttpGet]
        [Route("EditReview/{id}/{username}")]
        public async Task<IActionResult> EditReview(string id, string username)
        {
            var userName = userManager.GetUserId(HttpContext.User);
            AppUser user = userManager.FindByIdAsync(userName).Result;
            Movie editingMovie = await DynamoDBContext.LoadAsync<Movie>(id, username);
           
            Review review = editingMovie.Review[user.Email];
            return View("EditReview", review);
        }

        [HttpPost]
        [Route("EditReview/{id}/{username}")]
        public async Task<IActionResult> EditReview(Review review, string id, string username)
        {
            var userName = userManager.GetUserId(HttpContext.User);
            AppUser user = userManager.FindByIdAsync(userName).Result;
            Movie editingMovie = await DynamoDBContext.LoadAsync<Movie>(id, username);
            Dictionary<string, Review> updatingDictionary = new Dictionary<string, Review>() { };
            //update Review
            foreach (KeyValuePair<string, Review> entry in editingMovie.Review)
            {
                if(entry.Key == user.Email)
                {
                    updatingDictionary.Add(entry.Key, new Review(review.Rating, review.Comment, DateTime.Now.ToString()));
                }
                else
                {
                    updatingDictionary.Add(entry.Key, entry.Value);
                }
            }
            editingMovie.Review = updatingDictionary;
            await DynamoDBContext.SaveAsync(editingMovie);
            return RedirectToAction("Other");
        }
    }
}




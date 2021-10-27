using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;

namespace Lab3_WebApp.Models
{
    public static class SeedData
    {
        const string TABLE_NAME_MOVIE = "Movie";
        const string TABLE_NAME_REVIEW = "Review";
        static IAmazonDynamoDB dynamoDB;
        static IDynamoDBContext dynamoDBContext;

        public static void EnsurePopulated(IApplicationBuilder app)
        {
            dynamoDB = app.ApplicationServices.GetRequiredService<IAmazonDynamoDB>();
            dynamoDBContext = app.ApplicationServices.GetRequiredService<IDynamoDBContext>();

            List<AttributeDefinition> attributeDefinition_movie = new List<AttributeDefinition> 
            {
                new AttributeDefinition
                {
                    AttributeName = "Title",
                    AttributeType = "S"
                }
            };

            List<KeySchemaElement> keySchemaElements_movie = new List<KeySchemaElement>
            {
                new KeySchemaElement
                {
                    AttributeName = "Title",
                    KeyType = "HASH"
                }
            };

            ProvisionedThroughput provisionedThroughput = new ProvisionedThroughput
            {
                ReadCapacityUnits = 2,
                WriteCapacityUnits = 1
            };

            bool isCreated = CreateTable_async(TABLE_NAME_MOVIE, attributeDefinition_movie, keySchemaElements_movie, provisionedThroughput).Result;
            //CreateTable_async(TABLE_NAME_REVIEW, attributeDefinition_review, keySchemaElements_review, provisionedThroughput);

            if(isCreated)
            {
                if (CheckTableStatus(TABLE_NAME_MOVIE).Result)
                {
                    List<Movie> movies = new List<Movie>
                    {
                    new Movie("movie 1", "admin@mail.ca", "cast cast cast", "date1", 50),
                    new Movie("movie 201", "ha@mail.ca", "cast cast cast", "date2", 60),
                    new Movie("movie 202", "ha@mail.ca", "cast cast cast", "date2", 60),
                    new Movie("movie 203", "ha@mail.ca", "cast cast cast", "date2", 60),
                    new Movie("movie 204", "ha@mail.ca", "cast cast cast", "date2", 60),
                    new Movie("movie 205", "ha@mail.ca", "cast cast cast", "date2", 60),
                    new Movie("movie 3", "admin@mail.ca", "cast cast", "date3", 70),
                    new Movie("movie 4", "admin@mail.ca", "cast cast", "date3", 70),
                    new Movie("movie 5", "admin@mail.ca", "cast cast", "date3", 70),
                    new Movie("movie 6", "admin@mail.ca", "cast cast", "date3", 70),
                    new Movie("movie 7", "admin@mail.ca", "cast cast", "date3", 70)
                    };

                    BatchWrite<Movie> movieBatch = dynamoDBContext.CreateBatchWrite<Movie>();
                    movieBatch.AddPutItems(movies);
                    movieBatch.ExecuteAsync();
                }
            }
        }

        public static async Task<bool> CreateTable_async(string tableName, List<AttributeDefinition> tableAttributes, List<KeySchemaElement> tableKeySchema, ProvisionedThroughput provisionedThroughput)
        {
            ListTablesResponse response = await dynamoDB.ListTablesAsync();

            if (!response.TableNames.Contains(tableName))
            {
                CreateTableRequest request = new CreateTableRequest
                {
                    TableName = tableName,
                    AttributeDefinitions = tableAttributes,
                    KeySchema = tableKeySchema,
                    BillingMode = BillingMode.PROVISIONED,
                    ProvisionedThroughput = provisionedThroughput
                };

                await dynamoDB.CreateTableAsync(request);
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckTableStatus(string tableName)
        {
            var status = "";
            do
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5)); // Wait 5 seconds before checking (again).
                try
                {
                    var response2 = await dynamoDB.DescribeTableAsync(new DescribeTableRequest
                    {
                        TableName = tableName
                    });
                    status = response2.Table.TableStatus;
                }
                catch (ResourceNotFoundException)
                {
                }
            } while (status != TableStatus.ACTIVE);
            return true;
        }
    }
}

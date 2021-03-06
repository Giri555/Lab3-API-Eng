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
        static IAmazonDynamoDB dynamoDB;
        static IDynamoDBContext dynamoDBContext;

        public static async void EnsurePopulated(IApplicationBuilder app)
        {
            dynamoDB = app.ApplicationServices.GetRequiredService<IAmazonDynamoDB>();
            dynamoDBContext = app.ApplicationServices.GetRequiredService<IDynamoDBContext>();

            // if the movie table does not exist, create it
            if(!CheckTableExists(TABLE_NAME_MOVIE).Result)
            {
                List<AttributeDefinition> attributeDefinition_movie = new List<AttributeDefinition>
                {
                   new AttributeDefinition { AttributeName = "Id", AttributeType = "S" },
                   new AttributeDefinition { AttributeName = "Username", AttributeType = "S" },
                   new AttributeDefinition { AttributeName = "Rating", AttributeType = "N" }
                };

                List<KeySchemaElement> keySchemaElements_movie = new List<KeySchemaElement>
                {
                    new KeySchemaElement { AttributeName = "Id", KeyType = "HASH" },
                    new KeySchemaElement { AttributeName = "Username", KeyType = "RANGE" }
                };

                List<KeySchemaElement> indexKeySchema_movie = new List<KeySchemaElement>
                {
                    new KeySchemaElement { AttributeName = "Id", KeyType = "HASH" },
                    new KeySchemaElement { AttributeName = "Rating", KeyType = "RANGE" }
                };

                Projection projection = new Projection() { ProjectionType = "INCLUDE" };

                List<string> nonKeyAttributes = new List<string> { "Title", "Cast", "ReleaseDate", "Budget", "Review" };
                projection.NonKeyAttributes = nonKeyAttributes;

                List<LocalSecondaryIndex> localSecondaryIndex = new List<LocalSecondaryIndex>
                {
                    new LocalSecondaryIndex { IndexName = "Rating", KeySchema = indexKeySchema_movie, Projection = projection}
                };

                ProvisionedThroughput provisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 2,
                    WriteCapacityUnits = 1
                };

                CreateTable_async(TABLE_NAME_MOVIE, attributeDefinition_movie, keySchemaElements_movie, localSecondaryIndex, provisionedThroughput);

                if (CheckTableStatus(TABLE_NAME_MOVIE).Result) // if table status is active, populate it
                {
                    List<Movie> movies = new List<Movie>
                    {
                        new Movie("100","movie 1", "admin@mail.ca", "cast cast cast", "2021-10-09", 50, 5),
                        new Movie("101","movie 2", "admin@mail.ca", "cast cast cast", "2021-10-09", 60, 9),
                        new Movie("102","movie 3", "admin@mail.ca", "cast cast cast", "2021-10-09", 60, 9),
                        new Movie("103","movie 4", "admin@mail.ca", "cast cast cast", "2021-10-10", 60, 9),
                        new Movie("104","movie 5", "admin@mail.ca", "cast cast cast", "2021-10-10", 60, 9),
                        new Movie("105","movie 6", "ha@mail.ca", "cast cast cast", "2021-10-10", 60, 3),
                        new Movie("106","movie 7", "ha@mail.ca", "cast cast", "2021-10-09", 70, 7),
                        new Movie("107","movie 8", "ha@mail.ca", "cast cast", "2021-10-09", 70, 7),
                        new Movie("108","movie 9", "ha@mail.ca", "cast cast", "2021-10-09", 70, 7),
                        new Movie("109","movie 109", "admin@mail.ca", "cast cast", "2021-10-09", 70, 7, new Dictionary<string, Review>() // one movie, with 2 reviews
                        {
                            { "admin@mail.ca", new Review { Rating=9, Comment="This is a nice movie", DateTime="2021-10-09T23:43:21.556Z" } },
                            { "ha@mail.ca", new Review { Rating=7, Comment="This movie was boring", DateTime="2021-10-10T13:15:39.206Z" } }
                        })
                    };

                    BatchWrite<Movie> movieBatch = dynamoDBContext.CreateBatchWrite<Movie>();
                    movieBatch.AddPutItems(movies);
                    await movieBatch.ExecuteAsync();
                }
            }
        }

        public static async void CreateTable_async(string tableName, List<AttributeDefinition> tableAttributes, List<KeySchemaElement> tableKeySchema, List<LocalSecondaryIndex> localSecondaryIndexes, ProvisionedThroughput provisionedThroughput)
        {
            CreateTableRequest request = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = tableAttributes,
                KeySchema = tableKeySchema,
                LocalSecondaryIndexes = localSecondaryIndexes,
                BillingMode = BillingMode.PROVISIONED,
                ProvisionedThroughput = provisionedThroughput
            };
            await dynamoDB.CreateTableAsync(request);
        }

        public static async Task<bool> CheckTableExists(string tableName)
        {
            ListTablesResponse response = await dynamoDB.ListTablesAsync();
            if (response.TableNames.Contains(tableName))
                return true;
            else
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

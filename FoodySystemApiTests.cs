using System;
using System.Net;
using System.Text.Json;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Text.Json.Serialization;
using FoodySystemExamPrepII.Models;
using System.Web.Helpers;

namespace FoodySystemExamPrepII
{
    [TestFixture]
    public class FoodySystemApiTest
    {
        private RestClient client;
        private static string lastCreatedFoodId;
        private const string baseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";
        private const string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI2ZDIyMTk4My03MGNlLTRjMWItOTQ2MC00NjlhYzMyNGZiMDYiLCJpYXQiOiIwOC8xNS8yMDI1IDExOjQzOjIxIiwiVXNlcklkIjoiNDliZTQyMTgtNDUxYi00Y2Y4LTlhYWEtMDhkZGQ4ZTVkYWIyIiwiRW1haWwiOiJ0ZXN0VXNlcjEwMDJAZXhhbXBsZS5jb20iLCJVc2VyTmFtZSI6InRlc3RVc2VyMTAwMiIsImV4cCI6MTc1NTI3OTgwMSwiaXNzIjoiRm9vZHlfQXBwX1NvZnRVbmkiLCJhdWQiOiJGb29keV9XZWJBUElfU29mdFVuaSJ9.ALordB9UOEkkHmPOdlByiY0LuBKX71CtA3hsKTqcfYg";
        
        [OneTimeSetUp]
        public void Setup()
        {
            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            this.client = new RestClient(options);
        }

        [Order(1)]
        [Test]
        public void CreateNewFood_WithRequiredFields_ShouldReturnSuccess()
        {
            var foodRequest = new FoodDTO
            {
                Name = "Food name test",
                Description = "This is some test description",
                Url = ""
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(foodRequest);

            var response = client.Execute(request);
            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Expect status code to be 201");
            Assert.That(createResponse.FoodId, Is.Not.Null.And.Not.Empty, "reponse should include FoodId");
            lastCreatedFoodId = createResponse.FoodId;

        }

        [Order(2)]
        [Test]
        public void EditTitleOfFood_WithExcistingId_ShouldReturnSuccess()
        {
            var editRquest = new[]
            {
                       new { path = "/name", op = "replace", value = "updated food title" }
            };
            var request = new RestRequest($"/api/Food/Edit/{lastCreatedFoodId}", Method.Patch);
            
            request.AddJsonBody(editRquest);

            var response = client.Execute(request);
            var editedResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expect status code to be 200");
            Assert.That(editedResponse.Msg, Is.EqualTo("Successfully edited"), "Success message should be returned");

        }

        [Order(3)]
        [Test]
        public void GetAllFood_ShouldReturnOK_And_NotEmptyArray()
        {
            var request = new RestRequest("/api/Food/All", Method.Get);

            var response = client.Execute(request);
            var responseList = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expect status code to be 200");
            Assert.That(responseList, Is.Not.Null.And.Not.Empty, "should return non empty list");
        }

        [Order(4)]
        [Test]
        public void DeleteFood_WithExecistingId_ShouldReturnOK()
        {
            var request = new RestRequest($"/api/Food/Delete/{lastCreatedFoodId}", Method.Delete);
            var response = client.Execute(request);
            var responseInfo = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expect status code to be 200");
            Assert.That(responseInfo.Msg, Is.EqualTo("Deleted successfully!"), "Success message should be returned");
        }

        [Order(5)]
        [Test]
        public void CreateFood_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var foodRequest = new FoodDTO
            {
                Name = "",
                Description = "",
                Url = ""
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(foodRequest);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expect status code to be 400");
        }

        [Order(6)]
        [Test]
        public void EditTitleOfFood_WithNonExcistingId_ShouldReturnNotFound()
        {
            var editRquest = new[]
{
                       new { path = "/name", op = "replace", value = "updated food title" }
            };
            var request = new RestRequest($"/api/Food/Edit/none", Method.Patch);
            request.AddJsonBody(editRquest);

            var response = client.Execute(request);
            var editedResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "Expect status code to be 404");
            Assert.That(editedResponse.Msg, Is.EqualTo("No food revues..."), "No food revues... message should be returned");

        }

        [Order(7)]
        [Test]
        public void DeleteFood_WithNonExicistingId_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/api/Food/Delete/none", Method.Delete);
            var response = client.Execute(request);
            var responseInfo = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expect status code to be 200");
            Assert.That(responseInfo.Msg, Is.EqualTo("Unable to delete this food revue!"), "Unable to delete this food revue! message should be returned");
        }


        [OneTimeTearDown]  
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}
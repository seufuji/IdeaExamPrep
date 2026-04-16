using IdeaExamPrep.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace IdeaExamPrep
{
    [TestFixture]
    public class Tests
    {
        //step 1
        private RestClient client;

        private const string BaseUrl = "http://144.91.123.158:82/";

        private const string Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI2MjhiNGQwZS04MjMwLTQyOGItYjZjZC04ZTg4OGUwNTEzMTIiLCJpYXQiOiIwNC8xNi8yMDI2IDE3OjI5OjA3IiwiVXNlcklkIjoiN2I3NDVmOTAtYjM5YS00YTEwLTUzOWMtMDhkZTc2YTJkM2VjIiwiRW1haWwiOiJmdWtpMTIzQG1haWwuY29tIiwiVXNlck5hbWUiOiJmdWtpMTIzNCIsImV4cCI6MTc3NjM4MjE0NywiaXNzIjoiSWRlYUNlbnRlcl9BcHBfU29mdFVuaSIsImF1ZCI6IklkZWFDZW50ZXJfV2ViQVBJX1NvZnRVbmkifQ.pzOfrj9kWJzg5xGaYYawiH8-XFzDwpVKFL3J-9Ia-YA";

        private const string Email = "fuki123@mail.com";

        private const string Password = "123456";

        // za test 2 
        private static string lastCreatedIdeaId;


        [OneTimeSetUp]

        public void Setup()
        {

            string jwtToken;

            if (!string.IsNullOrWhiteSpace(Token))
            {
                jwtToken = Token;
            }
            else
            {
                jwtToken = GetJwtToken(Email, Password);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });
            var response = tempClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token is null or empty.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }

        [Order(1)]
        [Test]

        public void CreateNewIdea_ShouldReturn200OK()
        {
            var request = new RestRequest("api/Idea/Create", Method.Post);
            var idea = new
            {
                title = "Test Idea",
                description = "This is a test idea.",
                url = ""
            };

            request.AddJsonBody(idea);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"));  
        }

        [Order(2)]
        [Test]

        public void GetAllIdeas_ShouldReturn200OK()
        {
            var request = new RestRequest("api/Idea/All", Method.Get);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var createResponse = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);
            Assert.That(createResponse, Is.Not.Empty);

            lastCreatedIdeaId = createResponse.Last().Id;

        }

        [Order(3)]
        [Test]

        public void EditLastIdea_ShouldReturn200OK() 
        {
            var editRequestData = new IdeaDTO
            {
                Title = "Edited Idea",
                Description = "This is a edited idea description.",
                Url = ""
            };


            var request = new RestRequest("api/Idea/Edit", Method.Put);

            request.AddQueryParameter("ideaId", lastCreatedIdeaId);
            request.AddJsonBody(editRequestData);

            var response = this.client.Execute(request);

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(editResponse.Msg, Is.EqualTo("Edited successfully"));
        }

        [Order(4)]
        [Test]

        public void DeleteLastIdea_ShouldReturn200OK()
        {
            var request = new RestRequest("api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", lastCreatedIdeaId);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            
            Assert.That(response.Content, Is.EqualTo("\"The idea is deleted!\""));
        }

        [Order(5)]
        [Test]

        public void CreateIdeaWithEmptyTitle_ShouldReturn400BadRequest()
        {
            var request = new RestRequest("api/Idea/Create", Method.Post);
            var idea = new
            {
                title = "",
                description = "This is a test idea with empty title.",
                url = ""
            };
            request.AddJsonBody(idea);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]

        public void TryToEditNonExistingIdea_ShouldReturnBadRequest()
        {
            var editRequestData = new IdeaDTO
            {
                Title = "Edited Non-Existing Idea",
                Description = "This is a test for editing a non-existing idea.",
                Url = ""
            };
            var nonExistingIdeaId = "99999999";
            var request = new RestRequest("api/Idea/Edit", Method.Put);
            request.AddQueryParameter("ideaId", nonExistingIdeaId);
            request.AddJsonBody(editRequestData);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(7)]
        [Test]

        public void TryToDeleteNonExistingIdea_ShouldReturnBadRequest()
        {
            var nonExistingIdeaId = "99999999";
            var request = new RestRequest("api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", nonExistingIdeaId);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            
            Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));

        }

        // step 2
        [OneTimeTearDown]
        public void TearDown()
        {
            this.client.Dispose();
        }
    }
}
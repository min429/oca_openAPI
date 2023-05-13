using System.Net.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// add namespaces for azure functions openapi extension
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;

using BackendApi.Proxies;
using System.Linq;
using System.Collections.Generic;


namespace BackendApi
{
    public static class ChatHttpTrigger
    {
        [FunctionName("ChatHttpTrigger")]

        // add azure functions openapi extension decorators for operation, security, request body and response body
        [OpenApiOperation(operationId: "Chat", tags: new[] { "chat" }, Summary = "Chat with the bot", Description = "This is the function app as a chatbot.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ChatRequest), Required = true, Description = "This is the chat request payload")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ChatResponse), Summary = "This is the chat response payload", Description = "This is the chat response payload")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "This is the bad request response payload", Description = "This is the bad request response payload")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Summary = "This is the internal server error response payload", Description = "This is the internal server error response payload")]



        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "chat")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var chatRequest = JsonConvert.DeserializeObject<ChatRequest>(requestBody);


            // get base URL from the environment variable of ChatApiUrl
            var baseUrl = Environment.GetEnvironmentVariable("ChatApiUrl");
            var apiKey = Environment.GetEnvironmentVariable("ChatApiKey");
            var deploymentId = Environment.GetEnvironmentVariable("DeploymentId");
            var apiVersion = Environment.GetEnvironmentVariable("ApiVersion");
            var body = new Body3()
            {
                Messages = new List<Messages>()
                {
                    new Messages()
                    {
                        Role = MessagesRole.User,
                        Content = chatRequest.Prompt
                    }
                },
                Temperature = 0.7,
                N = 1,
                Max_tokens = 800
            };

            var http = new HttpClient();
            http.DefaultRequestHeaders.Add("api-key", apiKey);
            var chat = new ChatCompletionsClient(http);
            chat.BaseUrl = baseUrl;
            var response = await chat.CreateAsync(deploymentId, apiVersion, body);

            var message = response.Choices.ToList().First().Message.Content;

            return new OkObjectResult(new ChatResponse() { Message = message });


            // string name = req.Query["name"];

            // string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            // dynamic data = JsonConvert.DeserializeObject(requestBody);
            // name = name ?? data?.name;

            // string responseMessage = string.IsNullOrEmpty(name)
            //     ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //     : $"Hello, {name}. This HTTP triggered function executed successfully.";

            // return new OkObjectResult(responseMessage);
        }
    }

    public class ChatRequest
    {
        public string Prompt { get; set; }
    }

    public class ChatResponse
    {
        public string Message { get; set; }
    }
}

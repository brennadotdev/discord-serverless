using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Discord.Protocol.Data;
using Discord.Protocol.Request;
using Discord.Protocol.Response;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Discord.Serverless
{
    public class Function
    {
        private readonly byte[] _byteKey;
        protected readonly ApplicationCommandHandlerList ApplicationCommandHandlerList = new();

        protected Function()
        {
            var applicationPublicKey = Environment.GetEnvironmentVariable("ApplicationPublicKey");
            _byteKey = Protocol.Utils.ConvertStringToByteKey(applicationPublicKey);
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> BotCommandHandlerAsync(APIGatewayHttpApiV2ProxyRequest request,
            ILambdaContext context)
        {
            if (!Protocol.Utils.IsSignatureValid(request.Headers, request.Body, _byteKey))
            {
                context.Logger.LogLine("Request signature not valid!");
                return GenerateErrorResponse(HttpStatusCode.Unauthorized);
            }

            context.Logger.LogLine($"Request: {request.Body}");

            InteractionRequest interactionRequest = JsonSerializer.Deserialize<InteractionRequest>(request.Body);

            return interactionRequest.Type switch
            {
                InteractionType.PingPong => HandlePing(context),
                InteractionType.ApplicationCommand => await HandleApplicationCommand(interactionRequest, context),
                _ => GenerateErrorResponse()
            };
        }

        private APIGatewayHttpApiV2ProxyResponse HandlePing(ILambdaContext context)
        {
            context.Logger.LogLine($"Handling with {nameof(HandlePing)}");
            return GenerateResponse(new InteractionResponse
            {
                Type = InteractionType.PingPong
            });
        }

        internal async Task<APIGatewayHttpApiV2ProxyResponse> HandleApplicationCommand(InteractionRequest request,
            ILambdaContext context)
        {
            context.Logger.LogLine($"Handling with {nameof(HandleApplicationCommand)}");

            if (!ApplicationCommandHandlerList.TryGetHandler(request.Data.Name,
                out IApplicationCommandHandler handler))
            {
                context.Logger.LogLine($"No registered handler for the {request.Data.Name} command.");
                return GenerateErrorResponse();
            }

            return GenerateResponse(await handler.HandleCommandAsync(request));
        }

        private APIGatewayHttpApiV2ProxyResponse GenerateResponse(InteractionResponse response)
        {
            if (response == null)
            {
                return GenerateErrorResponse();
            }

            return new()
            {
                StatusCode = (int) HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(response),
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
            };
        }

        private APIGatewayHttpApiV2ProxyResponse GenerateErrorResponse(HttpStatusCode code = HttpStatusCode.InternalServerError)
        {
            return new()
            {
                StatusCode = (int) code,
            };
        }
    }
}
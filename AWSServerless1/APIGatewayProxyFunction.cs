using Amazon.Lambda;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace AWSServerless1
{
    public abstract class APIGatewayProxyFunction : Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    {
        public override async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request,
            ILambdaContext lambdaContext)
        {
            Console.WriteLine("In overridden FunctionHandlerAsync...");

            if (request.Resource == "WarmingLambda")
            {
                int.TryParse(request.Body, out var concurrencyCount);

                if (concurrencyCount > 1)
                {
                    Console.WriteLine($"Warming instance {concurrencyCount}.");
                    var client = new AmazonLambdaClient();
                    await client.InvokeAsync(new Amazon.Lambda.Model.InvokeRequest
                    {
                        FunctionName = lambdaContext.FunctionName,
                        InvocationType = InvocationType.RequestResponse,
                        Payload = JsonConvert.SerializeObject(new APIGatewayProxyRequest
                        {
                            Resource = request.Resource,
                            Body = (concurrencyCount - 1).ToString()
                        })
                    });
                }

                return new APIGatewayProxyResponse { };
            }

            return await base.FunctionHandlerAsync(request, lambdaContext);
        }
    }
}

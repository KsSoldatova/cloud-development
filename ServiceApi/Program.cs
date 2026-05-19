using Amazon.SQS;
using LocalStack.Client.Extensions;
using ServiceApi.Generator;
using ServiceApi.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisDistributedCache("RedisCache");

builder.Services.AddScoped<IGeneratorService, GeneratorService>();
builder.Services.AddScoped<IProgramProjectCache, ProgramProjectCache>();

builder.Services.AddLocalStack(builder.Configuration);
builder.Services.AddAwsService<IAmazonSQS>();
builder.Services.AddScoped<IProducerService, SqsProducerService>();

var app = builder.Build();
app.MapDefaultEndpoints();

app.MapGet("/program-project", (IGeneratorService service, int id) => service.ProcessProgramProject(id));
app.Run();

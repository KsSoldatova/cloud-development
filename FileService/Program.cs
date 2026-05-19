using Amazon.S3;
using Amazon.SQS;
using FileService.Messaging;
using FileService.Storage;
using LocalStack.Client.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var assembly = Assembly.GetExecutingAssembly();
    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddLocalStack(builder.Configuration);
builder.Services.AddAwsService<IAmazonSQS>();
builder.Services.AddAwsService<IAmazonS3>();
builder.Services.AddScoped<IS3Service, S3AwsService>();
builder.Services.AddHostedService<SqsConsumerService>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var s3Service = scope.ServiceProvider.GetRequiredService<IS3Service>();
await s3Service.EnsureBucketExists();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapDefaultEndpoints();
app.MapControllers();
app.Run();

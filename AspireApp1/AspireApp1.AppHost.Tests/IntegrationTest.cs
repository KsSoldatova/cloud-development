using Aspire.Hosting;
using Microsoft.Extensions.Logging;
using ServiceApi.Entities;
using System.Text.Json;
using Xunit.Abstractions;

namespace AspireApp.AppHost.Tests;

/// <summary>
/// Интеграционные тесты для проверки микросервисного пайплайна
/// </summary>
/// <param name="output">Служба журналирования юнит-тестов</param>
public class IntegrationTest(ITestOutputHelper output) : IAsyncLifetime
{
    private DistributedApplication? _app;
    private HttpClient? _gatewayClient;
    private HttpClient? _fileClient;

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        var cancellationToken = CancellationToken.None;
        IDistributedApplicationTestingBuilder builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AspireApp_AppHost>(cancellationToken);
        builder.Configuration["DcpPublisher:RandomizePorts"] = "false";
        builder.Services.AddLogging(logging =>
        {
            logging.AddXUnit(output);
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter("Aspire.Hosting.Dcp", LogLevel.Debug);
            logging.AddFilter("Aspire.Hosting", LogLevel.Debug);
        });
        _app = await builder.BuildAsync(cancellationToken);
        await _app.StartAsync(cancellationToken);
        _gatewayClient = _app.CreateHttpClient("api-gateway", "http");
        _fileClient = _app.CreateHttpClient("fileservice", "http");
    }

    /// <summary>
    /// Проверяет, что вызов гейтвея:
    /// <list type="bullet">
    /// <item><description>В ответ отправляет сгенерированный программный проект</description></item>
    /// <item><description>Сериализует программный проект в S3 хранилище</description></item>
    /// <item><description>Данные в ответе гейтвея и в S3 идентичны</description></item>
    /// </list>
    /// </summary>
    [Fact]
    public async Task GatewayCall_StoresProgramProjectInS3()
    {
        var id = Random.Shared.Next(1, 1000);
        using var gatewayResponse = await _gatewayClient!.GetAsync($"/program-project?id={id}");
        var apiProject = JsonSerializer.Deserialize<ProgramProject>(await gatewayResponse.Content.ReadAsStringAsync());

        await Task.Delay(5000);
        using var listResponse = await _fileClient!.GetAsync("/api/s3");
        var fileList = JsonSerializer.Deserialize<List<string>>(await listResponse.Content.ReadAsStringAsync());
        using var s3Response = await _fileClient!.GetAsync($"/api/s3/programproject_{id}.json");
        var s3Project = JsonSerializer.Deserialize<ProgramProject>(await s3Response.Content.ReadAsStringAsync());

        Assert.NotNull(fileList);
        Assert.Single(fileList);
        Assert.NotNull(apiProject);
        Assert.NotNull(s3Project);
        Assert.Equal(id, s3Project.Id);
        Assert.Equivalent(apiProject, s3Project);
    }

    /// <summary>
    /// Проверяет, что после генерации двух разных программных проектов
    /// в S3 хранилище появляются два разных файла
    /// </summary>
    [Fact]
    public async Task TwoDifferentIds_ProduceTwoFilesInS3()
    {
        var firstId = Random.Shared.Next(1, 500);
        var secondId = Random.Shared.Next(501, 1000);

        await _gatewayClient!.GetAsync($"/program-project?id={firstId}");
        await _gatewayClient!.GetAsync($"/program-project?id={secondId}");

        await Task.Delay(5000);
        using var listResponse = await _fileClient!.GetAsync("/api/s3");
        var fileList = JsonSerializer.Deserialize<List<string>>(await listResponse.Content.ReadAsStringAsync());

        Assert.NotNull(fileList);
        Assert.Equal(2, fileList.Count);
        Assert.Contains($"programproject_{firstId}.json", fileList);
        Assert.Contains($"programproject_{secondId}.json", fileList);
    }

    /// <summary>
    /// Проверяет, что повторный запрос того же программного проекта
    /// обслуживается из кэша и не приводит к дублирующей отправке в S3
    /// </summary>
    [Fact]
    public async Task RepeatedRequest_DoesNotDuplicateFileInS3()
    {
        var id = Random.Shared.Next(1, 1000);

        await _gatewayClient!.GetAsync($"/program-project?id={id}");
        await _gatewayClient!.GetAsync($"/program-project?id={id}");
        await _gatewayClient!.GetAsync($"/program-project?id={id}");

        await Task.Delay(5000);
        using var listResponse = await _fileClient!.GetAsync("/api/s3");
        var fileList = JsonSerializer.Deserialize<List<string>>(await listResponse.Content.ReadAsStringAsync());

        Assert.NotNull(fileList);
        Assert.Single(fileList);
        Assert.Equal($"programproject_{id}.json", fileList[0]);
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        await _app!.StopAsync();
        await _app.DisposeAsync();
    }
}

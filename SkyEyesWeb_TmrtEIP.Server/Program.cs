using Microsoft.Extensions.Options;
using SkyEyesWeb_TmrtEIP.Server.Options;

try
{

    var builder = WebApplication.CreateBuilder(args);

    // 添加背景服務設定檔
    builder.Configuration.AddJsonFile("BackGroundSettings.json", optional: false, reloadOnChange: true);

    // Add services to the container.
    builder.Services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // 註冊 SchedulerSettings
    builder.Services.Configure<SchedulerSettings>(builder.Configuration);

    // 註冊背景服務
    builder.Services.AddHostedService<BackgroundWorker>();

    #region 註冊 Options 設定

    builder.Services.Configure<ConnectionStringsOptions>(
        builder.Configuration.GetSection("ConnectionStrings"));

    #endregion

    using (ServiceProvider serviceProvider = builder.Services.BuildServiceProvider())
    {
        IOptions<ConnectionStringsOptions> connectionStringsOptions =
            serviceProvider.GetRequiredService<IOptions<ConnectionStringsOptions>>();

        clsPostgreDB.ConnectionStrings = connectionStringsOptions.Value.DefaultConnectionString
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnectionString is missing.");
    }

    var app = builder.Build();

    app.UseDefaultFiles();
    app.UseStaticFiles();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.MapFallbackToFile("/index.html");

    try
    {

        var dt = clsPostgreDB.getList(
        @"Select * From SYS_tbPasswordValidationRules LIMIT 1"
        , new { }
        );
        if (dt.Count == 1)
        {
            clsConfig.TokenLimit = (int)dt[0]["TokenLimit"];
        }

    }
    catch (Exception ex)
    {
        Console.WriteLine("Error retrieving password validation rules: " + ex.Message);
    }

    // 添加中間件來處理 404 錯誤
    app.Use(async (context, next) =>
    {
        await next();

        if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
        {
            context.Response.Redirect("/");
        }
    });

    Console.WriteLine("SkyEyesWeb_TmrtEIP.Server started successfully.");

    app.Run();

}
catch (Exception ex)
{
    var logPath = Path.Combine(AppContext.BaseDirectory, "startup_error.txt");
    File.WriteAllText(logPath, $"[{DateTime.Now}] Fatal error:\n{ex}");
    throw;
}

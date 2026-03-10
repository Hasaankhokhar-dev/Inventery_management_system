




using ElectronNET.API;
using ElectronNET.API.Entities;
using FinalInventerySystem.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using System;
using System.IO;


var builder = WebApplication.CreateBuilder(args);

// Tell QuestPDF which license you are using
QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddRazorPages();

// ✅ UPDATED: SAFE DATA LOCATION
var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
var appFolder = Path.Combine(appDataPath, "FinalInventorySystem");
var dbPath = Path.Combine(appFolder, "inventory.db");

// ✅ Ensure directory exists
if (!Directory.Exists(appFolder))
{
    Directory.CreateDirectory(appFolder);
}

// ✅ Copy existing database if exists (for migration)
var oldDbPath = "inventory.db";
if (File.Exists(oldDbPath) && !File.Exists(dbPath))
{
    File.Copy(oldDbPath, dbPath);
    Console.WriteLine($"✅ Database migrated to safe location: {dbPath}");
}
else if (File.Exists(dbPath))
{
    Console.WriteLine($"✅ Using existing database: {dbPath}");
}

// ✅ UPDATED DATABASE CONNECTION - Safe location use karega
builder.Services.AddDbContext<ApplicationDBcontext>(options =>
{
    options.UseSqlite($"Data Source={dbPath}");
});

Console.WriteLine($"📊 Database path: {dbPath}");

// ✅ Electron services add karein
builder.Services.AddElectron();
builder.WebHost.UseElectron(args);

var app = builder.Build();

// ✅ IMPORTANT: Static files middleware add karein
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/", context =>
{
    context.Response.Redirect("/Inventories/Index");
    return Task.CompletedTask;
});

// ✅ Port set karein
app.Urls.Clear();
app.Urls.Add("http://localhost:5148");

// ✅ Database ensure create karein
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBcontext>();
    dbContext.Database.EnsureCreated();
}

// ✅ Electron window creation
if (HybridSupport.IsElectronActive)
{
    Console.WriteLine("🖥️ Starting as Desktop Application...");

    // Create electron window
    var window = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
    {
        Width = 1200,
        Height = 800,
        Show = true,
        Center = true,
        Title = "Final Inventory System"
    });

    // ✅ Load correct URL
    window.LoadURL("http://localhost:5148");

    Console.WriteLine("✅ Desktop Application Started Successfully!");
}
else
{
    Console.WriteLine("🌐 Starting as Web Application...");
    Console.WriteLine("📊 URL: http://localhost:5148");
    Console.WriteLine("🏠 Home page: http://localhost:5148/Inventories/Index");
}

app.Run();
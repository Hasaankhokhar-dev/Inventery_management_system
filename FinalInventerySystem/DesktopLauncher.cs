//using System;
//using System.Diagnostics;
//using System.Threading;

//public static class DesktopLauncher
//{
//    public static void LaunchAsDesktop()
//    {
//        Console.WriteLine("🚀 Starting Desktop Application...");
//        Console.WriteLine("⏳ Application will start in 8 seconds...");

//        // Wait for app to start
//        System.Threading.Thread.Sleep(8000);

//        // Open browser
//        Console.WriteLine("🌐 Opening application in browser...");
//        OpenBrowser("http://localhost:5148");

//        Console.WriteLine("✅ Desktop Application Started Successfully!");
//        Console.WriteLine("📊 URL: http://localhost:5148");
//        Console.WriteLine("");
//        Console.WriteLine("❌ Press ANY key to stop the application...");

//        Console.ReadKey();

//        Console.WriteLine("🛑 Stopping application...");
//        Environment.Exit(0);
//    }

//    private static void OpenBrowser(string url)
//    {
//        try
//        {
//            Process.Start(new ProcessStartInfo
//            {
//                FileName = "cmd",
//                Arguments = $"/c start {url}",
//                CreateNoWindow = true
//            });
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"⚠️ Could not open browser automatically: {ex.Message}");
//            Console.WriteLine($"🌐 Please manually open: {url}");
//        }
//    }
//}
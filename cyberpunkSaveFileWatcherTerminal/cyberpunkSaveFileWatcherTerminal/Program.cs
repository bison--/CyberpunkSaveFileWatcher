using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace cyberpunkSaveFileWatcherTerminal
{
    class Program
    {
        // C:\Users\Gamer\Saved Games\CD Projekt Red\Cyberpunk 2077
        static string savePath = "\\Saved Games\\CD Projekt Red\\Cyberpunk 2077";
        static System.IO.FileSystemWatcher fileWatcher = new FileSystemWatcher();
        static System.Timers.Timer checkTimer = new System.Timers.Timer();
        static List<string> changedFiles = new List<string>();
        static NotifyIcon nfi = new NotifyIcon();
        static ContextMenu menu;
        static MenuItem mnuExit;

        static void Main(string[] args)
        {
            checkTimer.Interval = 1500;
            checkTimer.Elapsed += OnTimerTick;

            /*
            //TODO: https://stackoverflow.com/questions/12817468/system-tray-icon-with-c-sharp-console-application-wont-show-menu
            System.Threading.Thread notifyThread = new System.Threading.Thread(
                delegate ()
                {
                    //menu = new ContextMenu();
                    //mnuExit = new MenuItem("Exit");
                    //menu.MenuItems.Add(0, mnuExit);
                    
                        nfi = new NotifyIcon()
                        {
                            //Icon = Properties.Resources.Services,
                            ContextMenu = menu,
                            Text = "Main"
                        };
                        //mnuExit.Click += new EventHandler(mnuExit_Click);

                    nfi.Text = "CyberPunk Safe File checker";
                    nfi.Visible = true;
                    Application.Run();
                }
            );

            notifyThread.Start();*/

            run();
        }

        static void run()
        {
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("              .,:c,        ,c.                                              ");
            Console.WriteLine("          .,:::;'..      'oo'                         .               .''.  ");
            Console.WriteLine("      .,cll:,.    ;c,cl:okkdllooc;,cl::clodo,.,l:;:ldxxc'cl:..,;cxl;;;,.    ");
            Console.WriteLine("  .;loxOOxl::::coccdOkxkxllldxxlc,:kkdxxol;,'c0Oc;;:do;:dxk0doooxxkk:.      ");
            Console.WriteLine("  .';;;,,,'......'loclkd;'..,,'..:o;.,:llll:dOl'.  'dlcoc:;'lc';' .,;'.     ");
            Console.WriteLine("                'lc. ...         ''       .ck:      .. .              ...   ");
            Console.WriteLine("               .;.                        'l;                              .");
            
            Console.WriteLine();
            Console.WriteLine("CyberPunk save file watcher v0.0.1");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;

            savePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + savePath;
            fileWatcher.Path = savePath;
            fileWatcher.NotifyFilter = NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName
                                     | NotifyFilters.Size;

            fileWatcher.Filter = "*.dat";

            // Add event handlers.
            fileWatcher.Changed += OnChanged;
            fileWatcher.Created += OnChanged;
            fileWatcher.Deleted += OnChanged;
            fileWatcher.Renamed += OnRenamed;
            
            fileWatcher.IncludeSubdirectories = true;
            fileWatcher.EnableRaisingEvents = true;

            Console.WriteLine($"Scanning {savePath}");
            Console.WriteLine("reading stats...");
            Console.WriteLine();

            System.Threading.Thread.Sleep(1500);

            readPath();

            Console.WriteLine();
            
            Console.WriteLine($"MONITORING SAVES UNDER: {savePath}");
            Console.WriteLine("Press 'enter' to exit");
            Console.WriteLine();

            // Wait for the user to quit the program.
            while (Console.Read() <= 0);
        }

        static void readPath()
        {
            int countAllDirs = 0;
            int countQuickSave = 0;
            int countAutoSave = 0;
            int countManualSave = 0;

            System.IO.FileInfo biggestFile = null;
            long biggestSize = 0;

            foreach (string dir in Directory.GetDirectories(savePath)) {
                Console.WriteLine("**************");

                //Console.WriteLine(dir);
                countAllDirs++;

                if (dir.Contains("QuickSave")) {
                    countQuickSave++;
                } else if (dir.Contains("AutoSave")) {
                    countAutoSave++;
                } else if (dir.Contains("ManualSave")) {
                    countManualSave++;
                }

                string saveFile = dir + "\\sav.dat";

                if (System.IO.File.Exists(saveFile))
                {
                    FileInfo fInfo = new System.IO.FileInfo(saveFile);
                    if (fInfo.Length > biggestSize)
                    {
                        biggestFile = fInfo;
                    }

                    showFileStats(fInfo);
                }
            }

            Console.WriteLine();
            Console.WriteLine("==== stats ====");
            Console.WriteLine($"SaveFiles: {countAllDirs}");
            Console.WriteLine($"QuickSaves: {countQuickSave}");
            Console.WriteLine($"AutoSaves: {countAutoSave}");
            Console.WriteLine($"ManualSaves: {countManualSave}");
            Console.WriteLine();

            if (biggestFile != null)
            {
                Console.WriteLine("==== Biggest Save File ====");
                showFileStats(biggestFile);
                Console.WriteLine($"This save file is {getSizePercentageString(biggestFile.Length)} % corrupt.");
                Console.WriteLine("When it reaches 100 % you WON'T be able to load it anymore!");

                showWarning(getSizePercentage(biggestFile.Length));
            }
        }

        static void showFileStats(FileInfo fInfo)
        {
            Console.WriteLine($"Save Name: {fInfo.Directory.Name}");
            Console.WriteLine($"Save Date: {fInfo.LastWriteTime}");
            consoleWriteLineWarn($"Save Size: {BytesToString(fInfo.Length)} | {getSizePercentageString(fInfo.Length)} %", getSizePercentage(fInfo.Length));
        }

        static void OnChanged(object source, FileSystemEventArgs e)
        {
            checkTimer.Stop();
            if (!changedFiles.Contains(e.FullPath))
            {
                changedFiles.Add(e.FullPath);
            }

            // Specify what is done when a file is changed, created, or deleted.
            /*Console.WriteLine();
            Console.WriteLine("**************");
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
            showFileStats(new System.IO.FileInfo(e.FullPath));*/

            checkTimer.Start();
        }

        static void OnRenamed(object source, RenamedEventArgs e)
        {
            checkTimer.Stop();
            if (!changedFiles.Contains(e.FullPath))
            {
                changedFiles.Add(e.FullPath);
            }

            // Specify what is done when a file is renamed.
            /*Console.WriteLine();
            Console.WriteLine("**************");
            Console.WriteLine($"File: {e.OldFullPath} renamed to {e.FullPath}");
            showFileStats(new System.IO.FileInfo(e.FullPath));*/

            checkTimer.Start();
        }      

        static void OnTimerTick(Object source, System.Timers.ElapsedEventArgs e)
        {
            checkTimer.Stop();
            if (changedFiles.Count == 0)
            {
                return;
            }

            string checkFile = changedFiles[0];
            changedFiles.RemoveAt(0);

            Console.WriteLine();
            Console.WriteLine("**************");
            showFileStats(new System.IO.FileInfo(checkFile));

            float percentage = getSizePercentage(checkFile.Length);
            showWarning(percentage);

            if (changedFiles.Count > 0)
            {
                checkTimer.Start();
            }
        }

        static string getSizePercentageString(long size)
        {
            return string.Format("{0:n2}", getSizePercentage(size));
        }

        static float getSizePercentage(long size)
        {
            return ((float)size / 8388608f) * 100f;
        }

        static void showWarning(float percentage)
        {
            if (percentage >= 98f)
            {
                nfi.ShowBalloonTip(2000, "CyberPunk Save corruption iminent!", "Save file size too big! Clear your inventory!", ToolTipIcon.Error);
            }
            else if (percentage >= 75f)
            {
                nfi.ShowBalloonTip(2000, "CyberPunk Save corruption iminent!", "Save file size too big! Clear your inventory!", ToolTipIcon.Warning);
            }
        }

        static void consoleWriteLineWarn(string txt, float percentage)
        {
            Console.ForegroundColor = ConsoleColor.White;

            if (percentage >= 98f)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
            }
            else if (percentage >= 90f)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (percentage >= 75f)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            }
            else if (percentage >= 50f)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }

            Console.WriteLine(txt);

            Console.ForegroundColor = ConsoleColor.White;
        }

        static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }
    }
}

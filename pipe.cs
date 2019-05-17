using System;
using System.IO;
using System.Diagnostics;
using System.Text;

public class PipeTest
{
    public static int Main(string[] args) 
    {
        string cmd1  = "cat";
        string args1 = "infile";

        string cmd2  = "sort";
        string args2 = "";

        // Let us pipe the output of cmd1 into cmd2.

        Process p1 = new Process();
        p1.StartInfo.FileName  = cmd1;
        p1.StartInfo.Arguments = args1;
        p1.StartInfo.UseShellExecute = false;
        p1.StartInfo.RedirectStandardOutput = true;
        p1.EnableRaisingEvents = true;
        
        Process p2 = new Process();
        p2.StartInfo.FileName  = cmd2;
        p2.StartInfo.Arguments = args2;
        p2.StartInfo.UseShellExecute = false;
        p2.StartInfo.RedirectStandardInput = true;
        p2.EnableRaisingEvents = true;
        p2.Exited += 
            new EventHandler((sender, e) => {
                Debug.WriteLine("<<<p2 exits>>>");
            });
       
        // First start Process p2, the receiver, to ensure that the 
        // data is sent to a valid process.
        p2.Start();
        StreamWriter p2Stdin = p2.StandardInput;

        p1.OutputDataReceived += 
            new DataReceivedEventHandler((sender, e) => {
                if (e.Data == null) { // p1 sent an EOF. 
                    Debug.WriteLine("<<<p1 EOF Data Received>>>");
                    Debug.Assert(p1.HasExited, "p1 has exited.");
                    p2Stdin.Close(); // Sends an EOF to p2, which should close it also.
                    return;
                }
                if (!p2.HasExited) {
                    // We still need to catch exceptions
                    // because p2 might exit at any time.
                    p2Stdin.WriteLine(e.Data);
                }
            });
        p1.Exited += 
            new EventHandler((sender, e) => {
                Debug.WriteLine("<<<p1 exits>>>");
            });

        p1.Start();
        p1.BeginOutputReadLine();
        p1.WaitForExit();

        // Once p1 has exited, we should probably forcibly close p2 as well?
        p2.WaitForExit();

        return 1;
    }
}

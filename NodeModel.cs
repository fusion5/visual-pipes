using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.IO;

// A node is a process of which the program monitors and redirects
// its inputs and outputs according to arrows drawn in the diagram.

public enum NodeState {Running, Stopped}

[Serializable]
public class NodeModel : ISerializable
{
    public uint ID; // Id of the node. This helps us to serialize / deserialize.
    public string ShellCommand = "";
    public NodeState State = NodeState.Stopped;
    public int LastPID; // Process ID of the running node

    public int X; // Coords are in the model bc. they are to be
    public int Y; // serialized...

    public Process P; // The process

    private uint stdoutNodeID = 0;
    private uint stderrNodeID = 0;
    // private uint stdinNodeID  = 0;

    private DataReceivedEventHandler H;

    public NodeModel(uint id) {
        ID = id;
        InitProc();
    }

    public NodeModel(SerializationInfo info, StreamingContext c)
    {
        ID = (uint) info.GetValue("ID", typeof(uint));
        stdoutNodeID = (uint)   info.GetValue("ID_OUT", typeof(uint));
        stderrNodeID = (uint)   info.GetValue("ID_ERR", typeof(uint));
        // stdinNodeID  = (uint)   info.GetValue("ID_IN",  typeof(uint));
        ShellCommand = (string) info.GetValue("CMD",    typeof(string));
        X = (int) info.GetValue("X", typeof(int));
        Y = (int) info.GetValue("Y", typeof(int));
        InitProc();
    }

    private void InitProc() {
        P = new Process();
        // Parse ShellCommand into FileName and arguments.

        int sp = ShellCommand.IndexOf(" ");
        if (sp > -1) {
            P.StartInfo.FileName  = ShellCommand.Substring(0, sp);
            P.StartInfo.Arguments = ShellCommand.Substring(sp + 1);
        } else
            P.StartInfo.FileName = ShellCommand;
        
        P.StartInfo.UseShellExecute = false;
        P.StartInfo.RedirectStandardInput  = true;
        P.StartInfo.RedirectStandardOutput = true;
        P.EnableRaisingEvents = true;
        P.Exited += 
            new EventHandler((sender, e) => {
                Debug.WriteLine(P.StartInfo.FileName, "Exit");
                P.CancelOutputRead();
                State = NodeState.Stopped;
            });

        H = new DataReceivedEventHandler((sender, e) => {
            if (e.Data == null) // upstream Node sent an EOF
            {
                Debug.WriteLine("<<<Upstream EOF data received>>>");
                // Sends an EOF to our process, which should end it.
                P.StandardInput.Close(); 
                return;
            }
            if (!P.HasExited) 
            {
                Debug.WriteLine(e.Data, "Relaying data");
                P.StandardInput.WriteLine(e.Data); // Relay the data from upstream
            }
        });
    }

    public void GetObjectData(SerializationInfo info, StreamingContext c)
    {
        info.AddValue("ID",     ID,           typeof(uint));
        info.AddValue("ID_OUT", stdoutNodeID, typeof(uint));
        info.AddValue("ID_ERR", stderrNodeID, typeof(uint));
        // info.AddValue("ID_IN",  stdinNodeID,  typeof(uint));
        info.AddValue("CMD",    ShellCommand, typeof(string));
        info.AddValue("X", X, typeof(int));
        info.AddValue("Y", Y, typeof(int));
    }

    public void SetOutNode(NodeModel n) {
        if (n == this) throw new InvalidOperationException(
            "A node cannot pipe its standard output to its own input.");
        if (n == null) {
            this.stdoutNodeID = 0;
            return;
        }
        this.stdoutNodeID = n.ID;
    }

    public void SetErrNode(NodeModel n) {
        if (n == this) throw new InvalidOperationException(
            "A node cannot pipe its error output to its own input.");
        if (n == null) {
            this.stderrNodeID = 0;
            return;
        }
        this.stderrNodeID = n.ID;
    }

    /*
    public void SetInNode(NodeModel n) {
        if (n == this) throw new InvalidOperationException(
            "A node cannot be its own in node.");
        if (n == null) {
            this.stdinNodeID = 0;
            return;
        }

    }
    */

    public uint GetOutNodeID() {
        return this.stdoutNodeID;
    }

    public uint GetErrNodeID() {
        return this.stderrNodeID;
    }

    // When a certain node is removed, ensure that the links are reset.
    public void NodeRemove(NodeModel m) {
        if (stdoutNodeID == m.ID) stdoutNodeID = 0;
        if (stderrNodeID == m.ID) stderrNodeID = 0;
    }

    public void Run(NodeModel upstreamNode) 
    {
        Debug.WriteLine (P.StartInfo.FileName, "Run");
        Debug.Assert (
                upstreamNode == null 
            ||  upstreamNode.State == NodeState.Stopped,
            "If an upstream node exists then it shouldn't be running yet"
        );

        State = NodeState.Running;

        if (upstreamNode != null) 
        {
            // Remove if it exists (this avoids data duplication)
            upstreamNode.P.OutputDataReceived -= H; 
            upstreamNode.P.OutputDataReceived += H;
        }
        P.Start();
        P.BeginOutputReadLine();

        // P.WaitForExit();
    }
}

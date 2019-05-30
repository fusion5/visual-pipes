using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

enum EditorState {Start, AddingNode, MovingNode, AddingPipe};

public class VisualPipes : Form
{
    EditorState s;
    HashSet<NodeView>       nodes = new HashSet<NodeView>();
    HashSet<ConnectionView> links = new HashSet<ConnectionView>();

    // A temporary connection for creation purposes (used when we start
    // dragging from a node but we haven't dropped it anywhere yet):
    ConnectionView          NewConnection; 

    NodeView LastHitNode; // The node that was last hit
    int MovingX, MovingY;

    CheckBox addButton;

    static public void Main ()
    {
        Application.Run (new VisualPipes());
    }
 
    public VisualPipes ()
    {
        addButton        = new CheckBox();
        addButton.Text   = "Add node";
        addButton.Click += AddButtonClick;
        addButton.Appearance = System.Windows.Forms.Appearance.Button;

        Controls.Add (addButton);

        this.Size = new Size(640, 480);
        this.StartPosition = FormStartPosition.CenterScreen;
        // this.MouseClick   += WindowClick;
        this.MouseDown    += WindowMouseDown;
        this.MouseUp      += WindowMouseUp;
        this.MouseMove    += WindowMouseMove;
    }

    private void WindowMouseUp(object sender, MouseEventArgs e)
    {
        switch (s) {
            case EditorState.MovingNode:
            {
                s = EditorState.Start;
                LastHitNode = null;
                break;
            }
            case EditorState.AddingPipe:
            {
                s = EditorState.Start;
                bool hitFound = false;
                foreach (NodeView n in nodes) 
                {
                    hitFound = n.HitTest(e.X, e.Y);
                    if (!hitFound) continue;

                    NewConnection.To    = n;
                    NewConnection.State = ConnectionState.Start;
                    links.Add(NewConnection);

                    break;
                }
                NewConnection = null; // Delete the reference to NewConnection.
                LastHitNode   = null;
                break;
            }
        }
        UpdateView();
    }

    private void WindowMouseMove (object sender, MouseEventArgs e)
    {
        switch (s) {
            case EditorState.MovingNode:
            {
                Debug.Assert (LastHitNode != null);

                int NewX = e.X + MovingX;
                int NewY = e.Y + MovingY;

                NewX = Math.Max(0, NewX);
                NewX = Math.Min(NewX,
                        ClientRectangle.Width  - NodeView.NodeWidth);
                NewY = Math.Max(0, NewY);
                NewY = Math.Min(NewY, 
                        ClientRectangle.Height - NodeView.NodeHeight);

                LastHitNode.X = NewX;
                LastHitNode.Y = NewY;

                this.Refresh();
                break;
            }
            case EditorState.AddingPipe:
            {
                Debug.Assert (LastHitNode   != null);
                Debug.Assert (NewConnection != null);
                // Create a connection from the last hit node to the
                // current node (TODO: if we can).
                Debug.Assert (NewConnection.State == ConnectionState.Dragging);
                NewConnection.DraggingX = e.X;
                NewConnection.DraggingY = e.Y;

                this.Refresh();
                break;
            }
        }
    }

    private void WindowMouseDown (object sender, MouseEventArgs e)
    {
        switch (s) {
            case EditorState.AddingNode: 
            {
                NodeView n = new NodeView();
                n.X = e.X;
                n.Y = e.Y;
                n.Selected = false;

                nodes.Add(n);
                s = EditorState.Start;
                UpdateView();
                break;
            }
            case EditorState.Start:
            {
                // Hit test all nodes to see if we should select any of 
                // them. Only 1 node can be selected at the time.
                bool hitFound = false;
                foreach (NodeView n in nodes) {
                    if (hitFound) 
                    {
                        n.Selected = false; 
                        continue;
                    }
                    hitFound = n.HitTest(e.X, e.Y);
                    if (!hitFound) continue;

                    Debug.Assert(hitFound);
                    LastHitNode = n;

                    if (n.HitTestStdout(e.X, e.Y)) 
                    {
                        // We've hit stdout. Add a pipe.
                        Debug.WriteLine("HitTestStdout");
                        NewConnection = new ConnectionView();
                        NewConnection.From = n;
                        NewConnection.FromPort = NodePort.NodePortOut;
                        NewConnection.State = ConnectionState.Dragging;
                        NewConnection.DraggingX = e.X;
                        NewConnection.DraggingY = e.Y;

                        s = EditorState.AddingPipe;
                    } 
                    else if (n.HitTestStderr(e.X, e.Y)) 
                    {
                        Debug.WriteLine("HitTestStderr");
                        NewConnection = new ConnectionView();
                        NewConnection.From = n;
                        NewConnection.FromPort = NodePort.NodePortErr;
                        NewConnection.State = ConnectionState.Dragging;
                        NewConnection.DraggingX = e.X;
                        NewConnection.DraggingY = e.Y;

                        s = EditorState.AddingPipe;
                    }
                    else
                    {
                        Debug.WriteLine("HitTest General area");
                        MovingX    = n.X - e.X;
                        MovingY    = n.Y - e.Y;
                        s = EditorState.MovingNode;
                    }
                }
                UpdateView();
                break;
            }
            case EditorState.MovingNode:
            {
                Debug.Assert(false);
                break;
            }
        }
    }

    private void UpdateView()
    {
        addButton.Checked = (s == EditorState.AddingNode);
        Refresh();
    }

    private void AddButtonClick (object sender, EventArgs e)
    {
        s = EditorState.AddingNode;
    }

    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) 
    {
        var g = e.Graphics;

        if (NewConnection != null) {
            Debug.Assert (NewConnection.State == ConnectionState.Dragging);
            NewConnection.Draw(g);
        }

        foreach (ConnectionView c in links) {
            c.Draw(g);
        }
        foreach (NodeView n in nodes) {
            n.Draw(g);
        }
    }

}

/*
public class PipeTest
{

    public static int Main(string[] args) 
    {

        w = new CodeOnlyWindow();

        return 0;

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
*/

using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;

enum EditorState {Start, AddingNode, MovingNode, AddingPipe, ContextMenu, 
    EditProperties};

public class VisualPipes : Form
{
    string OpenFilename = ""; // Name of file we're editing

    EditorState s;
    HashSet<NodeView>           nodevs = new HashSet<NodeView>(); // Node views
    HashSet<ConnectionView>     links  = new HashSet<ConnectionView>();
    Dictionary<uint, NodeModel> nodems = new Dictionary<uint, NodeModel>();

    // A temporary connection for creation purposes (used when we start
    // dragging from a node but we haven't dropped it anywhere yet):
    ConnectionView  NewConnection; 
    ContextMenuView CMenu;

    NodeView SelectedNode; // The node that was last hit
    int MovingX, MovingY;

    CheckBox addButton;

    static public void Main (string[] args)
    {
        string editFile = "";

        foreach (string arg in args) {
            editFile = arg;
        }

        Application.Run (new VisualPipes(editFile));
    }
 
    public VisualPipes (string EditingFileName)
    {
        Debug.WriteLine(
            "Starting Visual-Pipes, the visual real-time pipe composer");

        OpenFilename = EditingFileName;
        
        addButton        = new CheckBox();
        addButton.Text   = "Add node";
        addButton.Click += AddButtonClick;
        addButton.Appearance = System.Windows.Forms.Appearance.Button;

        Controls.Add (addButton);

        CMenu = new ContextMenuView();
        CMenu.AddSlice(MenuSlice.Cancel,     "Cancel");
        CMenu.AddSlice(MenuSlice.Properties, "Properties");
        CMenu.AddSlice(MenuSlice.ViewStdout, "stdout");
        CMenu.AddSlice(MenuSlice.ViewStdin,  "stdin");
        CMenu.AddSlice(MenuSlice.ViewStderr, "stderr");
        CMenu.AddSlice(MenuSlice.Delete,     "Delete");
        CMenu.AddSlice(MenuSlice.Start,      "Start");

        CMenu.OnSelect += OnMenuSelect;

        Size = new Size(640, 480);
        StartPosition = FormStartPosition.CenterScreen;
        MouseDown    += WindowMouseDown;
        MouseUp      += WindowMouseUp;
        MouseMove    += WindowMouseMove;

        Text = "Visual Pipes <" + OpenFilename + ">";

        if (OpenFilename.Length > 0) {
            Deserialize();
        }
    }

    private void Deserialize() 
    {
        if (OpenFilename.Length > 0) {
            // Add models
            FileStream fs = File.Open(
                OpenFilename,
                FileMode.Open
            );
            BinaryFormatter fmt = new BinaryFormatter();
            nodems = (Dictionary<uint, NodeModel>) fmt.Deserialize(fs);
            // Create the views (nodes and links) based on the nodems.
            foreach (KeyValuePair<uint, NodeModel>m in nodems) {
                NodeView v = new NodeView();
                v.Selected = false;
                v.Model = m.Value;
                nodevs.Add(v);
                // TODO: ADD THE LINK VIEWS AS WELL
            }
            fs.Close();
        }
    }

    private void Serialize() 
    {
        if (OpenFilename.Length > 0) {
            // Save the model in the file.
            FileStream fs = File.Open(
                OpenFilename, 
                FileMode.OpenOrCreate,
                FileAccess.Write
            );
            BinaryFormatter fmt = new BinaryFormatter();
            fmt.Serialize(fs, nodems);
            fs.Close();
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        Serialize();
        base.OnClosing(e);
    }

    private void WindowMouseUp(object sender, MouseEventArgs e)
    {
        switch (s) {
            case EditorState.MovingNode:
            {
                s = EditorState.Start;
                SelectedNode = null;
                break;
            }
            case EditorState.AddingPipe:
            {
                s = EditorState.Start;
                bool hitFound = false;
                foreach (NodeView n in nodevs) 
                {
                    hitFound = n.HitTest(e.X, e.Y);
                    if (!hitFound) continue;

                    NewConnection.To    = n;
                    NewConnection.State = ConnectionState.Start;

                    // If NewConnection.To equals NewConnection.From
                    // we need to inform the user that this is not allowed.
                    if (NewConnection.To == NewConnection.From) {
                        Console.WriteLine(
                            "A node must refer to a different node");
                        break;
                    }

                    NewConnection.From.Model.SetOutNode(n.Model);
                    links.Add(NewConnection);

                    break;
                }
                NewConnection = null; // Delete the reference to NewConnection.
                SelectedNode  = null;
                break;
            }
            case EditorState.ContextMenu:
            {
                MouseUp -= CMenu.WindowMouseUp; 
                s = EditorState.Start;
                break;
            }
        }
        UpdateView();
    }

    public void PropertiesClosed (Object sender, FormClosedEventArgs e) 
    {
        Text = "Visual Pipes";
        s = EditorState.Start;
    }

    private void DeleteNode(NodeView n) 
    {
        // Delete all links on which n is present.
        links.RemoveWhere(c => c.From == n || c.To == n);
        foreach (NodeModel m in nodems.Values) {
            m.NodeRemove(n.Model);
        }
        nodems.Remove(n.Model.ID);
        nodevs.RemoveWhere(m => m == n);
    }

    private void OnMenuSelect(ContextMenuView c, ContextMenuEvent e) 
    {
        switch (e.SelectedSlice.Type) {
            case MenuSlice.Cancel: {
                // Do nothing
                break;
            }
            case MenuSlice.Delete: {
                // Deleting a node.
                Debug.Assert (SelectedNode != null);
                DeleteNode (SelectedNode);
                SelectedNode = null;
                UpdateView();
                break;
            }
            case MenuSlice.Properties: {

                Debug.Assert (SelectedNode != null);
                Text = "Visual Pipes <Properties>";

                s = EditorState.EditProperties;

                NodePropertiesForm props = 
                    new NodePropertiesForm(SelectedNode.Model);
                props.FormClosed += PropertiesClosed;
                props.ShowDialog(this);

                break;
            }
        }
    }

    private void WindowMouseMove (object sender, MouseEventArgs e)
    {
        switch (s) {
            case EditorState.MovingNode:
            {
                Debug.Assert (SelectedNode != null);

                int NewX = e.X + MovingX;
                int NewY = e.Y + MovingY;

                NewX = Math.Max(0, NewX);
                NewX = Math.Min(NewX,
                        ClientRectangle.Width  - NodeView.NodeWidth);
                NewY = Math.Max(0, NewY);
                NewY = Math.Min(NewY, 
                        ClientRectangle.Height - NodeView.NodeHeight);

                SelectedNode.X = NewX;
                SelectedNode.Y = NewY;

                this.Refresh();
                break;
            }
            case EditorState.AddingPipe:
            {
                Debug.Assert (SelectedNode != null);
                Debug.Assert (NewConnection != null);
                // Create a connection from the last hit node to the
                // current node (TODO: if we can).
                Debug.Assert (NewConnection.State == ConnectionState.Dragging);
                NewConnection.DraggingX = e.X;
                NewConnection.DraggingY = e.Y;

                this.Refresh();
                break;
            }
            case EditorState.ContextMenu:
            {
                CMenu.MouseX = e.X;
                CMenu.MouseY = e.Y;
                this.Refresh();
                break;
            }
        }
    }

    // Called when we press the left mouse button
    private void WindowMouseStartDownRightButton (MouseEventArgs e)
    {
        bool hitFound = false;
        foreach (NodeView n in nodevs) {
            if (hitFound)
            {
                n.Selected = false; 
                continue;
            }
            hitFound = n.HitTest(e.X, e.Y);
            if (!hitFound) 
            {
                n.Selected = false;
                continue;
            }

            Debug.Assert(hitFound);
            SelectedNode = n;
            n.Selected = true;

            s = EditorState.ContextMenu;
            CMenu.X = e.X;
            CMenu.Y = e.Y;

            // Notify the context menu of when the mouse goes up.
            MouseUp += CMenu.WindowMouseUp; 
        }
    }
    // Called when we press the left mouse button
    private void WindowMouseStartDownLeftButton (MouseEventArgs e) 
    {
        bool hitFound = false;
        foreach (NodeView n in nodevs) {
            if (hitFound) 
            {
                n.Selected = false; 
                continue;
            }
            hitFound = n.HitTest(e.X, e.Y);
            if (!hitFound) 
            {
                n.Selected = false;
                continue;
            }

            Debug.Assert(hitFound);
            SelectedNode = n;
            n.Selected = true;

            if (n.HitTestStdout(e.X, e.Y)) 
            {
                // We've hit stdout. Add a pipe.
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
                MovingX    = n.X - e.X;
                MovingY    = n.Y - e.Y;
                s = EditorState.MovingNode;
            }
        }
    }

    private void WindowMouseDown (object sender, MouseEventArgs e)
    {
        switch (s) {
            case EditorState.AddingNode: 
            {
                uint maxKey = 0;
                if (nodems.Keys.Count > 0) maxKey = nodems.Keys.Max();
                maxKey++;
                NodeModel nm = new NodeModel(maxKey);
                nodems.Add(maxKey, nm);

                NodeView v = new NodeView();
                nm.X = e.X;
                nm.Y = e.Y;
                v.Model = nm;
                v.Selected = false;
                nodevs.Add(v);

                s = EditorState.Start;
                UpdateView();
                break;
            }
            case EditorState.Start:
            {
                if (e.Button == MouseButtons.Left) {
                    WindowMouseStartDownLeftButton(e);
                } else if (e.Button == MouseButtons.Right) {
                    // Bring up the context menu...
                    WindowMouseStartDownRightButton(e);
                }
                // Hit test all node views to see if we should select any of 
                // them. Only 1 node can be selected at the time.
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
        foreach (NodeView n in nodevs) {
            n.Draw(g);
        }

        if (s == EditorState.ContextMenu) CMenu.Draw(g);
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

using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

enum EditorState {Start, AddingNode};

public class NodeView {
    public const int NodeWidth  = 32;
    public const int NodeHeight = 32;
    public int  X, Y;
    public bool Selected;

    public void Draw(Graphics g, Pen selectedPen, Pen normalPen) {
        if (Selected)
            g.DrawRectangle(selectedPen, X, Y, 
                NodeView.NodeWidth, NodeView.NodeHeight);
        else
            g.DrawRectangle(normalPen, X, Y, 
                NodeView.NodeWidth, NodeView.NodeHeight);
    }
    public bool HitTest(int HX, int HY) {
        return (X <= HX) && (HX <= X + NodeWidth) 
            && (Y <= HY) && (HY <= Y + NodeHeight);
    }
}

public class VisualPipes : Form
{
    EditorState s;
    HashSet<NodeView> nodes = new HashSet<NodeView>();

    CheckBox addButton;

    static public void Main ()
    {
        Application.Run (new VisualPipes());
    }
 
    public VisualPipes ()
    {
        addButton        = new CheckBox();
        addButton.Text   = "Add node";
        addButton.Click += new EventHandler (Add_Button_Click);
        addButton.Appearance = System.Windows.Forms.Appearance.Button;

        Controls.Add (addButton);

        this.Size = new Size(640, 480);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MouseClick   += new MouseEventHandler (Window_Click);
        this.MouseDown    += new MouseEventHandler (Window_MouseDown);
    }

    private void Window_MouseDown (object sender, MouseEventArgs e) 
    {

    }

    private void Window_Click (object sender, MouseEventArgs e)
    {
        switch (s) {
            case EditorState.AddingNode: 
            {
                // MessageBox.Show ("Adding a node at coordinates: " + e.X + ", " + e.Y);
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
                    if (hitFound) {
                        n.Selected = false; 
                        continue;
                    }
                    n.Selected = n.HitTest(e.X, e.Y);
                    if (n.Selected) hitFound = true;
                }
                UpdateView();
                break;
            }
        }
    }

    private void UpdateView()
    {
        addButton.Checked = (s == EditorState.AddingNode);
        this.Refresh();
    }

    private void Add_Button_Click (object sender, EventArgs e)
    {
        s = EditorState.AddingNode;
    }

    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) 
    {
        var g = e.Graphics;

        using (Pen p = new Pen(Brushes.Black)) // IDisposable interface
            using (Pen b = new Pen(Brushes.Blue)) {
                b.Width = 2.0F;
                p.Width = 2.0F;
                foreach(NodeView n in nodes) {
                    n.Draw(g, b, p);
                }
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

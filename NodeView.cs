using System.Drawing;

// A node, as shown on the diagram. The *View classes
// render the nodes and the connections between them.

public enum NodePort {NodePortOut, NodePortErr, NodePortIn};

public class NodeView 
{
    public const int NodeWidth  = 50;
    public const int NodeHeight = 50;
    public bool Selected;

    public NodeModel Model; // Reference to the node model

    public int X {
        get { return Model.X; }
        set { Model.X = value; }
    }

    public int Y {
        get { return Model.Y; }
        set { Model.Y = value; }
    }

    private const int SmallBoxWidth  = 20;
    private const int SmallBoxHeight = 16;
    private const int TxtYOffset = -2;

    private Font         drawFont   = new Font("Arial", 10);
    private Brush        drawBrush  = new SolidBrush(Color.Black);
    private StringFormat drawFormat = new System.Drawing.StringFormat();

    private void DrawStdinBox(Graphics g, Pen p) 
    {
        // Draw a small box in the top right corner
        g.DrawRectangle(p, X, Y, SmallBoxWidth, SmallBoxHeight);
        g.DrawString(
            "in", 
            drawFont, 
            drawBrush, 
            X + 4,
            Y + TxtYOffset, 
            drawFormat
        );
    }

    private void DrawStdoutBox(Graphics g, Pen p) 
    {
        // Draw a small box in the top right corner
        g.DrawRectangle(p, 
                X + NodeWidth  - SmallBoxWidth,
                Y,
                SmallBoxWidth,
                SmallBoxHeight
        );
        g.DrawString(
            "out", 
            drawFont, 
            drawBrush, 
            X + NodeWidth - SmallBoxWidth,
            Y + TxtYOffset, 
            drawFormat
        );
    }

    private void DrawStderrBox(Graphics g, Pen p)
    {
        g.DrawRectangle(p, 
                X + NodeWidth  - SmallBoxWidth,
                Y + NodeHeight - SmallBoxHeight,
                SmallBoxWidth,
                SmallBoxHeight
        );
        g.DrawString(
            "err", 
            drawFont, 
            drawBrush, 
            X + NodeWidth  - SmallBoxWidth,
            Y + NodeHeight - SmallBoxHeight + TxtYOffset, 
            drawFormat
        );
    }

    public void Draw(Graphics g) 
    {
        Brush b;
        if (Selected) b = Brushes.Blue;
        else          b = Brushes.Black;

        using (Pen p = new Pen(b)) {
            p.Width = 2.0F;
            DrawStdoutBox(g, p);
            DrawStderrBox(g, p);
            DrawStdinBox (g, p);
            g.DrawRectangle(p, X, Y, 
                NodeView.NodeWidth, NodeView.NodeHeight);
        }
    }

    public bool HitTest(int HX, int HY) {
        return (X <= HX) && (HX <= X + NodeWidth) 
            && (Y <= HY) && (HY <= Y + NodeHeight);
    }

    public bool HitTestStdout(int HX, int HY) {
        // Top right corner
        return (X + NodeWidth - SmallBoxWidth <= HX) 
            && (HX <= X + NodeWidth) 
            && (Y <= HY) 
            && (HY <= Y + SmallBoxHeight);
    }

    public bool HitTestStderr(int HX, int HY) {
        // Bottom right corner
        return (X + NodeWidth - SmallBoxWidth <= HX)
            && (HX <= X + NodeWidth)
            && (Y + NodeHeight - SmallBoxHeight <= HY)
            && (HY <= Y + NodeHeight)
            ;
    }

    public Point PortPoint(NodePort port) 
    {
        var p = new Point();
        switch (port) {
            case NodePort.NodePortOut: 
            {
                p.X = X + NodeWidth;
                p.Y = Y + SmallBoxHeight / 2;
                break;
            }
            case NodePort.NodePortErr: 
            {
                p.X = X + NodeWidth;
                p.Y = Y + NodeHeight - SmallBoxHeight / 2;
                break;
            }
            case NodePort.NodePortIn:
            {
                p.X = X;
                p.Y = Y + SmallBoxHeight / 2;
                break;
            }
        }
        return p;
    }
}


using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;

public enum ConnectionState {Start, Dragging};

// A connection is a line between two nodes.
// The *View classes render the nodes and the connections between them.

public class ConnectionView 
{
    public NodeView From;
    public NodePort FromPort;
    public NodeView To;
    public ConnectionState State;
    public int DraggingX, DraggingY; // End coords. while it's being dragged

    public void Draw(Graphics g) 
    {
        Brush b; 

        if (FromPort == NodePort.NodePortOut)
            b = Brushes.Green;
        else
            b = Brushes.Red;

        using (Pen p = new Pen(b))
        using (GraphicsPath capPath = new GraphicsPath())
        {
            p.Width = 2.0F;

            capPath.AddLine(0, 0,  2, -6);
            capPath.AddLine(0, 0, -2, -6);

            p.CustomEndCap = new System.Drawing.Drawing2D.CustomLineCap(
                null, capPath);

            switch (State)
            {
                case ConnectionState.Dragging:
                {
                    g.DrawLine(p, 
                        From.PortPoint(FromPort),
                        new Point(DraggingX, DraggingY));
                    break;
                }
                case ConnectionState.Start:
                {
                    Debug.Assert(To != null);
                    g.DrawLine(p, 
                        From.PortPoint(FromPort),
                        To.PortPoint(NodePort.NodePortIn));
                    break;
                }
            }
        }
    }
}

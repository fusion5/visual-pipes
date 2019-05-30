using System.Drawing;
using System.Diagnostics;

public enum ConnectionState {Start, Dragging};

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
        {
            p.Width = 2.0F;
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

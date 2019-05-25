using System.Drawing;

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
        using (Pen p = new Pen(Brushes.Green)) {
            p.Width = 2.0F;
            switch (State) 
            {
                case ConnectionState.Dragging:
                    g.DrawLine(p, 
                        From.PortPoint(FromPort),
                        new Point(DraggingX, DraggingY));
                    break;
                case ConnectionState.Start:
                    g.DrawLine(p, From.X, From.Y, To.X, To.Y);
                    break;
            }
        }
    }
}

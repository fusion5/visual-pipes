using System.Drawing;

public class NodeView 
{
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


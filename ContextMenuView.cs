using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

// public event ...

public enum MenuSlice {
    Cancel, Properties, ViewStdout, ViewStdin, ViewStderr, Delete};

public class ContextMenuSlice 
{
    public string    Name;
    public MenuSlice Type;
    public ContextMenuSlice (MenuSlice t, string s) {
        Name = s;
        Type = t;
    }
}

public class ContextMenuView 
{
    public int  X, Y;

    private const int RadiusOut = 90;
    private const int RadiusIn  = 25;

    private const int NSlices = 7;
    private List<ContextMenuSlice> Slices = new List<ContextMenuSlice>();

    // Text rendering stuff
    private Font         TextFont   = new Font("Arial", 10);
    private Brush        TextBrush  = new SolidBrush(Color.Black);
    private StringFormat TextFormat = new System.Drawing.StringFormat();

    public void AddSlice(MenuSlice type, string text) {
        ContextMenuSlice s = new ContextMenuSlice(type, text);
        Slices.Add(s);
    }

    private void RenderText(Graphics g, string t, int X, int Y) 
    {
        g.DrawString(t, TextFont, TextBrush, X, Y, TextFormat);
    }

    public void Draw(Graphics g)
    {
        using (Pen p = new Pen(Color.White)) {
            p.Width = (RadiusOut - RadiusIn); 
            int r   = (RadiusOut + RadiusIn) / 2;
            g.DrawEllipse(p, X-r, Y-r, 2*r, 2*r);
        }
        using (Pen p = new Pen(Color.Black)) {
            p.Width = 1.0F;
            g.DrawEllipse(p, X-RadiusIn,  Y-RadiusIn,  2*RadiusIn,  2*RadiusIn);
            g.DrawEllipse(p, X-RadiusOut, Y-RadiusOut, 2*RadiusOut, 2*RadiusOut);
        }

        // Draw the slice borders
        using (Pen p = new Pen(Color.Black)) {
            foreach (var s in Slices) {
                // Render a text for each slice...
            }
            p.Width  = 1.0F;
            Matrix m = new Matrix();
            int ang  = 360 / Slices.Count;
            foreach (var s in Slices) {
                m.RotateAt(ang, new Point(X, Y), MatrixOrder.Append);
                g.Transform = m;
                g.DrawLine(p, X, Y + RadiusIn, X, Y + RadiusOut);
                RenderText(g, s.Name, X + RadiusIn + 5, Y - 6);
            }
        }
    }
}

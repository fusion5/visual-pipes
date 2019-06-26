using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

// public event ...

public enum MenuSlice {
    Cancel, Properties, ViewStdout, ViewStdin, ViewStderr, Delete, Start, Stop};

public class ContextMenuSlice 
{
    public string    Name;
    public MenuSlice Type;
    public ContextMenuSlice (MenuSlice t, string s) {
        Name = s;
        Type = t;
    }
}

public class ContextMenuEvent : EventArgs
{
    public ContextMenuSlice SelectedSlice;
}

public class ContextMenuView 
{
    public int X, Y;
    public int MouseX, MouseY; // Allows us to higlight one of the slices

    private const int RadiusOut = 90;
    private const int RadiusIn  = 25;

    private const int NSlices = 7;
    private List<ContextMenuSlice> Slices = new List<ContextMenuSlice>();

    // Text rendering stuff
    private Font         TextFont     = new Font("Arial", 10);
    private Brush        TextBrush    = new SolidBrush(Color.Black);
    private Brush        SelTextBrush = new SolidBrush(Color.Blue);
    private StringFormat TextFormat   = new System.Drawing.StringFormat();

    public ContextMenuSlice SelectedSlice;

    public event SelectHandler OnSelect;
    public delegate void SelectHandler(ContextMenuView c, ContextMenuEvent e);

    public void AddSlice(MenuSlice type, string text) {
        ContextMenuSlice s = new ContextMenuSlice(type, text);
        Slices.Add(s);
    }

    private double AngMouse() {
        // The angle formed by the right click position and
        // the current mouse position.
        double dY = MouseY - Y;
        double dX = MouseX - X;
        return Math.Atan2(dY, dX);
    }

    private void RenderText(Graphics g, string t, int X, int Y, bool sel) 
    {
        if (sel)
            g.DrawString(t, TextFont, SelTextBrush, X, Y, TextFormat);
        else
            g.DrawString(t, TextFont, TextBrush, X, Y, TextFormat);
    }

    public void WindowMouseUp(object sender, MouseEventArgs e) 
    {
        Debug.Assert (SelectedSlice != null);

        // A Mouse Up event occurred in the main window. 
        // This can only happen if the Context Menu is displayed.
        // Emit an event that a menu item has been selected.

        ContextMenuEvent ce = new ContextMenuEvent();
        ce.SelectedSlice = SelectedSlice;

        if (OnSelect != null) OnSelect(this, ce);
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
            p.Width      = 1.0F;
            Matrix m     = new Matrix();
            float ang    = (float) 360 / (float) Slices.Count;
            float angSum = 0;

            float angMouse = (float) AngMouse() * 
                (180/(float)Math.PI); // Radians to degrees

            // Make the mouse angle go from 0..360 as the slice angles.
            if (angMouse < 0) angMouse += 360; 

            foreach (var s in Slices) {
                bool sel = (angSum <= angMouse) && (angMouse <= (angSum + ang));

                m.Reset();
                m.RotateAt(angSum, new Point(X, Y), MatrixOrder.Append);
                g.Transform = m;

                g.DrawLine(p, X+RadiusIn, Y, X+RadiusOut, Y);

                // Rotate a little bit more to show the text
                m.RotateAt(ang/2, new Point(X, Y), MatrixOrder.Append);
                g.Transform = m;

                // FIXME: Render the text in upright position
                RenderText(g, s.Name, X+RadiusIn+5, Y-6, sel);

                g.ResetTransform();
                angSum += ang;

                if (sel) SelectedSlice = s;
            }
            m.Dispose();
            m = null;
        }
    }
}

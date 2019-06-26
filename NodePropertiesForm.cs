using System;
using System.Windows.Forms;
using System.Drawing;

public class NodePropertiesForm : Form
{
    TableLayoutPanel Table;
    TextBox Command;

    Button Save;
    Button Cancel;

    NodeModel EditNode;

    public NodePropertiesForm(NodeModel editing) {

        EditNode = editing;

        Size = new Size(480, 320);

        Command = new TextBox();
        Command.Dock = DockStyle.Fill;
        Command.Multiline = true;
        Command.Font = 
            new System.Drawing.Font(
                    System.Drawing.FontFamily.GenericMonospace, 11);

        Command.Text = EditNode.ShellCommand;

        Save = new Button();
        Save.Text = "Save";
        Save.Dock = DockStyle.Fill;
        Save.Click += SaveButtonClick;

        Cancel = new Button();
        Cancel.Text = "Cancel";
        Cancel.Dock = DockStyle.Fill;

        Table = new TableLayoutPanel();
        Table.Dock = DockStyle.Fill;

        Table.RowCount    = 2;
        Table.ColumnCount = 1;

        Table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        Table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

        Table.Controls.Add(Command, 0, 0);
        Table.Controls.Add(Save, 0, 1);

        Controls.Add(Table);

        Text = "Node Properties";
    }

    private void SaveButtonClick (object sender, EventArgs e)
    {
        EditNode.ShellCommand = Command.Text;
        Close();
    }
}

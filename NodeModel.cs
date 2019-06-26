using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public enum NodeState {Running, Stopped}

[Serializable]
public class NodeModel : ISerializable
{
    public uint ID; // Id of the node. This helps us to serialize / deserialize.
    public string ShellCommand;
    public NodeState State = NodeState.Stopped;
    public int PID; // Process ID of the running node

    public int X; // Coords are in the model bc. they are to be
    public int Y; // serialized...

    private uint stdoutNode = 0;
    private uint stderrNode = 0;

    public NodeModel(uint id) {
        ID = id;
    }

    public NodeModel(SerializationInfo info, StreamingContext c)
    {
        ID = (uint) info.GetValue("ID", typeof(uint));
        stdoutNode = (uint) info.GetValue("ID_OUT", typeof(uint));
        stderrNode = (uint) info.GetValue("ID_ERR", typeof(uint));
        ShellCommand = (string) info.GetValue("CMD", typeof(string));
        X = (int) info.GetValue("X", typeof(int));
        Y = (int) info.GetValue("Y", typeof(int));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext c)
    {
        info.AddValue("ID", ID, typeof(uint));
        info.AddValue("ID_OUT", stdoutNode, typeof(uint));
        info.AddValue("ID_ERR", stderrNode, typeof(uint));
        info.AddValue("CMD", ShellCommand, typeof(string));
        info.AddValue("X", X, typeof(int));
        info.AddValue("Y", Y, typeof(int));
    }

    public void SetOutNode(NodeModel n) {
        if (n == this) throw new InvalidOperationException(
            "A node cannot pipe its standard output to its own input.");
        this.stdoutNode = n.ID;
    }

    public void SetErrNode(NodeModel n) {
        if (n == this) throw new InvalidOperationException(
            "A node cannot pipe its error output to its own input.");
        this.stderrNode = n.ID;
    }

    // When a certain node is removed, ensure that the links are reset.
    public void NodeRemove(NodeModel m) {
        if (stdoutNode == m.ID) stdoutNode = 0;
        if (stderrNode == m.ID) stderrNode = 0;
    }
}

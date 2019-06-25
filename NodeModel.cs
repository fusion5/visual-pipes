using System;

public enum NodeState {Running, Stopped}

public class NodeModel
{
    public string ShellCommand;
    public NodeState State = NodeState.Stopped;
    public int PID; // Process ID of the running node

    private NodeModel stdoutNode = null;
    private NodeModel stderrNode = null;

    public void SetOutNode(NodeModel n) {
        if (n == this) throw new InvalidOperationException(
            "A node cannot pipe its standard output to its own input.");
        this.stdoutNode = n;
    }

    public void SetErrNode(NodeModel n) {
        if (n == this) throw new InvalidOperationException(
            "A node cannot pipe its error output to its own input.");
        this.stderrNode = n;
    }

    // When a certain node is removed, ensure that the links are reset.
    public void NodeRemove(NodeModel m) {
        if (stdoutNode == m) stdoutNode = null;
        if (stderrNode == m) stderrNode = null;
    }

}

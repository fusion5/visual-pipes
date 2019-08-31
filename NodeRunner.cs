using System.Collections.Generic;
using System.Diagnostics;

// Runs the nodes. All process code is here but we might
// want to refactor this later on.

public class NodeRunner 
{
    public void Start (
        NodeModel targetNode, 
        Dictionary<uint, NodeModel> nodems // Node Models
    ) {
        Dictionary<uint, NodeModel> parentNodes = 
            new Dictionary<uint, NodeModel>();

        // First start any parent nodes of TargetNode.
        foreach (KeyValuePair<uint, NodeModel>m in nodems) 
        {
            if (m.Value.GetOutNodeID() == targetNode.ID) {
                Start (m.Value, nodems);
                parentNodes.Add(m.Value.ID, m.Value);
            }
            if (m.Value.GetErrNodeID() == targetNode.ID) {
                Start (m.Value, nodems);
                parentNodes.Add(m.Value.ID, m.Value);
            }
        }

        // Launch the processes for targetNode.
        Process pTarget = new Process();
        pTarget.StartInfo.FileName = targetNode.ShellCommand;
    }
}


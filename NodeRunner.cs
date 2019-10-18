using System.Collections.Generic;
using System.Diagnostics;
using System;

public class NodeRunner 
{
    public void Start (
        NodeModel targetNode, 
        Dictionary<uint, NodeModel> nodems // Node Models
    ) {
        NodeModel parentNode = null;

        // First start the parent node of TargetNode (if any).
        foreach (KeyValuePair<uint, NodeModel>m in nodems)
        {
            if (m.Value.GetOutNodeID() == targetNode.ID) {
                if (parentNode != null) 
                    throw new InvalidOperationException(
                        "Cannot have more than one parent node!");
                parentNode = m.Value;
            }
            if (m.Value.GetErrNodeID() == targetNode.ID) {
                if (parentNode != null) 
                    throw new InvalidOperationException(
                        "Cannot have more than one parent node!");
                parentNode = m.Value;
            }
        }

        targetNode.Run(parentNode);

        if (parentNode != null)
            Start (parentNode, nodems);

    }
}


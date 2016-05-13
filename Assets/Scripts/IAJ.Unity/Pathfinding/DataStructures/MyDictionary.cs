using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    class MyDictionary : IClosedSet
    {
        private Dictionary<NodeRecord, NodeRecord> NodeRecords { get; set; }

        public MyDictionary()
        {
            this.NodeRecords = new Dictionary<NodeRecord, NodeRecord>();
        }

        public void Initialize()
        {
            this.NodeRecords.Clear();
        }
        public void AddToClosed(NodeRecord nodeRecord)
        {
            this.NodeRecords.Add(nodeRecord, nodeRecord);
        }
        public void RemoveFromClosed(NodeRecord nodeRecord)
        {
            this.NodeRecords.Remove(nodeRecord);
        }
        //should return null if the node is not found
        public NodeRecord SearchInClosed(NodeRecord nodeRecord)
        {
            NodeRecord record = null;
            if (this.NodeRecords.ContainsKey(nodeRecord))
                record = this.NodeRecords[nodeRecord];
            return record;
        }
        public ICollection<NodeRecord> All()
        {
            return this.NodeRecords.Values;
        }


    }
}

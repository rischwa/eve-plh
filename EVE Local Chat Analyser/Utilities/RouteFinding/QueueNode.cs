namespace EveLocalChatAnalyser.Utilities.RouteFinding
{
    internal class QueueNode : PriorityQueueNode
    {
        public readonly int IndexInSystemArray;

        public QueueNode(int indexInSystemArray)
        {
            IndexInSystemArray = indexInSystemArray;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return Equals((QueueNode) obj);
        }

        public override int GetHashCode()
        {
            return IndexInSystemArray;
        }

        protected bool Equals(QueueNode other)
        {
            return IndexInSystemArray == other.IndexInSystemArray;
        }
    }
}
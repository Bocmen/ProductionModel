using System.Collections.Generic;
using System.Linq;

namespace CoreProductionModel.Abstract
{
    public abstract class LoggerCondition
    {
        private readonly Stack<int> _depthIndexes = new Stack<int>();
        public int LastIndex { get; private set; }
        protected int Index { get; private set; }
        protected int? ParentIndex { get; private set; } = null;

        protected void Init(int startIndex)
        {
            LastIndex = startIndex - 1;
            Index = LastIndex;
            _depthIndexes.Clear();
        }

        public int NextLevel()
        {
            _depthIndexes.Push(Index);
            ParentIndex = Index;
            Index = LastIndex++;
            return Index;
        }
        public void UpLevel()
        {
            Index = _depthIndexes.Pop();
            ParentIndex = _depthIndexes.Any() ? _depthIndexes.Peek() : (int?)null;
            if(ParentIndex == Index) ParentIndex = null;
        }
        public abstract void LoggerCurrentLevel(NodeOperator nodeOperator, bool state);
    }
}

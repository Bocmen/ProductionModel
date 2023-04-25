using CoreProductionModel.Abstract;
using CoreProductionModel.Tools;
using System.Drawing;

namespace CoreProductionModel.Models
{
    public class DotTreeLoggerCondition : LoggerCondition
    {
        private DotTreeDrawOperator.Write _writer = (_) => {};

        public DotTreeLoggerCondition() { Init(int.MaxValue); }
        public DotTreeLoggerCondition(DotTreeDrawOperator.Write writer, int startIndex = int.MaxValue) => SetLoggerContext(writer, startIndex);

        public void SetLoggerContext(DotTreeDrawOperator.Write writer, int? startIndex = null)
        {
            Init(startIndex ?? LastIndex + 1);
            _writer = writer;
        }

        public override void LoggerCurrentLevel(NodeOperator nodeOperator, bool state) => DotTreeDrawOperator.DrawNode(_writer, ParentIndex, Index, nodeOperator, state ? Color.Black : Color.Red);
    }
}

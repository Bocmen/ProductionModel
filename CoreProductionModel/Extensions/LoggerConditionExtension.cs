using CoreProductionModel.Abstract;
using System;

namespace CoreProductionModel.Extensions
{
    public static class LoggerConditionExtension
    {
        public static bool LoggerCurrentLevel(this LoggerCondition logger, NodeOperator nodeOperator, Func<bool> getState)
        {
            logger?.NextLevel();
            bool result = getState();
            logger?.LoggerCurrentLevel(nodeOperator, result);
            logger?.UpLevel();
            return result;
        }
    }
}

using System;
using System.Windows;
using ELW.Library.Math.Expressions;

namespace CashpointModeling
{
    public struct ModelingConfiguration
    {
        public int ClientsCount { get; set; }
        public double StepRange { get; set; }
        public int CashpointsNumber { get; set; }
        public Size ClientSize { get; set; }
        public Size CashpointsSize { get; set; }
        public double QueueDistance { get; set; }
        public TimeLimits ClientWaitingLimit { get; set; }
        public TimeLimits ClientInteractionLimit { get; set; }
        public CompiledExpression WaitingTimeFormula { get; set; }
        public CompiledExpression InteractionTimeFormula { get; set; }
    }

    public struct TimeLimits
    {
        public TimeSpan Upper;
        public TimeSpan Lower;

        public TimeLimits(TimeSpan lower, TimeSpan upper)
        {
            Upper = upper;
            Lower = lower;
        }
    }
}

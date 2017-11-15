﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trady.Analysis.Infrastructure;
using Trady.Core.Infrastructure;

namespace Trady.Analysis.Indicator
{
    public class ZigZagSupport<TInput, TOutput> : AnalyzableBase<TInput, decimal, bool, TOutput>
    {
        private decimal _turningPointMargin;
        private int _requiredNumberOfTurningPoints;
        private ZigZagMinimasByCloses _zigZag;
        public ZigZagSupport(IEnumerable<TInput> inputs, Func<TInput, decimal> inputMapper, decimal zigZagThreshold = 0.03m, decimal turningPointMargin = 0.007m, int requiredNumberOfTurningPoints = 2) : base(inputs, inputMapper)
        {
            _turningPointMargin = turningPointMargin;
            _requiredNumberOfTurningPoints = requiredNumberOfTurningPoints;
            var closes = inputs.Select(inputMapper);
            _zigZag = new ZigZagMinimasByCloses(closes, zigZagThreshold);
        }
        protected override bool ComputeByIndexImpl(IReadOnlyList<decimal> mappedInputs, int index)
        {
            var maximas = _zigZag.Compute(endIndex: index).Where(c => c.HasValue && c.Value.CalculationIndex <= index).Select(c => c.Value).ToList();
            var close = mappedInputs[index];
            var numberOfNearbyMaximas = maximas.Count(m => m.Close + m.Close * _turningPointMargin >= close && m.Close - m.Close * _turningPointMargin <= close);
            return numberOfNearbyMaximas >= _requiredNumberOfTurningPoints;
        }
    }

    public class ZigZagSupport : ZigZagSupport<IOhlcv, AnalyzableTick<bool>>
    {
        public ZigZagSupport(IEnumerable<IOhlcv> inputs, decimal zigZagThreshold = 0.03m, decimal turningPointMargin = 0.007m, int requiredNumberOfTurningPoints = 2)
            : base(inputs, i => i.Close, zigZagThreshold, turningPointMargin, requiredNumberOfTurningPoints)
        {
        }
    }
}

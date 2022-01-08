using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sanitizer.PropertyValueBuilder
{
    public class DestructorLimits
    {
        private int _maximumDestructuringDepth = 10;
        private int _maximumStringLength = int.MaxValue;
        private int _maximumCollectionCount = int.MaxValue;

        public DestructorLimits(int maximumDestructuringDepth, int maximumStringLength, int maximumCollectionCount)
        {
            _maximumCollectionCount = maximumCollectionCount;
            _maximumDestructuringDepth = maximumDestructuringDepth;
            _maximumStringLength = maximumStringLength;
        }

        public int MaximumStringLength { get { return _maximumStringLength; } }

        public int MaximumDestructuringDepth { get { return _maximumDestructuringDepth; } }

        public int MaximumCollectionCount { get { return _maximumCollectionCount; } }


        public static DestructorLimits GetFromLoggerConfiguration(LoggerConfiguration loggerConfiguration)
        {
            return new DestructorLimits
            (
                GetDestructingLimit(nameof(_maximumDestructuringDepth), loggerConfiguration),
                GetDestructingLimit(nameof(_maximumStringLength), loggerConfiguration),
                GetDestructingLimit(nameof(_maximumCollectionCount), loggerConfiguration)
            );
        }

        private static int GetDestructingLimit(string fieldName, LoggerConfiguration loggerConfiguration)
        {
            return (int)typeof(LoggerConfiguration)
                           .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                           .GetValue(loggerConfiguration);
        }
    }
}

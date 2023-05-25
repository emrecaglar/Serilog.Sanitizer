using System.Reflection;

namespace Serilog.Sanitizer.PropertyValueBuilder
{
    public class DestructorLimits
    {
        private readonly int _maximumDestructuringDepth;
        private readonly int _maximumStringLength;
        private readonly int _maximumCollectionCount;

        public DestructorLimits(int? maximumDestructuringDepth, int? maximumStringLength, int? maximumCollectionCount)
        {
            _maximumCollectionCount = maximumCollectionCount ?? int.MaxValue;
            _maximumDestructuringDepth = maximumDestructuringDepth ?? 10;
            _maximumStringLength = maximumStringLength ?? int.MaxValue;
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

        private static int? GetDestructingLimit(string fieldName, LoggerConfiguration loggerConfiguration)
        {
            return (int?)typeof(LoggerConfiguration)
                           .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                           .GetValue(loggerConfiguration);
        }
    }
}

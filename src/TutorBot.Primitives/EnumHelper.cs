using System.Linq.Expressions;

namespace WSS.Cryptography.Primitives.SYS.Lib.Primitives
{
    internal class EnumHelper<T>
        where T : struct, Enum, IConvertible
    {
        static readonly Type typeofT = typeof(T);
        static readonly Type underlyingType = Enum.GetUnderlyingType(typeofT);
        static readonly ParameterExpression[] parameters =
        {
        Expression.Parameter(typeofT),
        Expression.Parameter(typeofT)
    };

        static readonly Func<T, T, T> _orFunc = Expression.Lambda<Func<T, T, T>>(
            Expression.Convert(Expression.Or(
                Expression.Convert(parameters[0], underlyingType),
                Expression.Convert(parameters[1], underlyingType)
            ), typeofT), parameters).Compile();

        public static Func<T, T, T> OrFunction { get { return _orFunc; } }


        public static T CombineFlags(params T[] flags)
        {
            return flags.Select(flag => flag).Aggregate(OrFunction);
        }

        public static T[] ToEnumFlag(T value)
        {
            Type baseType = Enum.GetUnderlyingType(typeof(T));
            List<T> flags = new List<T>();
            if (baseType == typeof(int))
            {
                int x = (int)(object)value;

                for (int i = 1; i < (1 << 30); i = i << 1)
                    if ((x & i) != 0)
                        flags.Add((T)(object)i);
            }
            else if (baseType == typeof(long))
            {
                long x = (long)(object)value;

                for (long i = 1; i < (1 << 30); i = i << 1)
                    if ((x & i) != 0)
                        flags.Add((T)(object)i);
            }
            else if (baseType == typeof(ulong))
            {
                ulong x = (ulong)(object)value;

                for (ulong i = 1; i < (1 << 30); i = i << 1)
                    if ((x & i) != 0)
                        flags.Add((T)(object)i);
            }
            else if (baseType == typeof(uint))
            {
                uint x = (uint)(object)value;

                for (uint i = 1; i < (1 << 30); i = i << 1)
                    if ((x & i) != 0)
                        flags.Add((T)(object)i);
            }
            else if (baseType == typeof(byte))
            {
                int x = (int)(object)value;

                for (int i = 1; i < (1 << 30); i = i << 1)
                    if ((x & i) != 0)
                        flags.Add((T)(object)i);
            }
            else throw new NotImplementedException(baseType.Name);

            return flags.ToArray();
        }

        public static string ToStringEnumFlag(T value)
        {
            string result = ToEnumFlag(value).JoinString(", ");

            if (string.IsNullOrEmpty(result))
                result = Enum.GetValues<T>().First().ToString();

            return result;
        }

        public static T ParceEnumFlag(string values)
        {
            if (string.IsNullOrEmpty(values))
                return default;

            T result = values.Split(", ")
                .Select(x => Enum.Parse<T>(x, true))
                .Aggregate(OrFunction);

            return result;
        }
    }
}

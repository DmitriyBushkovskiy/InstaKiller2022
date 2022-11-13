using DAL.Entities;
using System.ComponentModel;

namespace Common
{
    public static class Utils
    {
        private static Func<User, string?>? _linkAvatarGenerator;
        public static T? Convert<T>(this string input)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null && converter.ConvertFromString(input) is T res)
                {
                    return res;
                }
                return default;
            }
            catch (NotSupportedException)
            {
                return default;
            }
        }
        /// <summary>
        /// ////////////////////////////////////////////////
        /// </summary>
        /// <param name="linkAvatarGenerator"></param>
        public static void SetAv(Func<User, string?> linkAvatarGenerator)
        {
            _linkAvatarGenerator = linkAvatarGenerator;
        }

        public static string get(User user)
        {
            return _linkAvatarGenerator(user);
        }
        /////////////////////////////////////////////////////////////////////
    }
}
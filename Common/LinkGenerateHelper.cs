using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class LinkGenerateHelper
    {
        public static Func<User, string?>? LinkAvatarGenerator { get; set; }
        public static Func<PostContent, string?>? LinkContentGenerator { get; set; }
    }
}

using Api.Models.Attach;
using System.ComponentModel.DataAnnotations;

namespace Api.Utils
{
    public class NotEmptyListMetadataAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            var list = value as IList<MetadataModel>;
            if (list != null)
            {
                return list.Count > 0;
            }
            return false;
        }
    }

    public class MinAgeAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            var date = value as DateTimeOffset?;
            ;
            if (date < DateTimeOffset.UtcNow.AddYears(-14))
            {
                return true;
            }
            return false;
        }
    }
}

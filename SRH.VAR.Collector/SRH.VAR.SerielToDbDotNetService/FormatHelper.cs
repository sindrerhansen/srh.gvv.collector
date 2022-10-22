using System;
using System.Linq;

namespace SRH.VAR.SerielToDbDotNetService
{
    public class FormatHelper
    {
        public static bool StringToBoolean(String str)
        {
            return StringToBoolean(str, false);
        }

        public static bool StringToBoolean(String str, bool bDefault)
        {
            String[] BooleanStringOff = { "0", "off", "no" };
            String[] BooleanStringOn = { "1", "on", "yes" };

            if (String.IsNullOrEmpty(str))
                return bDefault;
            else if (BooleanStringOff.Contains(str, StringComparer.InvariantCultureIgnoreCase))
                return false;
            else if (BooleanStringOn.Contains(str, StringComparer.InvariantCultureIgnoreCase))
                return true;

            if (!bool.TryParse(str, out bool result))
                result = true;

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bermuda.Enums
{

    // These enums need to be in the correct order as the backend as we are only sending over the decimal representation of this over.
    public enum SupportedSitesEnum
    {
        FootlockerUS,
        FootlockerCA,
        Eastbay,
        Champs,
        Footaction,
        KidsFootlocker
    }

    public enum StatusEnum
    {
        Stopped,
        Working,
        Failed,
        Success
    }

}

using Robust.Shared.Configuration;

namespace Content.Omu.Common.CCVar;

[CVarDefs]
public sealed partial class OmuCVars
{
    public static readonly CVarDef<bool> AlternateJobTitles =
        CVarDef.Create("omu.alternate_job_titles", true, CVar.SERVER | CVar.REPLICATED);
}

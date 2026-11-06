// SPDX-License-Identifier: MIT
using Robust.Shared.Configuration;

namespace Content.Goobstation.Common._MACRO.CCVars;

[CVarDefs]
public sealed class MacroCCVars
{
    /// <summary>
    ///     The prototype to use for announcer weights.
    /// </summary>
    public static readonly CVarDef<string> AnnouncerWeightPrototype =
        CVarDef.Create("macro.announcer_weight_prototype", "Announcers", CVar.SERVERONLY);
}

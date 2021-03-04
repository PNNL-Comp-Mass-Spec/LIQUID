// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:LiquidBackend.Util.FitUtilBase.GetAllIsotopePeaks(InformedProteomics.Backend.Data.Spectrometry.Spectrum,System.Collections.Generic.IReadOnlyCollection{System.Double},System.Double,InformedProteomics.Backend.Data.Spectrometry.Tolerance)~InformedProteomics.Backend.Data.Spectrometry.Peak[]")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:LiquidBackend.Util.LipidUtil.GetAllIsotopePeaks(InformedProteomics.Backend.Data.Spectrometry.Spectrum,InformedProteomics.Backend.Data.Composition.Composition,InformedProteomics.Backend.Data.Spectrometry.Tolerance,System.Double)~InformedProteomics.Backend.Data.Spectrometry.Peak[]")]
[assembly: SuppressMessage("Usage", "RCS1246:Use element access.", Justification = "Prefer to use .First()", Scope = "member", Target = "~M:LiquidBackend.Util.LipidUtil.CreateMsMsSearchUnits(System.String,System.Double,LiquidBackend.Domain.LipidClass,LiquidBackend.Domain.FragmentationMode,System.Collections.Generic.List{LiquidBackend.Domain.AcylChain})~System.Collections.Generic.List{LiquidBackend.Domain.MsMsSearchUnit}")]

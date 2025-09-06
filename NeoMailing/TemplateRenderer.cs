using Scriban;
using Scriban.Runtime;
using System;
using System.Collections.Concurrent;

namespace NeoMailing;

/// <summary>
/// Provides methods for rendering Scriban templates with caching.
/// </summary>
public static class TemplateRenderer
{
    private static readonly ConcurrentDictionary<string, Template> _cache = new();

    /// <summary>
    /// Renders a template with the given model.
    /// </summary>
    public static string Render(string templateText, object model, MemberRenamerDelegate? memberRenamer = null)
    {
        var tpl = _cache.GetOrAdd(templateText, t => Template.Parse(t));
        return tpl.Render(model, memberRenamer ?? (m => m.Name));
    }

    /// <summary>
    /// Renders a template using a full <see cref="TemplateContext"/>.
    /// </summary>
    public static string Render(
        string templateText, object model, Action<TemplateContext>? configureContext)
    {
        var tpl = _cache.GetOrAdd(templateText, t => Template.Parse(t));

        // Put model into a ScriptObject
        var globals = new ScriptObject();
        globals.Import(model); // default: keep original member names

        var ctx = new TemplateContext();
        ctx.PushGlobal(globals);

        // Let caller tweak StrictVariables, LoopLimit, etc.
        configureContext?.Invoke(ctx);

        return tpl.Render(ctx); // <-- note: context overload
    }

    /// <summary>
    /// Renders a template with case-insensitive member names.
    /// </summary>
    public static string RenderCaseInsensitive(string templateText, object model)
    {
        var tpl = _cache.GetOrAdd(templateText, t => Template.Parse(t));

        var globals = new ScriptObject();
        globals.Import(model); // original names
        globals.Import(model, renamer: m => m.Name.ToLowerInvariant()); // lower-cased duplicates

        var ctx = new TemplateContext();
        ctx.PushGlobal(globals);

        return tpl.Render(ctx);
    }
}

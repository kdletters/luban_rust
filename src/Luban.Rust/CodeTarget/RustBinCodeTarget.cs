using Luban.CodeTarget;
using Luban.CSharp.TemplateExtensions;
using Luban.Rust.CodeTarget;
using Luban.Tmpl;
using Scriban;

namespace Luban.CSharp.CodeTarget;

[CodeTarget("rust-bin")]
public class RustBinCodeTarget : RustCodeTargetBase
{
    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new RustBinTemplateExtension());
    }

    public override void Handle(GenerationContext ctx, OutputFileManifest manifest)
    {
        base.Handle(ctx, manifest);
        GenerateLubanLib(ctx, manifest);
    }

    protected virtual void GenerateLubanLib(GenerationContext ctx, OutputFileManifest manifest)
    {
        var template = TemplateManager.Ins.GetTemplateString($"{CommonTemplateSearchPath}/luban_lib/Cargo.toml");
        var path = $"luban_lib/Cargo.toml";
        manifest.AddFile(CreateOutputFile(path, template));
        template = TemplateManager.Ins.GetTemplateString($"{CommonTemplateSearchPath}/luban_lib/src/lib.rs");
        path = $"luban_lib/src/lib.rs";
        manifest.AddFile(CreateOutputFile(path, template));
    }
}
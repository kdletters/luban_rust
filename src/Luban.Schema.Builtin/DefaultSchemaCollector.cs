using Luban.RawDefs;
using Luban.Utils;

namespace Luban.Schema.Builtin;

[SchemaCollector("default")]
public class DefaultSchemaCollector : SchemaCollectorBase
{
    private static readonly NLog.Logger s_logger = NLog.LogManager.GetCurrentClassLogger();

    public override void Load(string schemaPath)
    {
        var rootLoader = (IRootSchemaLoader)SchemaLoaderManager.Ins.Create(FileUtil.GetExtensionWithDot(schemaPath), "root", this);
        rootLoader.Load(schemaPath);

        foreach (var importFile in rootLoader.ImportFiles)
        {
            s_logger.Debug("import schema file:{} type:{}", importFile.FileName, importFile.Type);
            var schemaLoader = SchemaLoaderManager.Ins.Create(FileUtil.GetExtensionWithDot(importFile.FileName), importFile.Type, this);
            schemaLoader.Load(importFile.FileName);
        }
        
        LoadTableValueTypeSchemasFromFile();
    }
    
    private void LoadTableValueTypeSchemasFromFile()
    {
        var tasks = new List<Task>();
        string beanSchemaLoaderName =
            GenerationContext.CurrentArguments.TryGetOption("schemaCollector", "beanSchemaLoader", true,
                out var loaderName)
                ? loaderName
                : "default";
        foreach (var table in Tables.Where(t => t.ReadSchemaFromFile))
        {
            tasks.Add(Task.Run(() =>
            {
                string fileName = table.InputFiles[0];
                IBeanSchemaLoader schemaLoader = SchemaLoaderManager.Ins.CreateBeanSchemaLoader(beanSchemaLoaderName);
                string fullPath = $"{GenerationContext.CurrentArguments.GetInputDataPath()}/{fileName}";
                RawBean bean = schemaLoader.Load(fullPath, table.ValueType);
                Add(bean);
            }));
        }
        Task.WaitAll(tasks.ToArray());
    }
}
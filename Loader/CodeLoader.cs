using System.Reflection;

namespace ET;

internal class CodeLoader : Singleton<CodeLoader>, ISingletonAwake
{
    private readonly Dictionary<string, Assembly> assemblies = new();
    public IReadOnlyCollection<Assembly> Assemblies => assemblies.Values;
    
    public Assembly this[string name] => assemblies[name];
    
    public void Awake()
    {
        // 不热更情况下用这些代码, 会导致加载的Type与Loader引用到的Type引用不同, 例如(type == typeof(XXX)) false
        /*byte[] modelDllBytes = File.ReadAllBytes(Define.ModelDll);
        byte[] modelPdbBytes = File.ReadAllBytes(Define.ModelPdb);
        byte[] modelViewDllBytes = File.ReadAllBytes(Define.ModelViewDll);
        byte[] modelViewPdbBytes = File.ReadAllBytes(Define.ModelViewPdb);
        byte[] hotfixDllBytes = File.ReadAllBytes(Define.HotfixDll);
        byte[] hotfixPdbBytes = File.ReadAllBytes(Define.HotfixPdb);
        byte[] hotfixViewDllBytes = File.ReadAllBytes(Define.HotfixViewDll);
        byte[] hotfixViewPdbBytes = File.ReadAllBytes(Define.HotfixViewPdb);
        
        assemblies[Define.Model] = Assembly.Load(modelDllBytes, modelPdbBytes);
        assemblies[Define.ModelView] = Assembly.Load(modelViewDllBytes, modelViewPdbBytes);
        assemblies[Define.Hotfix] = Assembly.Load(hotfixDllBytes, hotfixPdbBytes);
        assemblies[Define.HotfixView] = Assembly.Load(hotfixViewDllBytes, hotfixViewPdbBytes);*/

        this.assemblies["Model"] = Assembly.LoadFrom("Model.dll");
        this.assemblies["ModelView"] = Assembly.LoadFrom("ModelView.dll");
        this.assemblies["Hotfix"] = Assembly.LoadFrom("Hotfix.dll");
        this.assemblies["HotfixView"] = Assembly.LoadFrom("HotfixView.dll");
        if (Options.IsEditor)
        {
            this.assemblies["Editor"] = Assembly.LoadFrom("Editor.dll");
        }
    }
}
using System.Globalization;

using CsvHelper;

using CsvHelper.Configuration;

using WinInventory.Models;
 
namespace WinInventory.Services;
 
public class CatalogService

{

    private readonly IWebHostEnvironment _env;

    private List<MacBookSpec>? _cache;
 
    public CatalogService(IWebHostEnvironment env) => _env = env;
 
    public IReadOnlyList<MacBookSpec> GetMacCatalog()

    {

        if (_cache is not null) return _cache;
 
        var root = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");

        var path = Path.Combine(root, "data", "macbooks.csv");

        Console.WriteLine($"[Catalog] CSV path: {path}  Exists={File.Exists(path)}");
 
        var list = new List<MacBookSpec>();

        if (!File.Exists(path))

        {

            _cache = list;

            return _cache;

        }
 
        var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)

        {

            HasHeaderRecord = true,

            TrimOptions = TrimOptions.Trim,

            DetectDelimiter = true,

            PrepareHeaderForMatch = args => args.Header.ToLowerInvariant(),

            MissingFieldFound = null,        // be tolerant

            BadDataFound = null

        };
 
        using var reader = new StreamReader(path);

        using var csv = new CsvReader(reader, cfg);
 
        // IMPORTANT: read header BEFORE using GetField("name")

        if (!csv.Read())

        {

            _cache = list;

            return _cache;

        }

        csv.ReadHeader();
 
        while (csv.Read())

        {

            // Use TryGetField so missing cells don't crash

            string GetS(string name) => csv.TryGetField(name, out string? v) ? (v ?? "") : "";

            int GetI(string name) => csv.TryGetField(name, out int v) ? v : 0;

            double GetD(string name) => csv.TryGetField(name, out double v) ? v : 0.0;
 
            var m = new MacBookSpec

            {

                Model         = GetS("model"),

                Chip          = GetS("chip"),

                CoresCpu      = GetI("cores_cpu"),

                CoresGpu      = GetI("cores_gpu"),

                RamGb         = GetI("ram_gb"),

                StorageGb     = GetI("storage_gb"),

                DisplayInches = GetD("display_inches"),

                DisplayNits   = GetI("display_nits"),

                RefreshHz     = GetI("refresh_hz"),

                WeightKg      = GetD("weight_kg"),

                Ports         = GetS("ports"),

                MsrpAed       = GetI("msrp_aed"),

                LaunchDate    = DateTime.TryParse(GetS("launch_date"), out var dt) ? dt : DateTime.MinValue,

                BatteryWh     = GetD("battery_wh"),

                Wifi          = GetS("wifi")

            };

            if (!string.IsNullOrWhiteSpace(m.Model))

                list.Add(m);

        }
 
        _cache = list;

        return _cache;

    }

}

 
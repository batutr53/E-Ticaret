using System.IO.Compression;
using System.Text.Json;
using E_Ticaret.Core.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

namespace E_Ticaret.Data.ReferenceData;

public sealed class AddressReferenceCatalog
{
    public List<AddressProvince> Provinces { get; set; } = new();
    public List<AddressDistrict> Districts { get; set; } = new();
    public List<AddressNeighborhood> Neighborhoods { get; set; } = new();

    public static AddressReferenceCatalog Load()
    {
        using var resource = typeof(AddressReferenceCatalog).Assembly.GetManifestResourceStream("AddressReferenceCatalog.2025.json.gz")
            ?? throw new InvalidOperationException("Adres referans verisi bulunamadı.");
        using var stream = new GZipStream(resource, CompressionMode.Decompress);
        return JsonSerializer.Deserialize<AddressReferenceCatalog>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    // Only the new reference tables are seeded. Existing customer and financial records are untouched.
    public static void Insert(MigrationBuilder migration)
    {
        var data = Load();
        migration.InsertData("AddressProvinces", new[] { "Id", "Name" }, Rows(data.Provinces.Select(x => new object[] { x.Id, x.Name })));
        migration.InsertData("AddressDistricts", new[] { "Id", "ProvinceId", "Name" }, Rows(data.Districts.Select(x => new object[] { x.Id, x.ProvinceId, x.Name })));
        foreach (var batch in data.Neighborhoods.Chunk(500))
            migration.InsertData("AddressNeighborhoods", new[] { "Id", "DistrictId", "Name", "LocalityName" },
                Rows(batch.Select(x => new object[] { x.Id, x.DistrictId, x.Name, x.LocalityName! })));
    }

    private static object[,] Rows(IEnumerable<object[]> source)
    {
        var rows = source.ToArray();
        var result = new object[rows.Length, rows[0].Length];
        for (var i = 0; i < rows.Length; i++)
            for (var j = 0; j < rows[i].Length; j++) result[i, j] = rows[i][j];
        return result;
    }
}
